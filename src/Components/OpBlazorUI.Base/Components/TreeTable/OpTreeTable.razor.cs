using System.Reflection;
using Microsoft.AspNetCore.Components;
using OpBlazorUI.Base.Components.Common;
using OpBlazorUI.Base.Components.Paginator;
using OpBlazorUI.Base.Models;

namespace OpBlazorUI.Base.Components.TreeTable;

public partial class OpTreeTable : OpComponentBase
{
    // ------------------------------------------------------------------ params
    [Parameter] public IReadOnlyList<OpTreeNode>? Nodes { get; set; }

    [Parameter] public IReadOnlyList<OpTreeTableColumn> Columns { get; set; } = Array.Empty<OpTreeTableColumn>();

    /// <summary>Modo de seleção: <c>single</c>, <c>multiple</c> ou <c>checkbox</c>.</summary>
    [Parameter] public string? SelectionMode { get; set; }

    /// <summary>
    /// Seleção atual. Em <c>single</c> é um <see cref="OpTreeNode"/>; em <c>multiple</c>/<c>checkbox</c>
    /// é um <see cref="IReadOnlyDictionary{TKey,TValue}"/> de <c>Key</c> do nó para <c>bool</c>.
    /// </summary>
    [Parameter] public object? Selection { get; set; }

    [Parameter] public EventCallback<object?> SelectionChanged { get; set; }

    [Parameter] public bool Loading { get; set; }

    [Parameter] public string? EmptyMessage { get; set; } = "No records found";

    [Parameter] public bool Paginator { get; set; }

    [Parameter] public int Rows { get; set; } = 10;

    [Parameter] public int First { get; set; }

    [Parameter] public EventCallback<int> FirstChanged { get; set; }

    [Parameter] public int TotalRecords { get; set; }

    [Parameter] public bool Lazy { get; set; }

    [Parameter] public bool RowHover { get; set; } = true;

    /// <summary>Tamanho: <c>small</c> ou <c>large</c>.</summary>
    [Parameter] public string? Size { get; set; }

    /// <summary>Posição do paginador: <c>top</c>, <c>bottom</c> ou <c>both</c>.</summary>
    [Parameter] public string PaginatorPosition { get; set; } = "bottom";

    [Parameter] public int PageLinks { get; set; } = 5;

    [Parameter] public bool AlwaysShowPaginator { get; set; } = true;

    [Parameter] public IReadOnlyList<int>? RowsPerPageOptions { get; set; }

    // events
    [Parameter] public EventCallback<OpTreeNode> OnNodeExpand { get; set; }
    [Parameter] public EventCallback<OpTreeNode> OnNodeCollapse { get; set; }
    [Parameter] public EventCallback<OpTreeNode> OnNodeSelect { get; set; }
    [Parameter] public EventCallback<OpTreeNode> OnNodeUnselect { get; set; }
    [Parameter] public EventCallback<OpPaginatorState> OnPageChange { get; set; }

    // templates
    [Parameter] public RenderFragment? HeaderTemplate { get; set; }
    [Parameter] public RenderFragment? FooterTemplate { get; set; }
    [Parameter] public RenderFragment? EmptyTemplate { get; set; }
    [Parameter] public RenderFragment? LoadingTemplate { get; set; }
    [Parameter] public RenderFragment? CaptionTemplate { get; set; }

    // ------------------------------------------------------------------ helpers
    private IReadOnlyList<OpTreeNode> SourceNodes => Nodes ?? Array.Empty<OpTreeNode>();

    private bool IsCheckbox => SelectionMode == "checkbox";

    private bool IsMultiple => SelectionMode == "multiple" || IsCheckbox;

    private bool IsSelectable => SelectionMode is "single" or "multiple" or "checkbox";

    private bool ShowSelectionColumn => IsCheckbox;

    private int ColumnSpan => Math.Max(1, Columns.Count + (ShowSelectionColumn ? 1 : 0));

    private string RootClass => Class(
        "p-treetable p-component",
        RowHover ? "p-treetable-hoverable" : null,
        Size == "small" ? "p-treetable-sm" : null,
        Size == "large" ? "p-treetable-lg" : null,
        StyleClass);

    private OpTreeTableColumn? ExpandableColumn =>
        Columns.FirstOrDefault(c => c.Expandable) ?? Columns.FirstOrDefault();

