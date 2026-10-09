using System.Globalization;
using System.Reflection;
using System.Text;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using OpBlazorUI.Base.Components.Common;
using OpBlazorUI.Base.Models;

namespace OpBlazorUI.Base.Components.Tree;

/// <summary>
/// Árvore hierárquica (<c>p-tree</c>) baseada em <see cref="OpTreeNode"/>. Renderiza nos modos
/// Interativo e SSR estático: todo o estado é scoped na instância.
/// </summary>
public partial class OpTree : OpComponentBase
{
    private string _filterValue = string.Empty;
    private string _filterText = string.Empty;
    private string _filterSignature = string.Empty;
    private List<OpTreeNode>? _filteredNodes;
    private IReadOnlyList<OpTreeNode>? _nodesRef;

    // ---------------------------------------------------------------- params
    [Parameter] public IReadOnlyList<OpTreeNode> Nodes { get; set; } = Array.Empty<OpTreeNode>();

    /// <summary>Modo de seleção: <c>single</c>, <c>multiple</c> ou <c>checkbox</c>.</summary>
    [Parameter] public string SelectionMode { get; set; } = "single";

    /// <summary>
    /// Em <c>single</c>, um <see cref="OpTreeNode"/>; em <c>multiple</c>/<c>checkbox</c>, um
    /// <see cref="IReadOnlyDictionary{TKey,TValue}"/> chaveado por <see cref="OpTreeNode.Key"/>.
    /// </summary>
    [Parameter] public object? Selection { get; set; }

    [Parameter] public EventCallback<object?> SelectionChanged { get; set; }

    /// <summary>Quando true, ctrl/meta é exigido para somar/remover itens na seleção múltipla.</summary>
    [Parameter] public bool MetaKeySelection { get; set; } = true;

    [Parameter] public bool Filter { get; set; }
    [Parameter] public string FilterBy { get; set; } = "label";

    /// <summary>Estratégia de filtro: <c>lenient</c> (inclui a subárvore do nó que casa) ou <c>strict</c>.</summary>
    [Parameter] public string FilterMode { get; set; } = "lenient";

    [Parameter] public string FilterPlaceholder { get; set; } = "Filter";
    [Parameter] public string? ScrollHeight { get; set; } = "400px";
    [Parameter] public bool Loading { get; set; }

    /// <summary>Exibição do loading global: <c>mask</c> ou <c>icon</c>.</summary>
    [Parameter] public string LoadingMode { get; set; } = "mask";

    [Parameter] public string? EmptyMessage { get; set; }
    [Parameter] public string? AriaLabel { get; set; }

    [Parameter] public bool ShowHeader { get; set; } = true;
    [Parameter] public RenderFragment? HeaderTemplate { get; set; }
    [Parameter] public RenderFragment? FooterTemplate { get; set; }
    [Parameter] public RenderFragment? EmptyTemplate { get; set; }
    [Parameter] public RenderFragment<OpTreeNode>? ItemTemplate { get; set; }
    [Parameter] public RenderFragment<OpTreeNode>? TogglerIconTemplate { get; set; }

    // events
    [Parameter] public EventCallback<OpTreeNode> OnNodeExpand { get; set; }
    [Parameter] public EventCallback<OpTreeNode> OnNodeCollapse { get; set; }
    [Parameter] public EventCallback<OpTreeNode> OnNodeSelect { get; set; }
    [Parameter] public EventCallback<OpTreeNode> OnNodeUnselect { get; set; }
    [Parameter] public EventCallback<string> OnFilter { get; set; }

    // ------------------------------------------------------------ lifecycle
    protected override void OnParametersSet()
    {
        // O filtro também precisa ser recalculado quando FilterBy/FilterMode mudam em runtime.
        var signature = FilterSignature();
        if (!ReferenceEquals(_nodesRef, Nodes) ||
            !string.Equals(_filterSignature, signature, StringComparison.Ordinal))
        {
            _nodesRef = Nodes;
            UpdateFilter();
        }
    }

    private string FilterSignature() => $"{_filterValue}\u001f{FilterBy}\u001f{FilterMode}";

    private void UpdateFilter()
    {
        _filterSignature = FilterSignature();

        if (!Filter || string.IsNullOrEmpty(_filterValue))
        {
            _filteredNodes = null;
            _filterText = string.Empty;
        }
        else
        {
            ApplyFilter();
        }
    }

    // ------------------------------------------------------------ computed
    private bool IsCheckbox => SelectionMode == "checkbox";

    private bool IsMultiple => SelectionMode == "multiple" || IsCheckbox;

    private OpTreeNode? SingleSelection => Selection as OpTreeNode;

    private IReadOnlyDictionary<string, bool>? MultiSelection => Selection as IReadOnlyDictionary<string, bool>;

    private IReadOnlyList<OpTreeNode> VisibleRoots => _filteredNodes ?? Nodes;

    private string RootClass => Class(
        "p-tree p-component",
        ScrollHeight == "flex" ? "p-tree-flex-scrollable" : null,
        StyleClass);

    private string WrapperClass => "p-tree-root";

