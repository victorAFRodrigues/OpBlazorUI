using System.Reflection;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;

namespace OpBlazorUI.Base.Components.Select;

public partial class OpSelect<TValue> : ComponentBase
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
    private int _focusedOptionIndex = -1;

    // ---------------------------------------------------------------- params
    [Parameter] public IReadOnlyList<object>? Options { get; set; }
    [Parameter] public string? OptionLabel { get; set; }
    [Parameter] public string? OptionValue { get; set; }
    [Parameter] public string? OptionDisabled { get; set; }
    [Parameter] public TValue? Value { get; set; }
    [Parameter] public EventCallback<TValue?> ValueChanged { get; set; }
    [Parameter] public string? Placeholder { get; set; }
    [Parameter] public bool Filter { get; set; }
    [Parameter] public string? FilterBy { get; set; }
    [Parameter] public bool Checkmark { get; set; }
    [Parameter] public bool ShowClear { get; set; }
    [Parameter] public bool Editable { get; set; }
    [Parameter] public bool Readonly { get; set; }
    [Parameter] public bool Disabled { get; set; }
    [Parameter] public bool Invalid { get; set; }
    [Parameter] public bool Required { get; set; }
    [Parameter] public bool Group { get; set; }
    [Parameter] public string OptionGroupLabel { get; set; } = "label";
    [Parameter] public string OptionGroupChildren { get; set; } = "items";
    [Parameter] public string Variant { get; set; } = "outlined";
    [Parameter] public string? Size { get; set; }
    [Parameter] public bool Fluid { get; set; }
    [Parameter] public string? ScrollHeight { get; set; }
    [Parameter] public bool VirtualScroll { get; set; }
    [Parameter] public int VirtualScrollItemSize { get; set; } = 38;
    [Parameter] public bool Loading { get; set; }
    [Parameter] public string? DropdownIcon { get; set; }
    [Parameter] public string? EmptyMessage { get; set; } = "No results found";
    [Parameter] public string? EmptyFilterMessage { get; set; } = "No results found";
    [Parameter] public int TabIndex { get; set; }
    [Parameter] public string? AriaLabel { get; set; }
    [Parameter] public string? InputId { get; set; }
    [Parameter] public bool ShowOnFocus { get; set; } = true;
    [Parameter] public string? PanelStyleClass { get; set; }
    [Parameter] public string? StyleClass { get; set; }

    // events
    [Parameter] public EventCallback<TValue?> OnChange { get; set; }
    [Parameter] public EventCallback<string> OnFilter { get; set; }
    [Parameter] public EventCallback<FocusEventArgs> OnFocus { get; set; }
    [Parameter] public EventCallback<FocusEventArgs> OnBlur { get; set; }
    [Parameter] public EventCallback<MouseEventArgs> OnClick { get; set; }
    [Parameter] public EventCallback OnShow { get; set; }
    [Parameter] public EventCallback OnHide { get; set; }
    [Parameter] public EventCallback OnClear { get; set; }

    // templates
    [Parameter] public RenderFragment<TValue>? ItemTemplate { get; set; }
    [Parameter] public RenderFragment<TValue>? SelectedItemTemplate { get; set; }
    [Parameter] public RenderFragment<object>? GroupTemplate { get; set; }
    [Parameter] public RenderFragment? HeaderTemplate { get; set; }
    [Parameter] public RenderFragment? FooterTemplate { get; set; }
    [Parameter] public RenderFragment? EmptyTemplate { get; set; }
    [Parameter] public RenderFragment? EmptyFilterTemplate { get; set; }
    [Parameter] public RenderFragment? DropdownIconTemplate { get; set; }
    [Parameter] public RenderFragment? LoadingIconTemplate { get; set; }
    [Parameter] public RenderFragment? ClearIconTemplate { get; set; }

    [Parameter(CaptureUnmatchedValues = true)]
    public Dictionary<string, object>? AdditionalAttributes { get; set; }

    // ------------------------------------------------------------ lifecycle
    protected override void OnInitialized()
    {
        _id = InputId ?? $"op-select-{Guid.NewGuid():N}";
    }

    // ------------------------------------------------------------ computed
    private string RootClass => BuildClass(
        "p-select p-component p-inputwrapper",
        Disabled ? "p-disabled" : null,
        Variant == "filled" ? "p-variant-filled" : null,
        _focus ? "p-focus" : null,
        Invalid ? "p-invalid" : null,
        HasSelection ? "p-inputwrapper-filled" : null,
        _focus || _overlayVisible ? "p-inputwrapper-focus" : null,
        _overlayVisible ? "p-select-open" : null,
        Fluid ? "p-select-fluid" : null,
        Size == "small" ? "p-select-sm p-inputfield-sm" : null,
        Size == "large" ? "p-select-lg p-inputfield-lg" : null,
        StyleClass);

    private string LabelClass => BuildClass(
        "p-select-label",
        Placeholder is { Length: > 0 } && CurrentLabel == Placeholder ? "p-placeholder" : null,
        !Editable && SelectedItemTemplate is null && string.IsNullOrEmpty(CurrentLabel)
            ? "p-select-label-empty"
            : null);

    private string PanelClass => BuildClass(
        "p-select-overlay p-component-overlay p-component",
        PanelStyleClass,
        _panelAnimationClass);

    private bool HasSelection => SelectedOption is not null;

    private IEnumerable<object> AllOptions => Options ?? Array.Empty<object>();

    private object? SelectedOption
    {
        get
        {
            if (Value is null) return null;
            foreach (var opt in AllOptions)
            {
                if (Group && IsOptionGroup(opt))
                {
                    foreach (var child in GetOptionGroupChildren(opt))
                    {
                        if (Equals(GetOptionValue(child), GetOptionValue((object)Value)))
                        {
                            return child;
                        }
                    }
                }
                else if (Equals(GetOptionValue(opt), GetOptionValue((object)Value)))
                {
                    return opt;
                }
            }

            return null;
        }
    }

    private string CurrentLabel
    {
        get
        {
            var opt = SelectedOption;
            if (opt is null) return string.Empty;
            return GetOptionLabel(opt);
        }
    }

    private List<object> VisibleOptions
    {
        get
        {
            var list = new List<object>();
            foreach (var opt in AllOptions)
            {
                if (Group && IsOptionGroup(opt))
                {
                    foreach (var child in GetOptionGroupChildren(opt))
                    {
                        if (MatchesFilter(child))
                        {
                            list.Add(opt);
                            break;
                        }
                    }
                }
                else if (!Group && MatchesFilter(opt))
                {
                    list.Add(opt);
                }
            }

            return list;
        }
    }

    private bool MatchesFilter(object option)
    {
        if (!Filter || string.IsNullOrEmpty(_filterValue)) return true;
        var label = GetOptionLabel(option);
        if (label.Contains(_filterValue, StringComparison.CurrentCultureIgnoreCase)) return true;
        if (!string.IsNullOrEmpty(FilterBy))
        {
            var value = GetPropertyValue(option, FilterBy)?.ToString();
            if (value is not null && value.Contains(_filterValue, StringComparison.CurrentCultureIgnoreCase))
                return true;
        }

        return false;
    }

    private bool HasVisibleOptions
    {
        get
        {
            if (!Group) return VisibleOptions.Count > 0;
            foreach (var g in VisibleOptions)
            {
                if (GetOptionGroupChildren(g).Count > 0) return true;
            }

            return false;
        }
    }

    private bool ShowEmptyMessage => !HasVisibleOptions;

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

    private bool IsOptionGroup(object option)
    {
        if (option is null || !Group) return false;
        return GetPropertyValue(option, OptionGroupChildren) is not null;
    }

    private string GetOptionGroupLabelValue(object group)
    {
        if (group is null) return string.Empty;
        return GetPropertyValue(group, OptionGroupLabel)?.ToString() ?? string.Empty;
    }

    private List<object> GetOptionGroupChildren(object group)
    {
        var list = new List<object>();
        if (group is null) return list;
        var children = GetPropertyValue(group, OptionGroupChildren) as System.Collections.IEnumerable;
        if (children is null) return list;
        foreach (var child in children)
        {
            if (child is not null) list.Add(child);
        }

        return list;
    }

    private static object? GetPropertyValue(object obj, string propertyPath)
    {
        var type = obj.GetType();
        var property = type.GetProperty(propertyPath,
            BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
        return property?.GetValue(obj);
    }

    private bool IsSelected(object option) =>
        SelectedOption is not null && Equals(GetOptionValue(option), GetOptionValue(SelectedOption));

    private string OptionClass(object option)
    {
        var isDisabled = IsOptionDisabled(option);
        var isSel = IsSelected(option);
        return BuildClass(
            "p-select-option",
            isSel && !Checkmark ? "p-select-option-selected" : null,
            isDisabled ? "p-disabled" : null);
    }

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
        if (!Filter)
        {
            _filterValue = "";
        }

        _focusedOptionIndex = FindSelectedOptionIndex();
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
            _focusedOptionIndex = -1;
            await InvokeAsync(StateHasChanged);
        }
    }

    private async Task OnPanelAnimationEnd()
    {
        if (_panelClosing)
        {
            _panelRendered = false;
            _panelClosing = false;
            _focusedOptionIndex = -1;
        }

        _panelAnimationClass = null;
        await InvokeAsync(StateHasChanged);
    }

    private void OnRootFocusIn(FocusEventArgs e)
    {
        _focusInside = true;
    }

    /// <summary>
    /// Marca a interação com o painel. As opções não são focáveis, então clicar nelas
    /// dispara <c>focusout</c> no gatilho e fecharia o overlay de forma prematura
    /// (ex.: ao rolar a lista ou clicar no ícone de limpar).
    /// </summary>
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
    private async Task OnOptionClick(object option)
    {
        if (IsOptionDisabled(option)) return;
        await SelectOptionAsync(option);
    }

    private void OnOptionMouseEnter(int index)
    {
        _focusedOptionIndex = index;
        StateHasChanged();
    }

    private async Task SelectOptionAsync(object option)
    {
        if (option is TValue tv)
        {
            Value = tv;
            await ValueChanged.InvokeAsync(tv);
            await OnChange.InvokeAsync(tv);
        }

        if (Filter)
        {
            _filterValue = "";
        }

        await CloseAsync();
    }

    private int FindSelectedOptionIndex()
    {
        var visible = VisibleOptions;
        for (var i = 0; i < visible.Count; i++)
        {
            if (!Group && IsSelected(visible[i])) return i;
        }

        return -1;
    }

    private int FindNextOptionIndex(int fromIndex, bool backward = false)
    {
        var visible = VisibleOptions;
        if (visible.Count == 0) return -1;
        var step = backward ? -1 : 1;
        var i = fromIndex;
        for (var n = 0; n < visible.Count; n++)
        {
            i = (i + step + visible.Count) % visible.Count;
            if (!IsOptionDisabled(visible[i])) return i;
        }

        return -1;
    }

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
                    _focusedOptionIndex = FindNextOptionIndex(_focusedOptionIndex);
                    StateHasChanged();
                }

                break;
            case "ArrowUp":
                if (_overlayVisible)
                {
                    _focusedOptionIndex = FindNextOptionIndex(_focusedOptionIndex, backward: true);
                    StateHasChanged();
                }

                break;
            case "Enter":
                if (_overlayVisible)
                {
                    await SelectFocusedOptionAsync();
                }
                else
                {
                    await OpenAsync();
                }

                break;
            case "Tab":
                if (_overlayVisible)
                {
                    await SelectFocusedOptionAsync();
                }

                break;
            case "Home":
                if (_overlayVisible)
                {
                    _focusedOptionIndex = FindFirstEnabledIndex();
                    StateHasChanged();
                }

                break;
            case "End":
                if (_overlayVisible)
                {
                    _focusedOptionIndex = FindLastEnabledIndex();
                    StateHasChanged();
                }

                break;
            default:
                if (!string.IsNullOrEmpty(e.Key) && e.Key.Length == 1 && char.IsLetterOrDigit(e.Key[0]))
                {
                    if (!_overlayVisible) await OpenAsync();
                    SearchOptions(e.Key);
                }

                break;
        }
    }

    private int FindFirstEnabledIndex()
    {
        var visible = VisibleOptions;
        for (var i = 0; i < visible.Count; i++)
        {
            if (!IsOptionDisabled(visible[i])) return i;
        }

        return -1;
    }

    private int FindLastEnabledIndex()
    {
        var visible = VisibleOptions;
        for (var i = visible.Count - 1; i >= 0; i--)
        {
            if (!IsOptionDisabled(visible[i])) return i;
        }

        return -1;
    }

    private void SearchOptions(string key)
    {
        var visible = VisibleOptions;
        for (var i = 0; i < visible.Count; i++)
        {
            var idx = (_focusedOptionIndex + 1 + i) % visible.Count;
            if (GetOptionLabel(visible[idx]).StartsWith(key, StringComparison.CurrentCultureIgnoreCase))
            {
                _focusedOptionIndex = idx;
                StateHasChanged();
                return;
            }
        }
    }

    private async Task SelectFocusedOptionAsync()
    {
        var visible = VisibleOptions;
        if (_focusedOptionIndex >= 0 && _focusedOptionIndex < visible.Count)
        {
            var option = visible[_focusedOptionIndex];
            if (!IsOptionDisabled(option))
            {
                await SelectOptionAsync(option);
                return;
            }
        }

        await CloseAsync();
    }

    private async Task OnFilterInput(ChangeEventArgs e)
    {
        _filterValue = e.Value?.ToString() ?? string.Empty;
        _focusedOptionIndex = -1;
        await OnFilter.InvokeAsync(_filterValue);
    }

    private async Task Clear(MouseEventArgs? _ = null)
    {
        Value = default;
        await ValueChanged.InvokeAsync(default);
        await OnClear.InvokeAsync();
    }

    private static string BuildClass(params string?[] classes)
        => string.Join(' ', classes.Where(c => !string.IsNullOrWhiteSpace(c)));
}