using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.AspNetCore.Components.Web;
using OpBlazorUI.Base.Components.Icon;

namespace OpBlazorUI.Base.Components.CascadeSelect;

public partial class OpCascadeSelect<TValue> : ComponentBase
{
    private string _id = "";

    private bool _overlayVisible;
    private bool _panelRendered;
    private bool _panelClosing;
    private string? _panelAnimationClass;
    private bool _focus;
    private bool _focusInside;
    private DateTime _lastPanelPointerDown = DateTime.MinValue;

    private readonly List<object> _expandedPath = new();
    private object? _focusedNode;

    // ---------------------------------------------------------------- params
    [Parameter] public IReadOnlyList<object>? Options { get; set; }
    [Parameter] public string? OptionLabel { get; set; }
    [Parameter] public string? OptionValue { get; set; }
    [Parameter] public string? OptionDisabled { get; set; }
    [Parameter] public string OptionGroupLabel { get; set; } = "label";
    [Parameter] public IReadOnlyList<string>? OptionGroupChildren { get; set; }
    [Parameter] public TValue? Value { get; set; }
    [Parameter] public EventCallback<TValue?> ValueChanged { get; set; }
    [Parameter] public string? Placeholder { get; set; }
    [Parameter] public bool Disabled { get; set; }
    [Parameter] public bool Readonly { get; set; }
    [Parameter] public bool Invalid { get; set; }
    [Parameter] public bool Required { get; set; }
    [Parameter] public string Variant { get; set; } = "outlined";
    [Parameter] public string? Size { get; set; }
    [Parameter] public bool Fluid { get; set; }
    [Parameter] public bool ShowClear { get; set; }
    [Parameter] public bool Loading { get; set; }
    [Parameter] public string? DropdownIcon { get; set; }
    [Parameter] public int TabIndex { get; set; }
    [Parameter] public string? AriaLabel { get; set; }
    [Parameter] public string? AriaLabelledBy { get; set; }
    [Parameter] public string? InputId { get; set; }
    [Parameter] public bool ShowOnFocus { get; set; } = true;
    [Parameter] public string? PanelStyleClass { get; set; }
    [Parameter] public string? StyleClass { get; set; }

    // events
    [Parameter] public EventCallback<TValue?> OnChange { get; set; }
    [Parameter] public EventCallback<FocusEventArgs> OnFocus { get; set; }
    [Parameter] public EventCallback<FocusEventArgs> OnBlur { get; set; }
    [Parameter] public EventCallback<MouseEventArgs> OnClick { get; set; }
    [Parameter] public EventCallback OnShow { get; set; }
    [Parameter] public EventCallback OnHide { get; set; }
    [Parameter] public EventCallback OnClear { get; set; }

    // templates
    [Parameter] public RenderFragment<object>? ItemTemplate { get; set; }
    [Parameter] public RenderFragment<TValue>? SelectedItemTemplate { get; set; }
    [Parameter] public RenderFragment? OptionGroupIconTemplate { get; set; }
    [Parameter] public RenderFragment? DropdownIconTemplate { get; set; }
    [Parameter] public RenderFragment? LoadingIconTemplate { get; set; }
    [Parameter] public RenderFragment? ClearIconTemplate { get; set; }
    [Parameter] public RenderFragment? HeaderTemplate { get; set; }
    [Parameter] public RenderFragment? FooterTemplate { get; set; }

    [Parameter(CaptureUnmatchedValues = true)]
    public Dictionary<string, object>? AdditionalAttributes { get; set; }

    // ------------------------------------------------------------ lifecycle
    protected override void OnInitialized()
    {
        _id = InputId ?? $"op-cascadeselect-{Guid.NewGuid():N}";
    }

    // ------------------------------------------------------------ computed
    private IReadOnlyList<object> AllOptions => Options ?? System.Array.Empty<object>();

    private int GroupChildrenCount => OptionGroupChildren?.Count ?? 0;

    private bool HasSelection => SelectedOption is not null;

    private string RootClass => BuildClass(
        "p-cascadeselect p-component p-inputwrapper",
        Disabled ? "p-disabled" : null,
        Variant == "filled" ? "p-variant-filled" : null,
        _focus ? "p-focus" : null,
        Invalid ? "p-invalid" : null,
        HasSelection ? "p-inputwrapper-filled" : null,
        _focus || _overlayVisible ? "p-inputwrapper-focus" : null,
        Fluid ? "p-cascadeselect-fluid" : null,
        Size == "small" ? "p-cascadeselect-sm p-inputfield-sm" : null,
        Size == "large" ? "p-cascadeselect-lg p-inputfield-lg" : null,
        StyleClass);