    private bool IsExpandableColumn(OpTreeTableColumn column) => ReferenceEquals(column, ExpandableColumn);

    private int EffectiveTotal => TotalRecords > 0 ? TotalRecords : FlattenedNodes.Count;

    // Achata apenas os nós visíveis (ancestrais expandidos), com nível/depth.
    private List<OpTreeTableFlatNode> FlattenedNodes
    {
        get
        {
            var list = new List<OpTreeTableFlatNode>();

            void Walk(OpTreeNode node, int level)
            {
                list.Add(new OpTreeTableFlatNode(node, level));
                if (node.Expanded && node.Children is { Count: > 0 })
                {
                    foreach (var child in node.Children)
                    {
                        Walk(child, level + 1);
                    }
                }
            }

            foreach (var root in SourceNodes)
            {
                Walk(root, 0);
            }

            return list;
        }
    }

    private List<OpTreeTableFlatNode> VisibleNodes
    {
        get
        {
            var flat = FlattenedNodes;
            if (!Paginator || Lazy)
            {
                return flat;
            }

            return flat.Skip(Math.Max(0, First)).Take(Math.Max(1, Rows)).ToList();
        }
    }

    private List<OpTreeNode> AllNodes()
    {
        var list = new List<OpTreeNode>();

        void Walk(OpTreeNode node)
        {
            list.Add(node);
            if (node.Children is not null)
            {
                foreach (var child in node.Children)
                {
                    Walk(child);
                }
            }
        }

        foreach (var root in SourceNodes)
        {
            Walk(root);
        }

        return list;
    }

    // ------------------------------------------------------------------ classes
    private string ThClass(OpTreeTableColumn column) => Class(
        "p-treetable-header-cell",
        column.Sortable ? "p-treetable-sortable-column" : null,
        column.StyleClass);

    private string TrClass(OpTreeNode node) => Class(
        "p-treetable-row",
        IsSelectable && node.Selectable ? "p-treetable-selectable-row" : null,
        IsSelected(node) ? "p-treetable-row-selected" : null);

    private string CellStyle(OpTreeTableCellContext context, OpTreeTableColumn column)
    {
        var indent = IsExpandableColumn(column) && context.Level > 0
            ? $"padding-inline-start:calc({context.Level} * var(--p-treetable-indent, 1rem));"
            : null;
        return indent ?? "";
    }

    private OpTreeTableCellContext CellContext(OpTreeTableFlatNode flat, OpTreeTableColumn column) =>
        new(flat.Node, flat.Level, column.Field);

    private object? GetFieldValue(OpTreeNode node, string field)
    {
        if (string.IsNullOrEmpty(field))
        {
            return node.Data;
        }

