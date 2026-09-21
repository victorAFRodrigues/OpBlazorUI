using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;

namespace OpBlazorUI.Base.Components.TreeSelect;

public partial class OpTreeSelect : ComponentBase
{
    private string _id = "";

    private bool _overlayVisible;
    private bool _panelRendered;
    private bool _panelClosing;
    private string? _panelAnimationClass;
    private bool _focus;
    private bool _focusInside;
    private DateTime _lastPanelPointerDown = DateTime.MinValue;
    private string _filterValue = "";

    // ---------------------------------------------------------------- params
    [Parameter] public IReadOnlyList<TreeNode>? Options { get; set; }
    [Parameter] public string? Value { get; set; }
    [Parameter] public EventCallback<string?> ValueChanged { get; set; }
    [Parameter] public IReadOnlyDictionary<string, bool>? Selection { get; set; }
    [Parameter] public EventCallback<IReadOnlyDictionary<string, bool>?> SelectionChanged { get; set; }
    [Parameter] public string SelectionMode { get; set; } = "single";
    [Parameter] public string Display { get; set; } = "comma";
    [Parameter] public string? Placeholder { get; set; }
    [Parameter] public bool Filter { get; set; }
    [Parameter] public string FilterBy { get; set; } = "label";
    [Parameter] public string FilterMode { get; set; } = "lenient";
    [Parameter] public bool ShowClear { get; set; }
    [Parameter] public bool Disabled { get; set; }
    [Parameter] public bool Invalid { get; set; }
    [Parameter] public bool Required { get; set; }
    [Parameter] public string Variant { get; set; } = "outlined";
    [Parameter] public string? Size { get; set; }
    [Parameter] public bool Fluid { get; set; }
    [Parameter] public string? ScrollHeight { get; set; }
    [Parameter] public bool VirtualScroll { get; set; }
    [Parameter] public int VirtualScrollItemSize { get; set; } = 38;
    [Parameter] public string? EmptyMessage { get; set; } = "No results found";
    [Parameter] public bool Loading { get; set; }
    [Parameter] public string LoadingMode { get; set; } = "mask";
    [Parameter] public int TabIndex { get; set; }
    [Parameter] public string? AriaLabel { get; set; }
    [Parameter] public string? InputId { get; set; }
    [Parameter] public string? PanelStyleClass { get; set; }
    [Parameter] public string? StyleClass { get; set; }

    // events
    [Parameter] public EventCallback<TreeNode> OnNodeExpand { get; set; }
    [Parameter] public EventCallback<TreeNode> OnNodeCollapse { get; set; }
    [Parameter] public EventCallback<TreeNode> OnNodeSelect { get; set; }
    [Parameter] public EventCallback<TreeNode> OnNodeUnselect { get; set; }
    [Parameter] public EventCallback OnShow { get; set; }
    [Parameter] public EventCallback OnHide { get; set; }
    [Parameter] public EventCallback OnClear { get; set; }
    [Parameter] public EventCallback<string> OnFilter { get; set; }
    [Parameter] public EventCallback<FocusEventArgs> OnFocus { get; set; }
    [Parameter] public EventCallback<FocusEventArgs> OnBlur { get; set; }

    // templates
    [Parameter] public RenderFragment? HeaderTemplate { get; set; }
    [Parameter] public RenderFragment? FooterTemplate { get; set; }
    [Parameter] public RenderFragment? EmptyTemplate { get; set; }
    [Parameter] public RenderFragment? ClearIconTemplate { get; set; }
    [Parameter] public RenderFragment? DropdownIconTemplate { get; set; }
    [Parameter] public RenderFragment<TreeNode>? ItemTogglerIconTemplate { get; set; }

    [Parameter(CaptureUnmatchedValues = true)]
    public Dictionary<string, object>? AdditionalAttributes { get; set; }

    // ------------------------------------------------------------ lifecycle
    protected override void OnInitialized()
    {
        _id = InputId ?? $"op-treeselect-{Guid.NewGuid():N}";
    }

    // ------------------------------------------------------------ computed
    private bool IsCheckbox => SelectionMode == "checkbox";
    private bool IsMultiple => SelectionMode == "multiple" || IsCheckbox;

    private IEnumerable<TreeNode> AllOptions => Options ?? Array.Empty<TreeNode>();

    private string RootClass => BuildClass(
        "p-treeselect p-component",
        Disabled ? "p-disabled" : null,
        Variant == "filled" ? "p-variant-filled" : null,
        _focus ? "p-focus" : null,
        Invalid ? "p-invalid" : null,
        Fluid ? "p-treeselect-fluid" : null,
        Size == "small" ? "p-treeselect-sm" : null,
        Size == "large" ? "p-treeselect-lg" : null,
        StyleClass);

