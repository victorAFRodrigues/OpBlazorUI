using System.Net;
using System.Text.RegularExpressions;

namespace OpBlazorUI.DocsGen;

/// <summary>
/// Extrai das páginas do Showcase o que não existe em metadados: descrições das
/// tabelas de API, classes CSS documentadas e os exemplos de código.
/// </summary>
internal static class PageParser
{
    private static readonly Regex AttributeRegex = new(
        "([A-Za-z_][A-Za-z0-9_]*)\\s*=\\s*\"([^\"]*)\"",
        RegexOptions.Compiled);

    private static readonly Regex HeadersRegex = new(
        "Headers\\s*=\\s*\"@\\(new\\[\\]\\s*\\{([^}]*)\\}\\)\"",
        RegexOptions.Compiled | RegexOptions.Singleline);

    private static readonly Regex RowsAttrRegex = new(
        "Rows\\s*=\\s*\"@\\(new\\[\\]\\s*\\{",
        RegexOptions.Compiled);

    private static readonly Regex CaptionRegex = new(
        "Caption\\s*=\\s*\"([^\"]*)\"",
        RegexOptions.Compiled);

    private static readonly Regex StringLiteralRegex = new(
        "\"((?:[^\"\\\\]|\\\\.)*)\"",
        RegexOptions.Compiled);

    private static readonly Regex ConstRegex = new(
        "private const string (\\w+)\\s*=\\s*\"\"\"(.*?)\"\"\";",
        RegexOptions.Compiled | RegexOptions.Singleline);

    private static readonly Regex CodeBlockRegex = new(
        "<CodeBlock\\b([^>]*)>",
        RegexOptions.Compiled);

    private static readonly Regex CodeRefRegex = new(
        "\\bCode\\s*=\\s*\"@(\\w+)\"",
        RegexOptions.Compiled);

    private static readonly Regex LanguageRegex = new(
        "\\bLanguage\\s*=\\s*\"([^\"]*)\"",
        RegexOptions.Compiled);

    private static readonly Regex TypeTokenRegex = new(
        "Op[A-Za-z0-9_]+",
        RegexOptions.Compiled);

    public static PageDoc Parse(string path)
    {
        var doc = new PageDoc
        {
            File = Path.GetFileName(path),
            Raw = File.ReadAllText(path)
        };

        var text = doc.Raw;

        var docComponent = FindElement(text, "<DocComponent", out var docComponentTag);
        if (docComponent >= 0)
        {
            var attrs = ReadAttributes(docComponentTag);
            doc.Title = attrs.GetValueOrDefault("Title", "");
            doc.Description = attrs.TryGetValue("Description", out var description)
                ? WebUtility.HtmlDecode(description).Trim()
                : null;
        }
        else
        {
            doc.Warnings.Add("sem <DocComponent>");
        }

        var apiStart = text.IndexOf("<Api>", StringComparison.Ordinal);
        var apiEnd = apiStart >= 0 ? text.IndexOf("</Api>", apiStart, StringComparison.Ordinal) : -1;
        if (apiEnd < 0)
        {
            apiStart = -1;
        }

        // Seções: posição de cada <DocSection ...> até a próxima.
        var sectionStarts = new List<(int Start, int TagEnd, Dictionary<string, string> Attrs)>();
        var cursor = 0;
        while (true)
        {
            var index = text.IndexOf("<DocSection", cursor, StringComparison.Ordinal);
            if (index < 0)
            {
                break;
            }

            var tagEnd = FindTagEnd(text, index);
            if (tagEnd < 0)
            {
                break;
            }

            var tag = text[index..(tagEnd + 1)];
            sectionStarts.Add((index, tagEnd, ReadAttributes(tag)));
            cursor = tagEnd + 1;
        }

        for (var i = 0; i < sectionStarts.Count; i++)
        {
            var current = sectionStarts[i];
            var end = i + 1 < sectionStarts.Count ? sectionStarts[i + 1].Start : text.Length;
            var label = current.Attrs.GetValueOrDefault("Label", "");
            var id = current.Attrs.GetValueOrDefault("Id") ?? Slug(label);
            doc.Sections.Add(new SectionDoc
            {
                Label = label,
                Id = id,
                Start = current.Start,
                End = end,
                InsideApi = apiStart >= 0 && current.Start > apiStart && current.Start < apiEnd
            });
        }

        // Tabelas (todas; as de API ficam marcadas por InsideApi).
        // Delimitar pelo próximo <ApiTable evita o problema de escanear '>' dentro
        // dos literais C# das células (ex.: <code>...</code>) — o markup Razor não
        // segue a regra de aspas do HTML.
        cursor = 0;
        while (true)
        {
            var index = text.IndexOf("<ApiTable", cursor, StringComparison.Ordinal);
            if (index < 0)
            {
                break;
            }

            var next = text.IndexOf("<ApiTable", index + 1, StringComparison.Ordinal);
            var sliceEnd = next < 0 ? text.Length : next;
            var tag = text[index..sliceEnd];
            cursor = sliceEnd;

            var headers = ParseHeaders(tag);
            if (headers.Count == 0)
            {
                doc.Warnings.Add($"ApiTable sem Headers em {index}");
                continue;
            }

            var section = doc.Sections.LastOrDefault(s => s.Start <= index);
            var captionMatch = CaptionRegex.Match(tag);
            var table = new ApiTableDoc
            {
                SectionLabel = section?.Label ?? "",
                SectionId = section?.Id ?? "",
                Caption = captionMatch.Success ? captionMatch.Groups[1].Value : "",
                Headers = headers,
                InsideApi = apiStart >= 0 && index > apiStart && index < apiEnd
            };

            foreach (var row in ParseRows(tag))
            {
                table.Rows.Add(row);
            }

            doc.AllTables.Add(table);
            if (table.InsideApi)
            {
                doc.ApiTables.Add(table);
            }

            if (headers.Count > 0 && headers[0].Equals("Classe", StringComparison.OrdinalIgnoreCase))
            {
                foreach (var row in table.Rows)
                {
                    if (row.Length < 2)
                    {
                        continue;
                    }

                    var name = row[0].Trim();
                    var description = RepoContext.StripHtml(row[^1]);
                    if (name.Length > 0)
                    {
                        doc.ThemingClassDescriptions[name.TrimStart('.')] = description;
                    }
                }
            }
        }

        ParseExamples(doc, text);
        return doc;
    }

