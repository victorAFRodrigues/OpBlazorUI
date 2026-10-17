using System.Text.Json.Serialization;

namespace OpBlazorUI.Components.DataTable;

/// <summary>Estado persistido com <c>StateKey</c> (página, ordenação e filtros).</summary>
internal sealed class DataTableState
{
    public int First { get; set; }

    public int Rows { get; set; }

    public string? GlobalFilter { get; set; }

    public List<DataTableSortState>? Sort { get; set; }

    public Dictionary<string, string?>? Filters { get; set; }
}

internal sealed class DataTableSortState
{
    public string Field { get; set; } = "";

    public int Order { get; set; }
}

/// <summary>Contexto de serialização do estado (compatível com trimming/AOT).</summary>
[JsonSerializable(typeof(DataTableState))]
[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase)]
internal sealed partial class DataTableStateJsonContext : JsonSerializerContext
{
}
