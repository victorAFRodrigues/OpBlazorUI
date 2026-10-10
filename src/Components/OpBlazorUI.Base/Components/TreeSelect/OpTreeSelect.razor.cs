using System.Globalization;
using System.Reflection;
using System.Text;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using OpBlazorUI.Base.Components.Forms;

namespace OpBlazorUI.Base.Components.TreeSelect;

public partial class OpTreeSelect : OpInputBase<string>
{
    private ElementReference _rootRef;
    private ElementReference _treeContainer;

    private string _id = "";

    private bool _overlayVisible;
    private bool _panelRendered;
    private bool _panelClosing;
    private string? _panelAnimationClass;
    private bool _focus;
    private string _filterValue = "";
    private string _filterText = "";
    private string _filterSignature = "\u0000";
    private IReadOnlyList<TreeNode>? _optionsRef;
    private List<TreeNode>? _filteredNodes;

    // ---------------------------------------------------------------- params
    [Parameter] public IReadOnlyList<TreeNode>? Options { get; set; }
    [Parameter] public IReadOnlyDictionary<string, bool>? Selection { get; set; }
    [Parameter] public EventCallback<IReadOnlyDictionary<string, bool>?> SelectionChanged { get; set; }
    [Parameter] public string SelectionMode { get; set; } = "single";
    [Parameter] public string Display { get; set; } = "comma";
    [Parameter] public string? Placeholder { get; set; }
    [Parameter] public bool Filter { get; set; }
    [Parameter] public string FilterBy { get; set; } = "label";
    [Parameter] public string FilterMode { get; set; } = "lenient";
    [Parameter] public bool ShowClear { get; set; }
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
    [Parameter] public RenderFragment? LoadingIconTemplate { get; set; }
    [Parameter] public RenderFragment<TreeNode>? ItemTogglerIconTemplate { get; set; }

    // ------------------------------------------------------------ lifecycle
    protected override void OnInitialized()
    {
        _id = InputId ?? $"op-treeselect-{Guid.NewGuid():N}";
    }

    protected override void OnParametersSet()
    {
        // O filtro precisa ser recalculado quando Options/FilterBy/FilterMode mudam em runtime.
        var signature = FilterSignature();
        if (!ReferenceEquals(_optionsRef, Options) ||
            !string.Equals(_filterSignature, signature, StringComparison.Ordinal))
        {
            _optionsRef = Options;
            UpdateFilter();
        }
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (!_panelRendered)
        {
            return;
        }

        // Teclado do overlay aberto (roving tabindex sobre os nós visíveis).
        await Interop.InvokeVoidAsync(OpInterop.TreeInterop, "initTreeKeyboard", _treeContainer);
        await Interop.InvokeVoidAsync(OpInterop.TreeInterop, "refreshTreeTabstop", _treeContainer);
    }

    private string FilterSignature() => $"{_filterValue}\u001f{FilterBy}\u001f{FilterMode}";

    private void UpdateFilter()
    {
        _filterSignature = FilterSignature();

        if (!Filter || string.IsNullOrEmpty(_filterValue))
        {
            _filteredNodes = null;
            _filterText = string.Empty;
            return;
        }

        _filterText = RemoveAccents(_filterValue);
        var result = new List<TreeNode>();
        foreach (var node in AllOptions)
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

    // ------------------------------------------------------------ computed
    private bool IsCheckbox => SelectionMode == "checkbox";
    private bool IsMultiple => SelectionMode == "multiple" || IsCheckbox;

    private IEnumerable<TreeNode> AllOptions => Options ?? Array.Empty<TreeNode>();

    private string RootClass => OpCss.BuildClass(
        "p-treeselect p-component p-inputwrapper",
        Display == "chip" ? "p-treeselect-display-chip" : null,
        Disabled ? "p-disabled" : null,
        IsInvalid ? "p-invalid" : null,
        _focus ? "p-focus" : null,
        Variant == "filled" ? "p-variant-filled" : null,
        HasSelection ? "p-inputwrapper-filled" : null,
        _focus || _overlayVisible ? "p-inputwrapper-focus" : null,
        _overlayVisible ? "p-treeselect-open" : null,
        ShowClear ? "p-treeselect-clearable" : null,
        Fluid ? "p-treeselect-fluid" : null,
        Size == "small" ? "p-treeselect-sm p-inputfield-sm" : null,
        Size == "large" ? "p-treeselect-lg p-inputfield-lg" : null,
        StyleClass);

    private string LabelClass => OpCss.BuildClass(
        "p-treeselect-label",
        !HasSelection && Placeholder is { Length: > 0 } ? "p-placeholder" : null,
        !HasSelection && string.IsNullOrEmpty(Placeholder) ? "p-treeselect-label-empty" : null);

    private string PanelClass => OpCss.BuildClass(
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

    private bool IsFilterMatched(TreeNode node)
    {
        var matched = MatchesFilter(node);
        if (!matched || (FilterMode == "strict" && node.HasChildren))
        {
            matched = FindFilteredNodes(node) || matched;
        }

        return matched;
    }

    private bool FindFilteredNodes(TreeNode node)
    {
        var matched = false;
        if (node.Children is not null)
        {
            var kept = new List<TreeNode>();
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

    private bool MatchesFilter(TreeNode node)
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

    private static TreeNode CloneNode(TreeNode node) => new()
    {
        Key = node.Key,
        Label = node.Label,
        Data = node.Data,
        Children = node.Children,
        Leaf = node.Leaf,
        Loading = node.Loading,
        Expanded = node.Expanded,
    };

    private static string? ResolveField(TreeNode node, string field)
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

    private List<TreeNode> VisibleRoots => _filteredNodes ?? AllOptions.ToList();

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

    // Clique fora e Escape chegam pelo OpOverlayAttach (OnOutsideClick/OnEscape). Sair com
    // Tab (do gatilho ou do filtro) fecha aqui, como no PrimeNG.
    private async Task OnRootKeydown(KeyboardEventArgs e)
    {
        if (e.Key == "Tab" && _overlayVisible)
        {
            await CloseAsync();
        }
    }

    // ------------------------------------------------------------ interaction
    private async Task ToggleExpand(TreeNode node)
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
            CurrentValue = node.Key;
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
        // Propaga sobre a árvore visível (no filtro, os clones têm apenas os filhos exibidos).
        foreach (var root in VisibleRoots)
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
    private static bool SameKey(TreeNode a, TreeNode b) =>
        !string.IsNullOrEmpty(b.Key) && a.Key == b.Key;

    private async Task RemoveNodeAsync(TreeNode node)
    {
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
            CurrentValue = null;
            await OnNodeUnselect.InvokeAsync(node);
        }
    }

    private async Task OnFilterInput(ChangeEventArgs e)
    {
        _filterValue = e.Value?.ToString() ?? string.Empty;
        UpdateFilter();
        await OnFilter.InvokeAsync(_filterValue);
        StateHasChanged();
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
            CurrentValue = null;
        }

        await OnClear.InvokeAsync();
    }

}