    private string? WrapperStyle =>
        string.IsNullOrEmpty(ScrollHeight) || ScrollHeight == "flex" ? null : $"max-height:{ScrollHeight}";

    private string NodeClass(OpTreeNode node) =>
        Class("p-tree-node", node.IsLeaf ? "p-tree-node-leaf" : null);

    private string NodeContentClass(bool selected) =>
        Class("p-tree-node-content", "p-tree-node-selectable", selected ? "p-tree-node-selected" : null);

    private string ToggleIconClass(bool expanded) =>
        Class("p-tree-node-icon", "pi", expanded ? "pi-chevron-down" : "pi-chevron-right");

    // ------------------------------------------------------------ selection
    private bool IsSelected(OpTreeNode node)
    {
        if (IsMultiple)
        {
            return MultiSelection is not null && MultiSelection.TryGetValue(node.Key, out var value) && value;
        }

        var current = SingleSelection;
        return current is not null && (current.Key == node.Key || ReferenceEquals(current, node));
    }

    private bool IsPartial(OpTreeNode node)
    {
        if (IsSelected(node)) return false;
        if (!node.HasChildren) return false;
        return HasSelectedDescendant(node);
    }

    private bool HasSelectedDescendant(OpTreeNode node)
    {
        if (node.Children is null) return false;
        foreach (var child in node.Children)
        {
            if (IsSelected(child) || HasSelectedDescendant(child)) return true;
        }

        return false;
    }

    private static List<OpTreeNode> Descendants(OpTreeNode node)
    {
        var list = new List<OpTreeNode>();
        if (node.Children is null) return list;

        void Walk(OpTreeNode current)
        {
            list.Add(current);
            if (current.Children is null) return;
            foreach (var child in current.Children)
            {
                Walk(child);
            }
        }

        foreach (var child in node.Children)
        {
            Walk(child);
        }

        return list;
    }

    private void PropagateUp(OpTreeNode node, Dictionary<string, bool> dict)
    {
        // Propaga sobre a árvore visível (no filtro, os clones têm apenas os filhos exibidos).
        foreach (var root in VisibleRoots)
        {
            var path = new List<OpTreeNode>();
            if (!FindPath(root, node, path)) continue;

            for (var i = path.Count - 2; i >= 0; i--)
            {
                var ancestor = path[i];
                var children = ancestor.Children ?? new List<OpTreeNode>();
                var allSelected = children.Count > 0 &&
                                  children.All(c => dict.TryGetValue(c.Key, out var value) && value);
                if (allSelected)
                {
                    dict[ancestor.Key] = true;
                }
                else
                {
                    dict.Remove(ancestor.Key);
                }
            }

            return;
        }
    }

    private static bool FindPath(OpTreeNode current, OpTreeNode target, List<OpTreeNode> path)
    {
        path.Add(current);
        if (ReferenceEquals(current, target) || SameKey(current, target)) return true;
        if (current.Children is not null)
        {
            foreach (var child in current.Children)
            {
                if (FindPath(child, target, path)) return true;
            }
        }

        path.RemoveAt(path.Count - 1);
        return false;
    }

    // A árvore filtrada usa clones; casar por Key permite propagar a seleção para os ancestrais reais.
    private static bool SameKey(OpTreeNode a, OpTreeNode b) =>
        !string.IsNullOrEmpty(b.Key) && a.Key == b.Key;

    // ------------------------------------------------------------ interaction
    private async Task ToggleExpandAsync(OpTreeNode node)
    {
        if (node.IsLeaf) return;

        if (node.HasChildren)
        {
            node.Expanded = !node.Expanded;
            if (node.Expanded)
            {
                await OnNodeExpand.InvokeAsync(node);
            }
            else
            {
                await OnNodeCollapse.InvokeAsync(node);
            }
        }
        else
        {
            // Lazy: sem filhos ainda — sinaliza carregamento e dispara o expand.
            node.Loading = true;
            node.Expanded = true;
            await OnNodeExpand.InvokeAsync(node);
        }

        StateHasChanged();
    }

    private async Task OnNodeClickAsync(OpTreeNode node, MouseEventArgs e)
    {
        if (!node.Selectable) return;

        var metaKey = e.CtrlKey || e.MetaKey;

        if (IsCheckbox)
        {
            await ToggleCheckboxAsync(node);
        }
        else if (IsMultiple)
        {
            await ToggleMultipleAsync(node, metaKey);
        }
        else if (SelectionMode == "single")
        {
            await ToggleSingleAsync(node, metaKey);
        }

        StateHasChanged();
    }

    private async Task ToggleSingleAsync(OpTreeNode node, bool metaKey)
    {
        var selected = IsSelected(node);

        if (MetaKeySelection)
        {
            if (selected && metaKey)
            {
                Selection = null;
                await SelectionChanged.InvokeAsync(null);
                await OnNodeUnselect.InvokeAsync(node);
            }
            else
            {
                Selection = node;
                await SelectionChanged.InvokeAsync(node);
                await OnNodeSelect.InvokeAsync(node);
            }
        }
        else if (selected)
        {
            Selection = null;
            await SelectionChanged.InvokeAsync(null);
            await OnNodeUnselect.InvokeAsync(node);
        }
        else
        {
            Selection = node;
            await SelectionChanged.InvokeAsync(node);
            await OnNodeSelect.InvokeAsync(node);
        }
    }