    private string LabelClass => BuildClass(
        "p-cascadeselect-label",
        Placeholder is { Length: > 0 } && CurrentLabel == Placeholder ? "p-placeholder" : null,
        SelectedItemTemplate is null && string.IsNullOrEmpty(CurrentLabel)
            ? "p-cascadeselect-label-empty"
            : null);

    private string PanelClass => BuildClass(
        "p-cascadeselect-overlay p-component-overlay p-component",
        PanelStyleClass,
        _panelAnimationClass);

    private object? SelectedOption
    {
        get
        {
            if (Value is null) return null;
            return FindSelected(AllOptions, 0);
        }
    }

    private string CurrentLabel
    {
        get
        {
            var opt = SelectedOption;
            return opt is null ? string.Empty : GetOptionLabel(opt);
        }
    }

    private IReadOnlyList<object> CurrentList
    {
        get
        {
            if (_expandedPath.Count == 0) return AllOptions;
            var last = _expandedPath[^1];
            return GetChildren(last, _expandedPath.Count - 1);
        }
    }

    // ------------------------------------------------------------ option helpers
    private string GetOptionLabel(object option)
    {
        if (option is null) return string.Empty;
        if (string.IsNullOrEmpty(OptionLabel)) return option.ToString() ?? string.Empty;
        return GetPropertyValue(option, OptionLabel)?.ToString() ?? option.ToString() ?? string.Empty;
    }

    private object? GetOptionValue(object option)
    {
        if (option is null) return null;
        if (string.IsNullOrEmpty(OptionValue)) return option;
        return GetPropertyValue(option, OptionValue);
    }

    private bool IsOptionDisabled(object option)
    {
        if (option is null || string.IsNullOrEmpty(OptionDisabled)) return false;
        return GetPropertyValue(option, OptionDisabled) is bool b && b;
    }

    private string GetOptionGroupLabelValue(object group)
    {
        if (group is null) return string.Empty;
        return GetPropertyValue(group, OptionGroupLabel)?.ToString() ?? string.Empty;
    }

    private List<object> GetChildren(object node, int depth)
    {
        var list = new List<object>();
        if (node is null || depth >= GroupChildrenCount) return list;
        var propertyName = OptionGroupChildren![depth];
        if (string.IsNullOrEmpty(propertyName)) return list;
        if (GetPropertyValue(node, propertyName) is IEnumerable children)
        {
            foreach (var child in children)
            {
                if (child is not null) list.Add(child);
            }
        }

        return list;
    }

    private bool HasChildren(object node, int depth) => GetChildren(node, depth).Count > 0;

    private string GetNodeLabel(object node, int depth)
        => HasChildren(node, depth) ? GetOptionGroupLabelValue(node) : GetOptionLabel(node);

    private static object? GetPropertyValue(object obj, string propertyPath)
    {
        var type = obj.GetType();
        var property = type.GetProperty(propertyPath,
            BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
        return property?.GetValue(obj);
    }

    private object? FindSelected(IReadOnlyList<object> nodes, int depth)
    {
        foreach (var node in nodes)
        {
            var children = GetChildren(node, depth);
            if (children.Count == 0)
            {
                if (Equals(GetOptionValue(node), GetOptionValue((object)Value!)))
                {
                    return node;
                }
            }
            else
            {
                var result = FindSelected(children, depth + 1);
                if (result is not null) return result;
            }
        }

        return null;
    }

    private List<object>? FindPath(object target)
    {
        var path = new List<object>();
        return FindPathRecursive(AllOptions, 0, target, path) ? path : null;
    }

    private bool FindPathRecursive(IReadOnlyList<object> nodes, int depth, object target, List<object> path)
    {
        foreach (var node in nodes)
        {
            path.Add(node);
            if (ReferenceEquals(node, target)) return true;
            var children = GetChildren(node, depth);
            if (children.Count > 0 && FindPathRecursive(children, depth + 1, target, path)) return true;
            path.RemoveAt(path.Count - 1);
        }

        return false;
    }

    private int GetNodeDepth(object node) => (FindPath(node)?.Count ?? 1) - 1;

    private bool IsExpanded(object node) => _expandedPath.Any(n => ReferenceEquals(n, node));

    private bool IsFocused(object node) => ReferenceEquals(node, _focusedNode);

    private bool IsSelectedNode(object node, int depth)
        => !HasChildren(node, depth)
           && SelectedOption is not null
           && Equals(GetOptionValue(node), GetOptionValue(SelectedOption));

    // ------------------------------------------------------------ overlay
    private async Task OnLabelClick(MouseEventArgs e)
    {
        await OnClick.InvokeAsync(e);
        if (!_overlayVisible && !Disabled && !Readonly)
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
        if (ShowOnFocus && !_overlayVisible)
        {
            await OpenAsync();
        }
    }

