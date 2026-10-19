using System.Reflection;
using Microsoft.AspNetCore.Components;

namespace OpBlazorUI.DocsGen;

internal sealed class ManifestOptions
{
    public string Version { get; init; } = "0.0.0-dev";
    public string? Commit { get; init; }
    public string BaseUrl { get; init; } = "https://victorafrodrigues.github.io/OpBlazorUI";
    public string TargetFramework { get; init; } = "net10.0";
}

internal static class ManifestBuilder
{
    public static Manifest Build(
        RepoContext repo,
        IReadOnlyList<CatalogEntry> catalog,
        XmlDocs xml,
        ManifestOptions options)
    {
        var pages = catalog.ToDictionary(e => e.Id, e => PageParser.Parse(e.PagePath), StringComparer.Ordinal);
        var theming = ThemingData.Load(repo.DocThemingPath);
        var icons = IconsParser.Load(repo.IconsCssPath);

        var components = new List<ComponentManifest>(catalog.Count);

        foreach (var entry in catalog)
        {
            var page = pages[entry.Id];
            var types = new List<TypeManifest>();
            for (var i = 0; i < entry.Types.Count; i++)
            {
                types.Add(BuildType(entry.Types[i], page, xml, isPrimary: i == 0));
            }

            var label = entry.Label.ToLowerInvariant();
            var cssClasses = theming.ClassesFor(entry.Id, label)
                .Select(c => new CssClassManifest(
                    c,
                    page.ThemingClassDescriptions.GetValueOrDefault(c.TrimStart('.'))))
                .ToList();

            var tokens = theming.TokensFor(entry.Id, label)
                .Select(t => new TokenManifest(t.Var, t.Value))
                .ToList();

            var docsUrl = entry.Route is null
                ? null
                : options.BaseUrl.TrimEnd('/') + entry.Route;

            components.Add(new ComponentManifest(
                entry.Id,
                entry.Label,
                entry.Category,
                entry.Kind,
                entry.Status,
                entry.Route,
                docsUrl,
                page.Description,
                entry.Aliases,
                types,
                cssClasses,
                tokens,
                page.Sections.Select(s => new SectionManifest(s.Id, s.Label)).ToList(),
                page.Examples
                    .Select(e => new ExampleManifest(e.SectionId, e.SectionLabel, e.Code))
                    .ToList()));
        }

        var assemblies = new[]
        {
            Assembly.Load("OpBlazorUI"),
            Assembly.Load("OpBlazorUI.Charts")
        };

        return new Manifest(
            1,
            new LibraryManifest("OpBlazorUI", options.Version, options.Commit, options.BaseUrl.TrimEnd('/'), options.TargetFramework),
            GettingStarted.Create(),
            ["aura", "lara", "nora"],
            components,
            BuildServices(assemblies, catalog, pages, options),
            BuildEnums(assemblies),
            icons);
    }

    private static TypeManifest BuildType(Type type, PageDoc page, XmlDocs xml, bool isPrimary)
    {
        var rows = MapRows(PageParser.RowsForType(page, type, isPrimary));
        var parameters = new List<ParameterManifest>();
        var events = new List<ParameterManifest>();
        var templates = new List<ParameterManifest>();
        var properties = new List<PropertyManifest>();

        foreach (var property in type.GetProperties(BindingFlags.Public | BindingFlags.Instance))
        {
            if (property.GetIndexParameters().Length > 0)
            {
                continue;
            }

            var attribute = property.GetCustomAttribute<ParameterAttribute>();
            if (attribute is null)
            {
                rows.TryGetValue(property.Name, out var plainRow);
                var plainDescription = string.IsNullOrWhiteSpace(plainRow?.Description)
                    ? xml.ForProperty(property)
                    : plainRow!.Description;

                properties.Add(new PropertyManifest(
                    property.Name,
                    RepoContext.FriendlyType(property.PropertyType),
                    string.IsNullOrWhiteSpace(plainDescription) ? null : plainDescription,
                    property.DeclaringType != type));
                continue;
            }

            rows.TryGetValue(property.Name, out var row);
            var cascading = property.GetCustomAttribute<CascadingParameterAttribute>() is not null;
            var required = property.GetCustomAttribute<EditorRequiredAttribute>() is not null;
            var description = string.IsNullOrWhiteSpace(row?.Description)
                ? xml.ForProperty(property)
                : row!.Description;

            var manifest = new ParameterManifest(
                property.Name,
                RepoContext.FriendlyType(property.PropertyType),
                string.IsNullOrWhiteSpace(row?.DefaultValue) ? null : row!.DefaultValue,
                string.IsNullOrWhiteSpace(description) ? null : description,
                row is not null,
                required,
                property.DeclaringType != type,
                cascading,
                attribute.CaptureUnmatchedValues);

            if (RepoContext.IsEventCallback(property.PropertyType))
            {
                events.Add(manifest);
            }
            else if (RepoContext.IsRenderFragment(property.PropertyType))
            {
                templates.Add(manifest);
            }
            else
            {
                parameters.Add(manifest);
            }
        }

        return new TypeManifest(
            RepoContext.DeclarationName(type),
            RepoContext.FullDeclarationName(type),
            xml.ForType(type),
            parameters,
            events,
            templates,
            properties);
    }