    private string LabelClass => BuildClass(
        "p-treeselect-label",
        !HasSelection && Placeholder is { Length: > 0 } ? "p-placeholder" : null,
        !HasSelection && string.IsNullOrEmpty(Placeholder) ? "p-treeselect-label-empty" : null);

    private string PanelClass => BuildClass(
        "p-treeselect-overlay p-component",
        PanelStyleClass,
        _panelAnimationClass);

    private List<TreeNode> SelectedNodes
    {
        get
        {
            var list = new List<TreeNode>();
            if (IsMultiple)
            {
                foreach (var node in Flatten())
                {
                    if (IsKeySelected(node.Key)) list.Add(node);
                }
            }
            else
            {
                var node = FindByKey(Value);
                if (node is not null) list.Add(node);
            }

            return list;
        }
    }

    private bool HasSelection => SelectedNodes.Count > 0;

    private string CommaLabel => string.Join(", ", SelectedNodes.Select(n => n.Label));

    // ------------------------------------------------------------ helpers
    private List<TreeNode> Flatten()
    {
        var list = new List<TreeNode>();
        void Walk(TreeNode node)
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

        foreach (var root in AllOptions)
        {
            Walk(root);
        }

        return list;
    }

    private TreeNode? FindByKey(string? key)
    {
        if (string.IsNullOrEmpty(key)) return null;
        return Flatten().FirstOrDefault(n => n.Key == key);
    }

    private bool IsKeySelected(string key)
    {
        if (IsMultiple)
        {
            return Selection is not null && Selection.TryGetValue(key, out var v) && v;
        }

        return Value == key;
    }

    private bool IsSelected(TreeNode node) => IsKeySelected(node.Key);

    private bool IsPartial(TreeNode node)
    {
        if (IsSelected(node)) return false;
        if (!node.HasChildren) return false;
        return HasSelectedDescendant(node);
    }

    private bool HasSelectedDescendant(TreeNode node)
    {
        if (node.Children is null) return false;
        foreach (var child in node.Children)
        {
            if (IsSelected(child) || HasSelectedDescendant(child)) return true;
        }

        return false;
    }

    private static List<TreeNode> Descendants(TreeNode node)
    {
        var list = new List<TreeNode>();
        if (node.Children is null) return list;
        void Walk(TreeNode n)
        {
            list.Add(n);
            if (n.Children is not null)
            {
                foreach (var c in n.Children)
                {
                    Walk(c);
                }
            }
        }

        foreach (var child in node.Children)
        {
            Walk(child);
        }

        return list;
    }

    private bool MatchesFilter(TreeNode node)
    {
        if (!Filter || string.IsNullOrEmpty(_filterValue)) return true;
        return node.Label.Contains(_filterValue, StringComparison.CurrentCultureIgnoreCase);
    }

    private List<TreeNode> VisibleRoots
    {
        get
        {
            if (Filter && !string.IsNullOrEmpty(_filterValue))
            {
                return Flatten().Where(MatchesFilter).ToList();
            }

            return AllOptions.ToList();
        }
    }

    private List<OpTreeFlatNode> VisibleFlatNodes
    {
        get
        {
            var list = new List<OpTreeFlatNode>();
            void Walk(TreeNode node, int depth)
            {
                list.Add(new OpTreeFlatNode(node, depth));
                if (node.Expanded && node.Children is not null)
                {
                    foreach (var child in node.Children)
                    {
                        Walk(child, depth + 1);
                    }
                }
            }

            foreach (var root in VisibleRoots)
            {
                Walk(root, 0);
            }

            return list;
        }
    }

    // ------------------------------------------------------------ overlay
    private async Task OnLabelClick(MouseEventArgs e)
    {
        if (Disabled) return;
        if (!_overlayVisible)
        {
            await OpenAsync();
        }
    }

    private async Task OnDropdownClick(MouseEventArgs e)
    {
        if (Disabled) return;
        if (!_overlayVisible)
        {
            await OpenAsync();
        }
    }

    private async Task OnLabelFocus(FocusEventArgs e)
    {
        if (Disabled) return;
        _focus = true;
        await OnFocus.InvokeAsync(e);
    }

    private async Task OnLabelBlur(FocusEventArgs e)
    {
        _focus = false;
        await OnBlur.InvokeAsync(e);
        StateHasChanged();
    }

    private async Task OpenAsync()
    {
        if (_overlayVisible || Disabled) return;
        _overlayVisible = true;
        _panelRendered = true;
        _panelClosing = false;
        _panelAnimationClass = "p-anchored-overlay-enter-active";
        await OnShow.InvokeAsync();
        StateHasChanged();
    }

