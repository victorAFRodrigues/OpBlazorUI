namespace OpBlazorUI.Base.Models;

public sealed class OpDataTableColumn
{
    public string Field { get; set; } = "";
    public string Header { get; set; } = "";
    public bool Sortable { get; set; } = true;
    public string? Width { get; set; }
    public string? StyleClass { get; set; }
}