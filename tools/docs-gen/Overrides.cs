using System.Text.Json;
using System.Text.Json.Serialization;

namespace OpBlazorUI.DocsGen;

/// <summary>
/// Exceções e ajustes manuais: páginas que não seguem a convenção de nome,
/// componentes que vivem em tipos com outro nome, aliases de busca e isenções do audit.
/// </summary>
internal sealed class Overrides
{
    public Dictionary<string, ComponentOverride> ComponentOverrides { get; init; } = new(StringComparer.OrdinalIgnoreCase);

    public Dictionary<string, string> KindOverrides { get; init; } = new(StringComparer.OrdinalIgnoreCase);

    public AuditOverrides Audit { get; init; } = new();

    public static Overrides Load(string path)
    {
        if (!File.Exists(path))
        {
            throw new FileNotFoundException($"overrides.json não encontrado em {path}", path);
        }

        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            ReadCommentHandling = JsonCommentHandling.Skip,
            AllowTrailingCommas = true
        };

        return JsonSerializer.Deserialize<Overrides>(File.ReadAllText(path), options)
               ?? throw new InvalidOperationException("overrides.json inválido.");
    }
}

internal sealed class ComponentOverride
{
    public string? Id { get; init; }
    public string? Type { get; init; }
    public string[]? Types { get; init; }
    public string? Page { get; init; }
    public string[]? Aliases { get; init; }
}

internal sealed class AuditOverrides
{
    public string[] AlwaysIgnore { get; init; } = ["AdditionalAttributes"];

    public Dictionary<string, string[]> Ignore { get; init; } = new(StringComparer.OrdinalIgnoreCase);

    [JsonIgnore]
    public HashSet<string> AlwaysIgnoreSet => new(AlwaysIgnore, StringComparer.OrdinalIgnoreCase);
}
