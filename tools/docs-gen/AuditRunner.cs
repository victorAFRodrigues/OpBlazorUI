using System.Net;
using System.Reflection;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Components;

namespace OpBlazorUI.DocsGen;

/// <summary>
/// Confere as tabelas de API escritas à mão nas páginas contra a reflexão dos
/// componentes. Hard = prop real sem doc / linha que não existe no código.
/// Soft = divergência de tipo e parâmetros herdados.
/// </summary>
internal static class AuditRunner
{
    private static readonly Regex TypeAliasRegex = new("\\s+", RegexOptions.Compiled);

    public static AuditReport Run(
        IReadOnlyList<CatalogEntry> catalog,
        Overrides overrides,
        string mode,
        string version)
    {
        var issues = new List<AuditIssue>();
        var componentCount = 0;
        var alwaysIgnore = overrides.Audit.AlwaysIgnoreSet;

        foreach (var entry in catalog.Where(e => e.Kind == "component"))
        {
            componentCount++;
            var page = PageParser.Parse(entry.PagePath);
            var ignored = overrides.Audit.Ignore.TryGetValue(entry.Label, out var perComponent)
                ? new HashSet<string>(perComponent, StringComparer.OrdinalIgnoreCase)
                : new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            var rows = page.ApiTables
                .Where(t => t.Headers.Count > 0 && t.Headers[0].Equals("Nome", StringComparison.OrdinalIgnoreCase))
                .SelectMany(t => t.Rows.Select(cells => (Table: t, Cells: cells)))
                .Where(r => r.Cells.Length > 0)
                .ToList();

            var knownMembers = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                // Linhas “catch-all” que não são propriedades de um tipo específico.
                "Atributos",
                "Attributes",
                "AdditionalAttributes"
            };

            foreach (var type in entry.Types)
            {
                foreach (var argument in type.GetGenericArguments())
                {
                    knownMembers.Add(argument.Name);
                }

                foreach (var property in type.GetProperties(BindingFlags.Public | BindingFlags.Instance))
                {
                    knownMembers.Add(property.Name);
                }

                foreach (var method in type.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static))
                {
                    knownMembers.Add(method.Name);
                }
            }

            foreach (var owner in entry.Types)
            {
                var typeRows = PageParser.RowsForType(page, owner, owner == entry.PrimaryType);
                var rowsByName = new Dictionary<string, (ApiTableDoc Table, string[] Cells)>(StringComparer.OrdinalIgnoreCase);
                foreach (var row in typeRows
                             .Where(t => t.Headers.Count > 0 &&
                                         t.Headers[0].Equals("Nome", StringComparison.OrdinalIgnoreCase))
                             .SelectMany(t => t.Rows.Select(cells => (Table: t, Cells: cells)))
                             .Where(r => r.Cells.Length > 0))
                {
                    foreach (var name in RowNames.Split(row.Cells[0]))
                    {
                        rowsByName.TryAdd(name, row);
                    }
                }

                foreach (var property in owner.GetProperties(BindingFlags.Public | BindingFlags.Instance))
                {
                    if (property.GetCustomAttribute<ParameterAttribute>() is not { } attribute)
                    {
                        continue;
                    }

                    if (attribute.CaptureUnmatchedValues ||
                        property.GetCustomAttribute<CascadingParameterAttribute>() is not null ||
                        alwaysIgnore.Contains(property.Name) ||
                        ignored.Contains(property.Name))
                    {
                        continue;
                    }

                    var inherited = property.DeclaringType != owner || owner.IsAbstract;
                    var typeName = RepoContext.ShortName(owner);

                    if (!rowsByName.TryGetValue(property.Name, out var row))
                    {
                        issues.Add(new AuditIssue(
                            entry.Label,
                            typeName,
                            inherited ? "soft" : "hard",
                            inherited ? "missing_inherited" : "missing_in_docs",
                            property.Name,
                            $"{RepoContext.FriendlyType(property.PropertyType)} sem linha na tabela de API"));
                        continue;
                    }

                    var typeIndex = IndexOf(row.Table.Headers, "Tipo");
                    if (typeIndex <= 0 || typeIndex >= row.Cells.Length)
                    {
                        continue;
                    }

                    var documentedType = RepoContext.StripHtml(row.Cells[typeIndex]);
                    var codeType = RepoContext.FriendlyType(property.PropertyType);
                    if (documentedType.Length == 0 || TypesCompatible(documentedType, codeType))
                    {
                        continue;
                    }

                    issues.Add(new AuditIssue(
                        entry.Label,
                        typeName,
                        "soft",
                        "type_mismatch",
                        property.Name,
                        $"doc '{documentedType}' x código '{codeType}'"));
                }
            }

