namespace OpBlazorUI.Base.Models;

/// <summary>Nó de árvore usado pelo <c>OpTree</c> e pelo <c>OpTreeTable</c>.</summary>
public class OpTreeNode
{
    public string Key { get; set; } = "";
    public string Label { get; set; } = "";
    public object? Data { get; set; }
    public string? Icon { get; set; }
    public List<OpTreeNode>? Children { get; set; }
    public bool Leaf { get; set; }
    public bool Expanded { get; set; }
    public bool Selectable { get; set; } = true;
    public bool Loading { get; set; }

    public bool HasChildren => Children is { Count: > 0 };
}