    /// <summary>
    /// Tabelas que documentam um tipo. O nome precisa casar como token exato:
    /// "OpPaginatorState" não pode casar com "OpPaginator". Tipos extras podem ser
    /// identificados pelo label da seção ou pelo Caption da tabela (ex.: OtpTokenContext).
    /// </summary>
    public static IReadOnlyList<ApiTableDoc> RowsForType(PageDoc page, Type type, bool isPrimary)
    {
        var name = RepoContext.ShortName(type);
        var exact = page.ApiTables
            .Where(t => t.InsideApi &&
                        (HasTypeName(t.SectionLabel, name) || HasTypeName(RepoContext.StripHtml(t.Caption), name)))
            .ToList();

        if (!isPrimary)
        {
            return exact;
        }

        // Seções genéricas ("Propriedades", "Eventos", "Templates") pertencem ao tipo
        // primário; seções com outro token de tipo pertencem a tipos extras.
        var generic = page.ApiTables
            .Where(t => t.InsideApi && !TypeTokenRegex.IsMatch(t.SectionLabel))
            .ToList();

        return exact.Concat(generic).Distinct().ToList();
    }

    private static bool HasTypeName(string text, string name) =>
        Regex.IsMatch(text, $@"(?<![A-Za-z0-9_]){Regex.Escape(name)}(?![A-Za-z0-9_])");

    private static void ParseExamples(PageDoc doc, string text)
    {
        var examples = new Dictionary<string, ExampleDoc>(StringComparer.Ordinal);
        foreach (Match match in ConstRegex.Matches(text))
        {
            var name = match.Groups[1].Value;
            examples[name] = new ExampleDoc
            {
                ConstName = name,
                Code = Dedent(match.Groups[2].Value)
            };
        }

        foreach (var section in doc.Sections)
        {
            var raw = text[section.Start..section.End];
            foreach (Match block in CodeBlockRegex.Matches(raw))
            {
                var attributes = block.Groups[1].Value;
                var reference = CodeRefRegex.Match(attributes);
                if (!reference.Success)
                {
                    continue;
                }

                var name = reference.Groups[1].Value;
                if (!examples.TryGetValue(name, out var example))
                {
                    continue;
                }

                if (example.SectionId is null)
                {
                    example.SectionId = section.Id;
                    example.SectionLabel = section.Label;
                }

                var language = LanguageRegex.Match(attributes);
                if (language.Success && example.Language is null && language.Groups[1].Value.Length > 0)
                {
                    example.Language = language.Groups[1].Value;
                }
            }
        }

        doc.Examples.AddRange(examples.Values.Where(e => e.Code.Length > 0));
    }

    private static List<string> ParseHeaders(string tag)
    {
        var match = HeadersRegex.Match(tag);
        if (!match.Success)
        {
            return [];
        }

        return StringLiteralRegex.Matches(match.Groups[1].Value)
            .Select(m => Unescape(m.Groups[1].Value))
            .ToList();
    }