        var property = typeof(OpTreeNode).GetProperty(
            field, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
        if (property is not null)
        {
            return property.GetValue(node);
        }

        if (node.Data is not null)
        {
            var dataProperty = node.Data.GetType().GetProperty(
                field, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
            return dataProperty?.GetValue(node.Data);
        }

        return null;
    }

    // ------------------------------------------------------------------ selection
    private bool IsSelected(OpTreeNode node)
    {
        if (IsMultiple)
        {
            return Selection is IReadOnlyDictionary<string, bool> dict
                   && dict.TryGetValue(node.Key, out var value) && value;
        }

        return Selection is OpTreeNode selected
               && (ReferenceEquals(selected, node) || selected.Key == node.Key);
    }

    private bool IsPartial(OpTreeNode node)
    {
        if (!IsMultiple || IsSelected(node) || !node.HasChildren)
        {
            return false;
        }

        return HasSelectedDescendant(node);
    }

    private bool HasSelectedDescendant(OpTreeNode node)
    {
        if (node.Children is null)
        {
            return false;
        }

        foreach (var child in node.Children)
        {
            if (IsSelected(child) || HasSelectedDescendant(child))
            {
                return true;
            }
        }

        return false;
    }

    private bool AllSelected
    {
        get
        {
            var nodes = AllNodes();
            return nodes.Count > 0 && nodes.All(IsSelected);
        }
    }

    private bool AllPartial => AllNodes().Any(IsPartial);

    private static List<OpTreeNode> Descendants(OpTreeNode node)
    {
        var list = new List<OpTreeNode>();
        if (node.Children is null)
        {
            return list;
        }

        void Walk(OpTreeNode current)
        {
            list.Add(current);
            if (current.Children is not null)
            {
                foreach (var child in current.Children)
                {
                    Walk(child);
                }
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
        var path = new List<OpTreeNode>();
        foreach (var root in SourceNodes)
        {
            if (FindPath(root, node, path))
            {
                break;
            }
        }

        for (var i = path.Count - 2; i >= 0; i--)
        {
            var ancestor = path[i];
            var children = ancestor.Children ?? new List<OpTreeNode>();
            var allSelected = children.Count > 0
                              && children.All(c => dict.TryGetValue(c.Key, out var v) && v);
            if (allSelected)
            {
                dict[ancestor.Key] = true;
            }
            else
            {
                dict.Remove(ancestor.Key);
            }
        }
    }

    private static bool FindPath(OpTreeNode current, OpTreeNode target, List<OpTreeNode> path)
    {
        path.Add(current);
        if (ReferenceEquals(current, target))
        {
            return true;
        }

        if (current.Children is not null)
        {
            foreach (var child in current.Children)
            {
                if (FindPath(child, target, path))
                {
                    return true;
                }
            }
        }

        path.RemoveAt(path.Count - 1);
        return false;
    }

    // ------------------------------------------------------------------ interaction
    private async Task ToggleExpand(OpTreeNode node)
    {
        if (node.Leaf)
        {
            return;
        }

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
            node.Loading = true;
            node.Expanded = true;
            await OnNodeExpand.InvokeAsync(node);
        }

        StateHasChanged();
    }

    private async Task OnRowClicked(OpTreeNode node)
    {
        if (!IsSelectable || !node.Selectable)
        {
            return;
        }

        if (IsCheckbox)
        {
            await ToggleCheckboxAsync(node);
        }
        else if (IsMultiple)
        {
            await ToggleKeyAsync(node);
        }
        else
        {
            await ToggleSingleAsync(node);
        }
    }

    private async Task ToggleSingleAsync(OpTreeNode node)
    {
        var isSelected = Selection is OpTreeNode selected
                         && (ReferenceEquals(selected, node) || selected.Key == node.Key);

        if (isSelected)
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

    private async Task ToggleKeyAsync(OpTreeNode node)
    {
        var dict = new Dictionary<string, bool>(CurrentSelectionMap());
        var nowSelected = dict.TryGetValue(node.Key, out var value) && value;

        if (nowSelected)
        {
            dict.Remove(node.Key);
        }
        else
        {
            dict[node.Key] = true;
        }

        Selection = dict;
        await SelectionChanged.InvokeAsync(dict);

        if (nowSelected)
        {
            await OnNodeUnselect.InvokeAsync(node);
        }
        else
        {
            await OnNodeSelect.InvokeAsync(node);
        }
    }

    private async Task ToggleCheckboxAsync(OpTreeNode node)
    {
        var dict = new Dictionary<string, bool>(CurrentSelectionMap());
        var newValue = !IsSelected(node);

        if (newValue)
        {
            dict[node.Key] = true;
        }
        else
        {
            dict.Remove(node.Key);
        }

        foreach (var descendant in Descendants(node))
        {
            if (newValue)
            {
                dict[descendant.Key] = true;
            }
            else
            {
                dict.Remove(descendant.Key);
            }
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

    private async Task ToggleAllNodes()
    {
        var dict = new Dictionary<string, bool>(CurrentSelectionMap());
        var select = !AllSelected;

        foreach (var node in AllNodes())
        {
            if (!node.Selectable)
            {
                continue;
            }

            if (select)
            {
                dict[node.Key] = true;
            }
            else
            {
                dict.Remove(node.Key);
            }
        }

        Selection = dict;
        await SelectionChanged.InvokeAsync(dict);
    }

    private IReadOnlyDictionary<string, bool> CurrentSelectionMap() =>
        Selection as IReadOnlyDictionary<string, bool> ?? new Dictionary<string, bool>();

    private async Task OnPageChangeHandler(OpPaginatorState state)
    {
        First = state.First;
        Rows = state.Rows;
        await FirstChanged.InvokeAsync(First);
        await OnPageChange.InvokeAsync(state);
    }
}

internal sealed record OpTreeTableFlatNode(OpTreeNode Node, int Level);

public sealed record OpTreeTableCellContext(OpTreeNode Node, int Level, string Field);