    private async Task ToggleMultipleAsync(OpTreeNode node, bool metaKey)
    {
        var dict = new Dictionary<string, bool>(MultiSelection ?? new Dictionary<string, bool>());
        var selected = dict.TryGetValue(node.Key, out var value) && value;

        if (MetaKeySelection)
        {
            if (selected && metaKey)
            {
                dict[node.Key] = false;
                Selection = dict;
                await SelectionChanged.InvokeAsync(dict);
                await OnNodeUnselect.InvokeAsync(node);
                return;
            }

            if (!metaKey)
            {
                dict = new Dictionary<string, bool>();
            }

            dict[node.Key] = true;
            Selection = dict;
            await SelectionChanged.InvokeAsync(dict);
            await OnNodeSelect.InvokeAsync(node);
        }
        else
        {
            dict[node.Key] = !selected;
            Selection = dict;
            await SelectionChanged.InvokeAsync(dict);
            if (selected)
            {
                await OnNodeUnselect.InvokeAsync(node);
            }
            else
            {
                await OnNodeSelect.InvokeAsync(node);
            }
        }
    }

    private async Task ToggleCheckboxAsync(OpTreeNode node)
    {
        var dict = new Dictionary<string, bool>(MultiSelection ?? new Dictionary<string, bool>());
        var newValue = !IsSelected(node);

        dict[node.Key] = newValue;
        foreach (var descendant in Descendants(node))
        {
            dict[descendant.Key] = newValue;
        }

        PropagateUp(node, dict);

        Selection = dict;
        await SelectionChanged.InvokeAsync(dict);
        if (newValue)
        {
            await OnNodeSelect.InvokeAsync(node);
        }
        else
        {
            await OnNodeUnselect.InvokeAsync(node);
        }
    }

    // ------------------------------------------------------------ filtering
    private async Task OnFilterInput(ChangeEventArgs e)
    {
        _filterValue = e.Value?.ToString() ?? string.Empty;
        UpdateFilter();

        await OnFilter.InvokeAsync(_filterValue);
        StateHasChanged();
    }

    private void ApplyFilter()
    {
        _filterText = RemoveAccents(_filterValue);
        var result = new List<OpTreeNode>();

        foreach (var node in Nodes)
        {
            var copy = CloneNode(node);
            var matched = FilterMode == "strict"
                ? FindFilteredNodes(copy) || IsFilterMatched(copy)
                : IsFilterMatched(copy) || FindFilteredNodes(copy);

            if (matched)
            {
                result.Add(copy);
            }
        }

        _filteredNodes = result;
    }

    private bool IsFilterMatched(OpTreeNode node)
    {
        var matched = MatchesFilter(node);
        if (!matched || (FilterMode == "strict" && node.HasChildren))
        {
            matched = FindFilteredNodes(node) || matched;
        }

        return matched;
    }

    private bool FindFilteredNodes(OpTreeNode node)
    {
        var matched = false;
        if (node.Children is not null)
        {
            var kept = new List<OpTreeNode>();
            foreach (var child in node.Children)
            {
                var copyChild = CloneNode(child);
                if (IsFilterMatched(copyChild))
                {
                    matched = true;
                    kept.Add(copyChild);
                }
            }

            node.Children = kept;
        }

        if (matched)
        {
            node.Expanded = true;
            return true;
        }

        return false;
    }

    private bool MatchesFilter(OpTreeNode node)
    {
        if (string.IsNullOrEmpty(_filterText)) return true;

        foreach (var field in FilterBy.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            var value = ResolveField(node, field);
            if (value is not null &&
                RemoveAccents(value).Contains(_filterText, StringComparison.CurrentCultureIgnoreCase))
            {
                return true;
            }
        }

        return false;
    }

    private static OpTreeNode CloneNode(OpTreeNode node) => new()
    {
        Key = node.Key,
        Label = node.Label,
        Data = node.Data,
        Icon = node.Icon,
        Children = node.Children,
        Leaf = node.Leaf,
        Expanded = node.Expanded,
        Selectable = node.Selectable,
        Loading = node.Loading,
    };

    private static string? ResolveField(OpTreeNode node, string field)
    {
        object? current = node;
        foreach (var part in field.Split('.', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            if (current is null) return null;
            var property = current.GetType().GetProperty(
                part, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
            if (property is null) return null;
            current = property.GetValue(current);
        }

        return current?.ToString();
    }

    private static string RemoveAccents(string value)
    {
        var normalized = value.Normalize(NormalizationForm.FormD);
        var builder = new StringBuilder(normalized.Length);
        foreach (var ch in normalized)
        {
            if (CharUnicodeInfo.GetUnicodeCategory(ch) != UnicodeCategory.NonSpacingMark)
            {
                builder.Append(ch);
            }
        }

        return builder.ToString().Normalize(NormalizationForm.FormC);
    }
}
