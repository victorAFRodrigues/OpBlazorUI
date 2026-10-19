namespace OpBlazorUI.DocsGen;

internal sealed record MenuItem(string Label, string? Route, string Category, string Status);

internal sealed class CatalogEntry
{
    public required string Label { get; init; }
    public required string Id { get; init; }
    public string? Route { get; init; }
    public required string Category { get; init; }
    public string Kind { get; set; } = "component";
    public required string Status { get; init; }
    public required string PageFile { get; init; }
    public required string PagePath { get; init; }
    public IReadOnlyList<string> Aliases { get; init; } = [];
    public List<Type> Types { get; } = [];
    public Type? PrimaryType => Types.Count > 0 ? Types[0] : null;
}

internal sealed class PageDoc
{
    public string File { get; set; } = "";
    public string Raw { get; set; } = "";
    public string Title { get; set; } = "";
    public string? Description { get; set; }
    public List<SectionDoc> Sections { get; } = [];
    public List<ApiTableDoc> ApiTables { get; } = [];
    public List<ApiTableDoc> AllTables { get; } = [];
    public List<ExampleDoc> Examples { get; } = [];
    public List<string> Warnings { get; } = [];
    public Dictionary<string, string> ThemingClassDescriptions { get; } = new(StringComparer.OrdinalIgnoreCase);
}

internal sealed class SectionDoc
{
    public string Label { get; init; } = "";
    public string Id { get; init; } = "";
    public int Start { get; init; }
    public int End { get; init; }
    public bool InsideApi { get; init; }
}

internal sealed class ApiTableDoc
{
    public string SectionLabel { get; init; } = "";
    public string SectionId { get; init; } = "";
    public string Caption { get; init; } = "";
    public IReadOnlyList<string> Headers { get; init; } = [];
    public List<string[]> Rows { get; } = [];
    public bool InsideApi { get; init; }
}

internal sealed class ExampleDoc
{
    public string? SectionId { get; set; }
    public string? SectionLabel { get; set; }
    public string? Language { get; set; }
    public string ConstName { get; set; } = "";
    public string Code { get; set; } = "";
}

internal sealed record Manifest(
    int SchemaVersion,
    LibraryManifest Library,
    GettingStartedManifest GettingStarted,
    IReadOnlyList<string> Themes,
    IReadOnlyList<ComponentManifest> Components,
    IReadOnlyList<ServiceManifest> Services,
    IReadOnlyList<EnumManifest> Enums,
    IReadOnlyList<string> Icons);

internal sealed record LibraryManifest(
    string Name,
    string Version,
    string? Commit,
    string DocsBaseUrl,
    string TargetFramework);

internal sealed record GettingStartedManifest(
    PackageManifest Package,
    ConfigureManifest Configure,
    IReadOnlyList<string> Notes);

internal sealed record PackageManifest(
    string Id,
    string? ChartsId,
    string Feed,
    bool RequiresPat,
    string NugetConfig,
    string AddPackage);

internal sealed record ConfigureManifest(
    string Program,
    string Imports,
    string Head,
    string Layout);

internal sealed record ComponentManifest(
    string Id,
    string Name,
    string Category,
    string Kind,
    string Status,
    string? DocsRoute,
    string? DocsUrl,
    string? Summary,
    IReadOnlyList<string> Aliases,
    IReadOnlyList<TypeManifest> Types,
    IReadOnlyList<CssClassManifest> CssClasses,
    IReadOnlyList<TokenManifest> Tokens,
    IReadOnlyList<SectionManifest> Sections,
    IReadOnlyList<ExampleManifest> Examples);

internal sealed record TypeManifest(
    string Name,
    string FullName,
    string? Summary,
    IReadOnlyList<ParameterManifest> Parameters,
    IReadOnlyList<ParameterManifest> Events,
    IReadOnlyList<ParameterManifest> Templates,
    IReadOnlyList<PropertyManifest> Properties);

internal sealed record PropertyManifest(string Name, string Type, string? Description, bool Inherited);

internal sealed record ParameterManifest(
    string Name,
    string Type,
    string? DefaultValue,
    string? Description,
    bool Documented,
    bool Required,
    bool Inherited,
    bool Cascading,
    bool CaptureUnmatchedValues);

internal sealed record CssClassManifest(string Class, string? Description);

internal sealed record TokenManifest(string Var, string Value);

internal sealed record SectionManifest(string Id, string Label);

internal sealed record ExampleManifest(string? SectionId, string? SectionLabel, string Code, string? Language);

internal sealed record ServiceManifest(
    string Name,
    string FullName,
    string? Summary,
    string? DocsUrl,
    IReadOnlyList<MethodManifest> Methods);

internal sealed record MethodManifest(string Name, string Signature, string? Summary);

internal sealed record EnumManifest(string Name, string FullName, IReadOnlyList<string> Values);

internal sealed record AuditIssue(
    string Component,
    string Type,
    string Severity,
    string Kind,
    string Member,
    string? Detail);

internal sealed record AuditSummary(int Components, int Hard, int Soft);

internal sealed record AuditReport(
    int SchemaVersion,
    string Mode,
    string Version,
    AuditSummary Summary,
    IReadOnlyList<AuditIssue> Issues);