    private async Task OnLabelBlur(FocusEventArgs e)
    {
        _focus = false;
        await OnBlur.InvokeAsync(e);
        StateHasChanged();
    }

    private async Task OpenAsync()
    {
        if (_overlayVisible || Disabled || Readonly) return;
        _overlayVisible = true;
        _panelRendered = true;
        _panelClosing = false;
        _panelAnimationClass = "p-anchored-overlay-enter-active";

        _expandedPath.Clear();
        var selected = SelectedOption;
        if (selected is not null)
        {
            var path = FindPath(selected);
            if (path is not null)
            {
                for (var i = 0; i < path.Count - 1; i++) _expandedPath.Add(path[i]);
            }

            _focusedNode = selected;
        }
        else
        {
            _focusedNode = AllOptions.FirstOrDefault(n => !IsOptionDisabled(n));
        }

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

    // ------------------------------------------------------------ options interaction
    private async Task OnNodeClick(object node)
    {
        if (IsOptionDisabled(node)) return;
        var depth = GetNodeDepth(node);
        if (HasChildren(node, depth))
        {
            ExpandTo(node);
        }
        else
        {
            await SelectNodeAsync(node);
        }
    }

    private void ExpandTo(object node)
    {
        var path = FindPath(node);
        _expandedPath.Clear();
        if (path is not null) _expandedPath.AddRange(path);
        var depth = _expandedPath.Count - 1;
        _focusedNode = GetChildren(node, depth).FirstOrDefault();
        StateHasChanged();
    }

    private async Task SelectNodeAsync(object node)
    {
        if (node is TValue tv)
        {
            Value = tv;
            await ValueChanged.InvokeAsync(tv);
            await OnChange.InvokeAsync(tv);
        }

        await CloseAsync();
    }

    private async Task Clear(MouseEventArgs? _ = null)
    {
        Value = default;
        await ValueChanged.InvokeAsync(default);
        await OnClear.InvokeAsync();
    }

    // ------------------------------------------------------------ keyboard
    private async Task OnLabelKeydown(KeyboardEventArgs e)
    {
        if (Disabled || Readonly || Loading) return;

        switch (e.Key)
        {
            case "ArrowDown":
                if (!_overlayVisible)
                {
                    await OpenAsync();
                }
                else
                {
                    MoveFocus(forward: true);
                }

                break;
            case "ArrowUp":
                if (_overlayVisible)
                {
                    MoveFocus(forward: false);
                }

                break;
            case "ArrowRight":
                if (_overlayVisible)
                {
                    ExpandFocused();
                }

                break;
            case "ArrowLeft":
                if (_overlayVisible)
                {
                    CollapseFocused();
                }

                break;
            case "Enter":
                if (_overlayVisible)
                {
                    await ActivateFocusedAsync();
                }
                else
                {
                    await OpenAsync();
                }

                break;
            case "Tab":
                if (_overlayVisible)
                {
                    await CloseAsync();
                }

                break;
            case "Home":
                if (_overlayVisible)
                {
                    _focusedNode = CurrentList.FirstOrDefault(n => !IsOptionDisabled(n));
                    StateHasChanged();
                }

                break;
            case "End":
                if (_overlayVisible)
                {
                    _focusedNode = CurrentList.LastOrDefault(n => !IsOptionDisabled(n));
                    StateHasChanged();
                }

                break;
            default:
                if (!string.IsNullOrEmpty(e.Key) && e.Key.Length == 1 && char.IsLetterOrDigit(e.Key[0]))
                {
                    if (!_overlayVisible) await OpenAsync();
                    SearchNode(e.Key);
                }

                break;
        }
    }

    private void MoveFocus(bool forward)
    {
        var list = CurrentList;
        if (list.Count == 0) return;

        var start = -1;
        if (_focusedNode is not null)
        {
            for (var i = 0; i < list.Count; i++)
            {
                if (ReferenceEquals(list[i], _focusedNode))
                {
                    start = i;
                    break;
                }
            }
        }

        var step = forward ? 1 : -1;
        for (var n = 0; n < list.Count; n++)
        {
            start += step;
            if (start >= list.Count) start = 0;
            if (start < 0) start = list.Count - 1;
            if (!IsOptionDisabled(list[start]))
            {
                _focusedNode = list[start];
                break;
            }
        }

        StateHasChanged();
    }

    private void ExpandFocused()
    {
        if (_focusedNode is null) return;
        var path = FindPath(_focusedNode);
        if (path is null) return;
        var depth = path.Count - 1;
        if (!HasChildren(_focusedNode, depth)) return;

        _expandedPath.Clear();
        _expandedPath.AddRange(path);
        _focusedNode = GetChildren(_focusedNode, depth).FirstOrDefault();
        StateHasChanged();
    }

    private void CollapseFocused()
    {
        if (_expandedPath.Count == 0) return;
        _focusedNode = _expandedPath[^1];
        _expandedPath.RemoveAt(_expandedPath.Count - 1);
        StateHasChanged();
    }

    private async Task ActivateFocusedAsync()
    {
        if (_focusedNode is null)
        {
            await CloseAsync();
            return;
        }

        var path = FindPath(_focusedNode);
        var depth = (path?.Count ?? 1) - 1;
        if (HasChildren(_focusedNode, depth))
        {
            ExpandFocused();
        }
        else
        {
            await SelectNodeAsync(_focusedNode);
        }
    }

    private void SearchNode(string key)
    {
        var list = CurrentList;
        if (list.Count == 0) return;

        var start = -1;
        if (_focusedNode is not null)
        {
            for (var i = 0; i < list.Count; i++)
            {
                if (ReferenceEquals(list[i], _focusedNode))
                {
                    start = i;
                    break;
                }
            }
        }

        var depth = _expandedPath.Count;
        for (var n = 0; n < list.Count; n++)
        {
            var idx = (start + 1 + n) % list.Count;
            if (GetNodeLabel(list[idx], depth).StartsWith(key, System.StringComparison.CurrentCultureIgnoreCase))
            {
                _focusedNode = list[idx];
                StateHasChanged();
                return;
            }
        }
    }

    // ------------------------------------------------------------ render tree
    private RenderFragment BuildTree() => builder => RenderOptionList(builder, 0, AllOptions);

    private void RenderOptionList(RenderTreeBuilder builder, int depth, IReadOnlyList<object> options)
    {
        builder.OpenElement(0, "ul");
        builder.AddAttribute(1, "class", depth == 0 ? "p-cascadeselect-list" : "p-cascadeselect-option-list");
        builder.AddAttribute(2, "role", depth == 0 ? "tree" : "group");
        if (depth == 0)
        {
            builder.AddAttribute(3, "id", $"{_id}_tree");
        }

        for (var i = 0; i < options.Count; i++)
        {
            RenderOption(builder, depth, options[i], i, options.Count);
        }

        builder.CloseElement();
    }

    private void RenderOption(RenderTreeBuilder builder, int depth, object option, int index, int setSize)
    {
        var isGroup = HasChildren(option, depth);
        var disabled = IsOptionDisabled(option);
        var expanded = IsExpanded(option);
        var selected = IsSelectedNode(option, depth);
        var focused = IsFocused(option);

        var optionClass = BuildClass(
            "p-cascadeselect-option",
            expanded ? "p-cascadeselect-option-active" : null,
            selected ? "p-cascadeselect-option-selected" : null,
            disabled ? "p-disabled" : null,
            focused ? "p-focus" : null);

        builder.OpenElement(0, "li");
        builder.AddAttribute(1, "class", optionClass);
        builder.AddAttribute(2, "role", "treeitem");
        builder.AddAttribute(3, "aria-label", GetNodeLabel(option, depth));
        builder.AddAttribute(4, "aria-selected", selected ? "true" : "false");
        builder.AddAttribute(5, "aria-expanded", isGroup ? (expanded ? "true" : "false") : null);
        builder.AddAttribute(6, "aria-level", depth + 1);
        builder.AddAttribute(7, "aria-setsize", setSize);
        builder.AddAttribute(8, "aria-posinset", index + 1);
        builder.AddAttribute(9, "onclick",
            EventCallback.Factory.Create<MouseEventArgs>(this, _ => OnNodeClick(option)));

        builder.OpenElement(10, "div");
        builder.AddAttribute(11, "class", "p-cascadeselect-option-content");

        if (ItemTemplate is not null)
        {
            builder.AddContent(12, ItemTemplate(option));
        }
        else
        {
            builder.OpenElement(13, "span");
            builder.AddContent(14, GetNodeLabel(option, depth));
            builder.CloseElement();
        }

        if (isGroup)
        {
            builder.OpenElement(15, "span");
            builder.AddAttribute(16, "class", "p-cascadeselect-group-icon-container");
            if (OptionGroupIconTemplate is not null)
            {
                builder.AddContent(17, OptionGroupIconTemplate);
            }
            else
            {
                builder.OpenComponent<OpIcon>(18);
                builder.AddAttribute(19, "Name", "chevron-right");
                builder.AddAttribute(20, "CssClass", "p-cascadeselect-group-icon");
                builder.CloseComponent();
            }

            builder.CloseElement();
        }

        builder.CloseElement();

        if (isGroup)
        {
            RenderOptionList(builder, depth + 1, GetChildren(option, depth));
        }

        builder.CloseElement();
    }

    private static string BuildClass(params string?[] classes)
        => string.Join(' ', classes.Where(c => !string.IsNullOrWhiteSpace(c)));
}