    private static IEnumerable<string[]> ParseRows(string tag)
    {
        var match = RowsAttrRegex.Match(tag);
        if (!match.Success)
        {
            yield break;
        }

        // O match termina na chave que abre o array. Escanear com consciência de
        // strings é obrigatório: células como "{value}"/"{0}" quebram regex de chaves.
        var openBrace = match.Index + match.Length - 1;
        var end = FindBlockEnd(tag, openBrace);
        if (end < 0)
        {
            yield break;
        }

        foreach (var row in ExtractRowBlocks(tag[(openBrace + 1)..end]))
        {
            var cells = StringLiteralRegex.Matches(row)
                .Select(m => Unescape(m.Groups[1].Value))
                .ToArray();
            if (cells.Length > 0)
            {
                yield return cells;
            }
        }
    }

    private static IEnumerable<string> ExtractRowBlocks(string rowsText)
    {
        var i = 0;
        while (i < rowsText.Length)
        {
            if (rowsText[i] == '"')
            {
                i = SkipString(rowsText, i);
                continue;
            }

            if (rowsText.AsSpan(i).StartsWith("new[]", StringComparison.Ordinal))
            {
                var brace = rowsText.IndexOf('{', i + 5);
                if (brace < 0)
                {
                    yield break;
                }

                var end = FindBlockEnd(rowsText, brace);
                if (end < 0)
                {
                    yield break;
                }

                yield return rowsText[(brace + 1)..end];
                i = end + 1;
                continue;
            }

            i++;
        }
    }

    /// <summary>Índice da chave que fecha o bloco, ignorando chaves dentro de strings.</summary>
    private static int FindBlockEnd(string text, int openBrace)
    {
        var depth = 0;
        var inString = false;
        var escaped = false;
        for (var i = openBrace; i < text.Length; i++)
        {
            var c = text[i];
            if (inString)
            {
                if (escaped)
                {
                    escaped = false;
                }
                else if (c == '\\')
                {
                    escaped = true;
                }
                else if (c == '"')
                {
                    inString = false;
                }

                continue;
            }

            if (c == '"')
            {
                inString = true;
            }
            else if (c == '{')
            {
                depth++;
            }
            else if (c == '}')
            {
                depth--;
                if (depth == 0)
                {
                    return i;
                }
            }
        }

        return -1;
    }

    private static int SkipString(string text, int quote)
    {
        var escaped = false;
        for (var i = quote + 1; i < text.Length; i++)
        {
            var c = text[i];
            if (escaped)
            {
                escaped = false;
            }
            else if (c == '\\')
            {
                escaped = true;
            }
            else if (c == '"')
            {
                return i + 1;
            }
        }

        return text.Length;
    }

    private static Dictionary<string, string> ReadAttributes(string tag)
    {
        var attrs = new Dictionary<string, string>(StringComparer.Ordinal);
        foreach (Match match in AttributeRegex.Matches(tag))
        {
            // O primeiro nome de uma tag é o elemento; o regex exige '=', então não colide.
            attrs[match.Groups[1].Value] = match.Groups[2].Value;
        }

        return attrs;
    }

    private static int FindElement(string text, string tagName, out string tag)
    {
        var index = text.IndexOf(tagName, StringComparison.Ordinal);
        if (index < 0)
        {
            tag = "";
            return -1;
        }

        var end = FindTagEnd(text, index);
        tag = end < 0 ? "" : text[index..(end + 1)];
        return index;
    }

    /// <summary>Fim do '>' da tag, ignorando '>' dentro de atributos entre aspas.</summary>
    private static int FindTagEnd(string text, int start)
    {
        char quote = '\0';
        for (var i = start; i < text.Length; i++)
        {
            var c = text[i];
            if (quote != '\0')
            {
                if (c == quote)
                {
                    quote = '\0';
                }

                continue;
            }

            if (c is '"' or '\'')
            {
                quote = c;
            }
            else if (c == '>')
            {
                return i;
            }
        }

        return -1;
    }

    private static string Unescape(string value) => value
        .Replace("\\\"", "\"")
        .Replace("\\n", "\n")
        .Replace("\\r", "\r")
        .Replace("\\t", "\t")
        .Replace("\\\\", "\\");

    private static string Dedent(string raw)
    {
        var lines = raw.Replace("\r\n", "\n").Split('\n');
        if (lines.Length > 0 && lines[0].Trim().Length == 0)
        {
            lines = lines[1..];
        }

        if (lines.Length > 0 && lines[^1].Trim().Length == 0)
        {
            lines = lines[..^1];
        }

        var indent = lines
            .Where(l => l.Trim().Length > 0)
            .Select(l => l.Length - l.TrimStart().Length)
            .DefaultIfEmpty(0)
            .Min();

        return string.Join("\n", lines.Select(l => l.Length >= indent ? l[indent..] : l)).TrimEnd();
    }

    private static string Slug(string label) => label
        .ToLowerInvariant()
        .Replace(" ", "-", StringComparison.Ordinal);
}
