using Microsoft.AspNetCore.Components;
using OpBlazorUI.Base.Components.TreeTable;

namespace OpBlazorUI.Base.Models;

public sealed class OpTreeTableColumn
{
    public string Field { get; set; } = "";
    public string Header { get; set; } = "";
    public string? Width { get; set; }
    public bool Sortable { get; set; }
    public string? StyleClass { get; set; }
    public RenderFragment<OpTreeTableCellContext>? BodyTemplate { get; set; }

    /// <summary>Coluna que contém o botão de expandir/recolher (padrão: a primeira).</summary>
    public bool Expandable { get; set; }
}
