using System.Text.RegularExpressions;

namespace OpBlazorUI.DocsGen;

/// <summary>
/// Lê o DocTheming.cs do Showcase: classes CSS e design tokens por componente.
/// O arquivo é mantido à mão, no formato `["id"] = [ ... ],` (uma entrada por linha),
/// o que permite leitura linha a linha sem interpretar C#.
/// </summary>
internal sealed class ThemingData
{
    private enum Section
    {
        None,
        Classes,
        Tokens
    }

    private static readonly Regex EntryRegex = new(
        "^\\s*\\[\"([^\"]+)\"\\]\\s*=\\s*\\[(.*)\\],?\\s*$",
        RegexOptions.Compiled);

    private static readonly Regex ClassRegex = new("\"(\\.p-[^\"]+)\"", RegexOptions.Compiled);

    private static readonly Regex TokenRegex = new("\\(\"([^\"]+)\",\\s*\"([^\"]*)\"\\)", RegexOptions.Compiled);

    public Dictionary<string, string[]> Classes { get; } = new(StringComparer.OrdinalIgnoreCase);

    public Dictionary<string, (string Var, string Value)[]> Tokens { get; } = new(StringComparer.OrdinalIgnoreCase);

    public static ThemingData Load(string path)
    {
        var data = new ThemingData();
        var section = Section.None;

        foreach (var line in File.ReadLines(path))
        {
            if (line.Contains("Classes = new", StringComparison.Ordinal))
            {
                section = Section.Classes;
                continue;
            }

            if (line.Contains("Tokens = new", StringComparison.Ordinal))
            {
                section = Section.Tokens;
                continue;
            }

            var match = EntryRegex.Match(line);
            if (!match.Success)
            {
                continue;
            }

            var key = match.Groups[1].Value;
            var content = match.Groups[2].Value;

            if (section == Section.Tokens)
            {
                var tuples = TokenRegex.Matches(content)
                    .Select(m => (Var: m.Groups[1].Value, Value: m.Groups[2].Value))
                    .Distinct()
                    .ToArray();
                if (tuples.Length > 0)
                {
                    data.Tokens[key] = tuples;
                }
            }
            else if (content.Contains(".p-", StringComparison.Ordinal))
            {
                data.Classes[key] = ClassRegex.Matches(content)
                    .Select(m => m.Groups[1].Value)
                    .Distinct(StringComparer.Ordinal)
                    .ToArray();
            }
        }

        return data;
    }

    public string[] ClassesFor(string id, string label)
    {
        if (Classes.TryGetValue(id, out var byId))
        {
            return byId;
        }

        return Classes.TryGetValue(label, out var byLabel) ? byLabel : [];
    }

    public (string Var, string Value)[] TokensFor(string id, string label)
    {
        if (Tokens.TryGetValue(id, out var byId))
        {
            return byId;
        }

        return Tokens.TryGetValue(label, out var byLabel) ? byLabel : [];
    }
}
