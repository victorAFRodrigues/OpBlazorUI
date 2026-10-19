using System.Reflection;
using System.Text.RegularExpressions;

namespace OpBlazorUI.DocsGen;

internal static class CatalogBuilder
{
    private static readonly Regex TypeTokenRegex = new("\\bOp[A-Za-z0-9]+", RegexOptions.Compiled);

    public static IReadOnlyList<CatalogEntry> Build(RepoContext repo, Overrides overrides)
    {
        var assemblies = new[]
        {
            Assembly.Load("OpBlazorUI"),
            Assembly.Load("OpBlazorUI.Charts")
        };

        var typesByName = assemblies
            .SelectMany(a => a.GetExportedTypes())
            .Where(t => t.IsClass)
            .GroupBy(t => StripArity(t.Name), StringComparer.Ordinal)
            .ToDictionary(
                g => g.Key,
                g => g.OrderBy(t => t.IsGenericTypeDefinition ? 1 : 0).ThenBy(t => t.Name).ToList(),
                StringComparer.Ordinal);

        var menu = AppMenuParser.Parse(repo.AppMenuPath);
        var entries = new List<CatalogEntry>();
        var problems = new List<string>();

        foreach (var item in menu.Where(i => i.Status != "coming-soon"))
        {
            overrides.ComponentOverrides.TryGetValue(item.Label, out var componentOverride);
            overrides.KindOverrides.TryGetValue(item.Label, out var forcedKind);

            var pageFile = componentOverride?.Page ?? GuessPageFile(item.Label);
            var pagePath = Path.Combine(repo.PagesDir, pageFile);

            IReadOnlyList<string> typeNames = componentOverride?.Types
                                              ?? (componentOverride?.Type is { } single
                                                  ? new[] { single }
                                                  : new[] { GuessTypeName(item.Label) });

            var types = new List<Type>();
            foreach (var name in typeNames)
            {
                if (typesByName.TryGetValue(name, out var matches))
                {
                    types.Add(matches[0]);
                }
            }

            var kind = forcedKind
                       ?? (types.Count == 0 ? "guide" : types.Any(RepoContext.IsComponent) ? "component" : "service");

            if (kind == "component" && types.Count == 0)
            {
                problems.Add($"{item.Label}: sem tipo resolvido para o componente (candidato '{GuessTypeName(item.Label)}').");
            }

            if (!File.Exists(pagePath))
            {
                problems.Add($"{item.Label}: página não encontrada ({pageFile}).");
            }

            var aliases = BuildAliases(item.Label, componentOverride?.Aliases);

            var entry = new CatalogEntry
            {
                Label = item.Label,
                Id = componentOverride?.Id ?? RepoContext.Slug(item.Label),
                Route = item.Route,
                Category = item.Category,
                Kind = kind,
                Status = item.Status,
                PageFile = pageFile,
                PagePath = pagePath,
                Aliases = aliases
            };
            entry.Types.AddRange(types);
            entries.Add(entry);
        }

        // Tipos extras documentados em seções com o nome do tipo (ex.: OpAccordionPanel, OpStep)
        // ou no Caption de uma tabela (ex.: OtpTokenContext, OpVirtualScrollerItemContext).
        foreach (var entry in entries.Where(e => File.Exists(e.PagePath)))
        {
            var page = PageParser.Parse(entry.PagePath);
            foreach (var section in page.Sections)
            {
                foreach (Match token in TypeTokenRegex.Matches(section.Label))
                {
                    AddType(entry, token.Value, typesByName);
                }
            }

            foreach (var table in page.ApiTables)
            {
                var caption = System.Net.WebUtility.HtmlDecode(table.Caption);
                var generic = caption.IndexOf('<');
                if (generic > 0)
                {
                    caption = caption[..generic];
                }

                AddType(entry, caption.Trim(), typesByName);
            }
        }

        // Ids duplicados quebrariam o manifesto e os nomes de arquivo.
        var duplicate = entries.GroupBy(e => e.Id, StringComparer.Ordinal).FirstOrDefault(g => g.Count() > 1);
        if (duplicate is not null)
        {
            problems.Add($"id duplicado: '{duplicate.Key}' ({string.Join(", ", duplicate.Select(e => e.Label))}).");
        }

        if (problems.Count > 0)
        {
            throw new InvalidOperationException(
                "Catálogo inconsistente:\n  - " + string.Join("\n  - ", problems));
        }

        return entries;
    }

    private static void AddType(
        CatalogEntry entry,
        string name,
        IReadOnlyDictionary<string, List<Type>> typesByName)
    {
        if (name.Length > 0 && typesByName.TryGetValue(name, out var matches) && !entry.Types.Contains(matches[0]))
        {
            entry.Types.Add(matches[0]);
        }
    }

    private static string GuessPageFile(string label) =>
        label.Replace(" ", "", StringComparison.Ordinal) + ".razor";

    private static string GuessTypeName(string label)
    {
        var chars = label.Where(char.IsLetterOrDigit).ToArray();
        return "Op" + new string(chars);
    }

    private static string StripArity(string name)
    {
        var tick = name.IndexOf('`');
        return tick >= 0 ? name[..tick] : name;
    }

    private static IReadOnlyList<string> BuildAliases(string label, string[]? extra)
    {
        var aliases = new List<string>
        {
            label.ToLowerInvariant()
        };

        var spaced = Regex.Replace(label, "([a-z0-9])([A-Z])", "$1 $2").ToLowerInvariant();
        if (spaced != aliases[0])
        {
            aliases.Add(spaced);
        }

        if (extra is not null)
        {
            aliases.AddRange(extra);
        }

        return aliases
            .Where(a => !string.IsNullOrWhiteSpace(a))
            .Select(a => a.Trim().ToLowerInvariant())
            .Distinct(StringComparer.Ordinal)
            .ToArray();
    }
}