            var phantomKeys = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (var row in rows)
            {
                if (!IsApiSection(row.Table.SectionLabel))
                {
                    continue;
                }

                foreach (var name in RowNames.Split(row.Cells[0]))
                {
                    if (knownMembers.Contains(name) || !phantomKeys.Add(name))
                    {
                        continue;
                    }

                    issues.Add(new AuditIssue(
                        entry.Label,
                        RepoContext.ShortName(entry.PrimaryType ?? typeof(object)),
                        "hard",
                        "phantom_in_docs",
                        name,
                        $"linha na seção '{row.Table.SectionLabel}' não corresponde a nenhuma propriedade/método da página"));
                }
            }
        }

        var ordered = issues
            .OrderBy(i => i.Severity == "hard" ? 0 : 1)
            .ThenBy(i => i.Component, StringComparer.Ordinal)
            .ThenBy(i => i.Member, StringComparer.OrdinalIgnoreCase)
            .ToList();

        return new AuditReport(
            1,
            mode,
            version,
            new AuditSummary(
                componentCount,
                ordered.Count(i => i.Severity == "hard"),
                ordered.Count(i => i.Severity == "soft")),
            ordered);
    }

    public static void Print(AuditReport report)
    {
        Console.WriteLine(
            $"[docs-gen] audit ({report.Mode}): {report.Summary.Components} componentes | " +
            $"hard: {report.Summary.Hard} | soft: {report.Summary.Soft}");

        foreach (var group in report.Issues.GroupBy(i => i.Component))
        {
            var hard = group.Count(i => i.Severity == "hard");
            var soft = group.Count(i => i.Severity == "soft");
            Console.WriteLine($"  {group.Key}: {hard} hard, {soft} soft");
            foreach (var issue in group.Take(8))
            {
                Console.WriteLine($"    - [{issue.Severity}] {issue.Kind} {issue.Member} ({issue.Type}) — {issue.Detail}");
            }

            if (group.Count() > 8)
            {
                Console.WriteLine($"    … +{group.Count() - 8} (veja o relatório JSON)");
            }
        }
    }

    private static bool IsApiSection(string label) =>
        label.StartsWith("Propriedades", StringComparison.OrdinalIgnoreCase) ||
        label.StartsWith("Templates", StringComparison.OrdinalIgnoreCase) ||
        label.StartsWith("Eventos", StringComparison.OrdinalIgnoreCase) ||
        label.StartsWith("Métodos", StringComparison.OrdinalIgnoreCase) ||
        label.StartsWith("Serviço", StringComparison.OrdinalIgnoreCase) ||
        label.StartsWith("Herança", StringComparison.OrdinalIgnoreCase) ||
        label.StartsWith("Op", StringComparison.Ordinal);

    private static bool TypesCompatible(string documented, string code)
    {
        var a = NormalizeType(documented);
        var b = NormalizeType(code);
        return a.Length == 0 || b.Length == 0 || a.Contains(b, StringComparison.Ordinal) || b.Contains(a, StringComparison.Ordinal);
    }

    private static string NormalizeType(string value)
    {
        var normalized = TypeAliasRegex.Replace(WebUtility.HtmlDecode(value).ToLowerInvariant(), "");
        normalized = normalized.Replace("?", "", StringComparison.Ordinal).Replace("system.", "", StringComparison.Ordinal);
        return normalized switch
        {
            "boolean" => "bool",
            "integer" => "int",
            "list" => "list",
            _ => normalized
        };
    }

    private static int IndexOf(IReadOnlyList<string> headers, string name)
    {
        for (var i = 0; i < headers.Count; i++)
        {
            if (headers[i].Equals(name, StringComparison.OrdinalIgnoreCase))
            {
                return i;
            }
        }

        return -1;
    }
}