    private async Task CloseAsync()
    {
        if (!_overlayVisible) return;
        _overlayVisible = false;
        _panelClosing = true;
        _panelAnimationClass = "p-anchored-overlay-leave-active";
        StateHasChanged();
        await OnHide.InvokeAsync();
        _ = ForceRemovePanelAsync();
    }

    private async Task ForceRemovePanelAsync()
    {
        await Task.Delay(400);
        if (_panelClosing)
        {
            _panelRendered = false;
            _panelClosing = false;
            _panelAnimationClass = null;
            await InvokeAsync(StateHasChanged);
        }
    }

    private async Task OnPanelAnimationEnd()
    {
        if (_panelClosing)
        {
            _panelRendered = false;
            _panelClosing = false;
        }

        _panelAnimationClass = null;
        await InvokeAsync(StateHasChanged);
    }

    private void OnRootFocusIn(FocusEventArgs e)
    {
        _focusInside = true;
    }

    private void OnPanelMouseDown()
    {
        _lastPanelPointerDown = DateTime.UtcNow;
        _focusInside = true;
    }

    private async Task OnRootFocusOut(FocusEventArgs e)
    {
        if (!_overlayVisible || _panelClosing) return;

        _focusInside = false;
        await Task.Delay(10);

        var interactedWithPanel = (DateTime.UtcNow - _lastPanelPointerDown).TotalMilliseconds < 250;
        if (!_focusInside && !interactedWithPanel)
        {
            await CloseAsync();
        }
    }

    private async Task OnRootKeydown(KeyboardEventArgs e)
    {
        if (e.Key == "Escape" && _overlayVisible)
        {
            await CloseAsync();
        }
    }

    // ------------------------------------------------------------ interaction
    private async Task ToggleExpand(TreeNode node)
    {
        if (node.Leaf) return;

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

    private async Task OnNodeClick(TreeNode node)
    {
        if (Disabled) return;

        if (IsCheckbox)
        {
            await ToggleCheckboxAsync(node);
        }
        else if (IsMultiple)
        {
            await ToggleKeyAsync(node.Key, node);
        }
        else
        {
            Value = node.Key;
            await ValueChanged.InvokeAsync(node.Key);
            await OnNodeSelect.InvokeAsync(node);
            await CloseAsync();
        }
    }

    private async Task ToggleKeyAsync(string key, TreeNode node)
    {
        var dict = new Dictionary<string, bool>(Selection ?? new Dictionary<string, bool>());
        var nowSelected = dict.TryGetValue(key, out var v) && v;
        dict[key] = !nowSelected;
        Selection = dict;
        await SelectionChanged.InvokeAsync(dict);
        if (!nowSelected)
        {
            await OnNodeSelect.InvokeAsync(node);
        }
        else
        {
            await OnNodeUnselect.InvokeAsync(node);
        }
    }

    private async Task ToggleCheckboxAsync(TreeNode node)
    {
        var dict = new Dictionary<string, bool>(Selection ?? new Dictionary<string, bool>());
        var newValue = !IsSelected(node);

        dict[node.Key] = newValue;
        foreach (var d in Descendants(node))
        {
            dict[d.Key] = newValue;
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

    private void PropagateUp(TreeNode node, Dictionary<string, bool> dict)
    {
        foreach (var root in AllOptions)
        {
            var path = new List<TreeNode>();
            if (FindPath(root, node, path))
            {
                // path[0] == root ... path[last] == node
                for (var i = path.Count - 2; i >= 0; i--)
                {
                    var ancestor = path[i];
                    var children = ancestor.Children ?? new List<TreeNode>();
                    var allSelected = children.Count > 0 &&
                                      children.All(c => dict.TryGetValue(c.Key, out var v) && v);
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
    }

    private bool FindPath(TreeNode current, TreeNode target, List<TreeNode> path)
    {
        path.Add(current);
        if (ReferenceEquals(current, target)) return true;
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

    private async Task RemoveNodeAsync(TreeNode node)
    {
        if (IsMultiple)
        {
            await ToggleKeyAsync(node.Key, node);
        }
        else
        {
            Value = null;
            await ValueChanged.InvokeAsync(null);
            await OnNodeUnselect.InvokeAsync(node);
        }
    }

    private async Task OnFilterInput(ChangeEventArgs e)
    {
        _filterValue = e.Value?.ToString() ?? string.Empty;
        await OnFilter.InvokeAsync(_filterValue);
    }

    private async Task Clear()
    {
        if (IsMultiple)
        {
            Selection = new Dictionary<string, bool>();
            await SelectionChanged.InvokeAsync(Selection);
        }
        else
        {
            Value = null;
            await ValueChanged.InvokeAsync(null);
        }

        await OnClear.InvokeAsync();
    }

    private static string BuildClass(params string?[] classes)
        => string.Join(' ', classes.Where(c => !string.IsNullOrWhiteSpace(c)));
}