    private static Dictionary<string, RowInfo> MapRows(IReadOnlyList<ApiTableDoc> tables)
    {
        var map = new Dictionary<string, RowInfo>(StringComparer.OrdinalIgnoreCase);

        foreach (var table in tables)
        {
            var nameIndex = IndexOf(table.Headers, "Nome");
            if (nameIndex < 0)
            {
                continue;
            }

            var typeIndex = IndexOf(table.Headers, "Tipo");
            var defaultValueIndex = IndexOf(table.Headers, "Padrão");
            var descriptionIndex = IndexOf(table.Headers, "Descrição");

            foreach (var row in table.Rows)
            {
                if (row.Length <= nameIndex)
                {
                    continue;
                }

                var info = new RowInfo(
                    descriptionIndex >= 0 && descriptionIndex < row.Length
                        ? NullIfEmpty(RepoContext.StripHtml(row[descriptionIndex]))
                        : null,
                    defaultValueIndex >= 0 && defaultValueIndex < row.Length
                        ? NullIfEmpty(RepoContext.StripHtml(row[defaultValueIndex]))
                        : null,
                    typeIndex >= 0 && typeIndex < row.Length
                        ? NullIfEmpty(RepoContext.StripHtml(row[typeIndex]))
                        : null);

                foreach (var name in RowNames.Split(row[nameIndex]))
                {
                    map.TryAdd(name, info);
                }
            }
        }

        return map;
    }

    private static IReadOnlyList<ServiceManifest> BuildServices(
        Assembly[] assemblies,
        IReadOnlyList<CatalogEntry> catalog,
        IReadOnlyDictionary<string, PageDoc> pages,
        ManifestOptions options)
    {
        var services = new List<ServiceManifest>();

        foreach (var type in assemblies.SelectMany(a => a.GetExportedTypes())
                     .Where(t => t.IsClass && t.Name.EndsWith("Service", StringComparison.Ordinal))
                     .OrderBy(t => t.Name, StringComparer.Ordinal))
        {
            var methods = type.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
                .Where(m => !m.IsSpecialName)
                .Select(m => new MethodManifest(m.Name, Signature(m), null))
                .ToList();

            // Resumos de método são caros de casar por assinatura; o tipo já ajuda o agente.
            var docsEntry = catalog.FirstOrDefault(entry =>
                entry.Types.Contains(type) ||
                (pages.TryGetValue(entry.Id, out var page) &&
                 page.Sections.Any(s => s.Label.Contains(RepoContext.ShortName(type), StringComparison.Ordinal))));

            services.Add(new ServiceManifest(
                type.Name,
                RepoContext.FullDeclarationName(type),
                null,
                docsEntry?.Route is null ? null : options.BaseUrl.TrimEnd('/') + docsEntry.Route,
                methods));
        }

        return services;
    }

    private static IReadOnlyList<EnumManifest> BuildEnums(Assembly[] assemblies) =>
        assemblies.SelectMany(a => a.GetExportedTypes())
            .Where(t => t.IsEnum)
            .OrderBy(t => t.Name, StringComparer.Ordinal)
            .Select(t => new EnumManifest(
                t.Name,
                RepoContext.FullDeclarationName(t),
                Enum.GetNames(t)))
            .ToList();

    private static string Signature(MethodInfo method)
    {
        var parameters = method.GetParameters()
            .Select(p => $"{RepoContext.FriendlyType(p.ParameterType)} {p.Name}");
        return $"{RepoContext.FriendlyType(method.ReturnType)} {method.Name}({string.Join(", ", parameters)})";
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

    private static string? NullIfEmpty(string value) => string.IsNullOrWhiteSpace(value) ? null : value;

    private sealed record RowInfo(string? Description, string? DefaultValue, string? TypeName);
}
