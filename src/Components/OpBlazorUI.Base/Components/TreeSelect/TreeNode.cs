namespace OpBlazorUI.Base.Components.TreeSelect;

/// <summary>Nó de árvore usado pelo <c>OpTreeSelect</c>.</summary>
public class TreeNode
{
    public string Key { get; set; } = "";
    public string Label { get; set; } = "";
    public object? Data { get; set; }
    public List<TreeNode>? Children { get; set; }

    /// <summary>
    /// Força o nó como folha (<c>true</c>) ou como pai expansível mesmo sem filhos ainda carregados
    /// — o caso do lazy (<c>false</c>). Quando <c>null</c>, deriva de <see cref="HasChildren"/>.
    /// </summary>
    public bool? Leaf { get; set; }

    public bool Loading { get; set; }
    public bool Expanded { get; set; }

    public bool HasChildren => Children is { Count: > 0 };

    public bool IsLeaf => Leaf ?? !HasChildren;
}

/// <summary>Nó achatado (com profundidade) usado pelo virtual scroll do <c>OpTreeSelect</c>.</summary>
public sealed record OpTreeFlatNode(TreeNode Node, int Depth);
