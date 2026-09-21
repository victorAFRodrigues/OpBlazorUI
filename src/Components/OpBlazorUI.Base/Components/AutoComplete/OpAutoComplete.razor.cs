using System.Reflection;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.Web.Virtualization;
using OpBlazorUI.Base.Models;

namespace OpBlazorUI.Base.Components.AutoComplete;

public partial class OpAutoComplete<TValue> : ComponentBase
{
    private string _id = "";

    private bool _overlayVisible;
    private bool _panelRendered;
    private bool _panelClosing;
    private string? _panelAnimationClass;
    private bool _focus;
    private bool _focusInside;
    private DateTime _lastPanelPointerDown = DateTime.MinValue;
    private int _focusedOptionIndex = -1;
    private string _searchValue = "";
    private string? _inputValue;
    private TValue? _prevValue;
    private bool _autoFocused;

    private ElementReference _inputRef;

    // ---------------------------------------------------------------- params
    [Parameter] public IReadOnlyList<object>? Suggestions { get; set; }
    [Parameter] public string? OptionLabel { get; set; }
    [Parameter] public string? OptionValue { get; set; }
    [Parameter] public string? OptionDisabled { get; set; }
    [Parameter] public TValue? Value { get; set; }
    [Parameter] public EventCallback<TValue?> ValueChanged { get; set; }
    [Parameter] public bool Multiple { get; set; }
    [Parameter] public IReadOnlyList<TValue>? MultipleValue { get; set; }
    [Parameter] public EventCallback<IReadOnlyList<TValue>?> MultipleValueChanged { get; set; }
    [Parameter] public string? Placeholder { get; set; }
    [Parameter] public bool Dropdown { get; set; }
    [Parameter] public string DropdownMode { get; set; } = "blank";
    [Parameter] public bool ShowClear { get; set; }
    [Parameter] public bool Disabled { get; set; }
    [Parameter] public bool Readonly { get; set; }
    [Parameter] public bool Invalid { get; set; }
    [Parameter] public bool Required { get; set; }
    [Parameter] public bool Group { get; set; }
    [Parameter] public string OptionGroupLabel { get; set; } = "label";
    [Parameter] public string OptionGroupChildren { get; set; } = "items";
    [Parameter] public bool ForceSelection { get; set; }
    [Parameter] public bool Typeahead { get; set; } = true;
    [Parameter] public bool AddOnBlur { get; set; }
    [Parameter] public bool AddOnTab { get; set; }
    [Parameter] public string? Separator { get; set; }
    [Parameter] public int MinLength { get; set; } = 1;
    [Parameter] public string Variant { get; set; } = "outlined";
    [Parameter] public string? Size { get; set; }
    [Parameter] public bool Fluid { get; set; }
    [Parameter] public string? ScrollHeight { get; set; }
    [Parameter] public bool Loading { get; set; }
    [Parameter] public string? EmptyMessage { get; set; } = "No results found";
    [Parameter] public int TabIndex { get; set; }
    [Parameter] public string? AriaLabel { get; set; }
    [Parameter] public string? AriaLabelledBy { get; set; }
    [Parameter] public string? InputId { get; set; }
    [Parameter] public bool AutoFocus { get; set; }
    [Parameter] public string? StyleClass { get; set; }
    [Parameter] public string? PanelStyleClass { get; set; }

    // virtual scroll
    [Parameter] public bool VirtualScroll { get; set; }
    [Parameter] public float VirtualScrollItemSize { get; set; } = 34;
    [Parameter] public int VirtualScrollOverscan { get; set; } = 3;
    [Parameter] public ItemsProviderDelegate<object>? ItemsProvider { get; set; }

    // events
    [Parameter] public EventCallback<OpAutoCompleteCompleteEvent> CompleteMethod { get; set; }
    [Parameter] public EventCallback<TValue?> OnChange { get; set; }
    [Parameter] public EventCallback<OpAutoCompleteSelectEvent> OnSelect { get; set; }
    [Parameter] public EventCallback<OpAutoCompleteSelectEvent> OnUnselect { get; set; }
    [Parameter] public EventCallback<FocusEventArgs> OnFocus { get; set; }
    [Parameter] public EventCallback<FocusEventArgs> OnBlur { get; set; }
    [Parameter] public EventCallback OnShow { get; set; }
    [Parameter] public EventCallback OnHide { get; set; }
    [Parameter] public EventCallback OnClear { get; set; }
    [Parameter] public EventCallback<MouseEventArgs> OnDropdownClick { get; set; }

    // templates
    [Parameter] public RenderFragment<TValue>? ItemTemplate { get; set; }
    [Parameter] public RenderFragment<TValue>? SelectedItemTemplate { get; set; }
    [Parameter] public RenderFragment<object>? GroupTemplate { get; set; }
    [Parameter] public RenderFragment? HeaderTemplate { get; set; }
    [Parameter] public RenderFragment? FooterTemplate { get; set; }
    [Parameter] public RenderFragment? EmptyTemplate { get; set; }
    [Parameter] public RenderFragment? LoadingIconTemplate { get; set; }
    [Parameter] public RenderFragment? DropdownIconTemplate { get; set; }
    [Parameter] public RenderFragment? ClearIconTemplate { get; set; }

    [Parameter(CaptureUnmatchedValues = true)]
    public Dictionary<string, object>? AdditionalAttributes { get; set; }

    // ------------------------------------------------------------ lifecycle
    protected override void OnInitialized()
    {
        _id = InputId ?? $"op-ac-{Guid.NewGuid():N}";
    }

    protected override void OnParametersSet()
    {
        if (!Multiple && !Equals(_prevValue, Value))
        {
            _inputValue = GetLabelFromValue(Value);
            _prevValue = Value;
        }
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (AutoFocus && !_autoFocused && !Disabled)
        {
            _autoFocused = true;
            await _inputRef.FocusAsync();
        }
    }

    // ------------------------------------------------------------ computed
    private string RootClass => BuildClass(
        "p-autocomplete p-component p-inputwrapper",
        Disabled ? "p-disabled" : null,
        Invalid ? "p-invalid" : null,
        Variant == "filled" ? "p-variant-filled" : null,
        _focus ? "p-focus" : null,
        HasSelection ? "p-inputwrapper-filled" : null,
        _focus || _overlayVisible ? "p-inputwrapper-focus" : null,
        _overlayVisible ? "p-autocomplete-open" : null,
        Fluid ? "p-autocomplete-fluid" : null,
        StyleClass);

    private string InputClass => BuildClass(
        "p-autocomplete-input p-inputtext",
        Size == "small" ? "p-inputtext-sm" : null,
        Size == "large" ? "p-inputtext-lg" : null,
        Invalid ? "p-invalid" : null,
        Variant == "filled" ? "p-variant-filled" : null);

    private string MultipleClass => BuildClass(
        "p-autocomplete-input-multiple",
        Size == "small" ? "p-inputtext-sm" : null,
        Size == "large" ? "p-inputtext-lg" : null,
        Invalid ? "p-invalid" : null,
        Variant == "filled" ? "p-variant-filled" : null,
        Disabled ? "p-disabled" : null);

    private string PanelClass => BuildClass(
        "p-autocomplete-overlay p-component-overlay p-component",
        PanelStyleClass,
        _panelAnimationClass);

    private bool HasSelection => Multiple ? SelectedOptions.Count > 0 : Value is not null;

    private List<TValue> SelectedOptions
    {
        get
        {
            var list = new List<TValue>();
            var values = MultipleValue ?? Array.Empty<TValue>();
            foreach (var v in values)
            {
                list.Add(v);
            }

            return list;
        }
    }

    private IEnumerable<object> AllSuggestions => Suggestions ?? Array.Empty<object>();

    private bool ShowEmptyMessage => !AllSuggestions.Any() && !VirtualScroll;

    // ------------------------------------------------------------ option helpers
    private string GetOptionLabel(object option)
    {
        if (option is null) return string.Empty;
        if (string.IsNullOrEmpty(OptionLabel)) return option is string s ? s : option.ToString() ?? string.Empty;
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

    private string GetLabelFromValue(TValue? value)
    {
        if (value is null) return string.Empty;
        foreach (var opt in AllSuggestions)
        {
            if (Equals(GetOptionValue(opt), GetOptionValue((object)value!)))
            {
                return GetOptionLabel(opt);
            }
        }

        return value is string s ? s : value.ToString() ?? string.Empty;
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

    private bool IsSelected(object option)
    {
        if (Multiple)
        {
            foreach (var v in SelectedOptions)
            {
                if (Equals(GetOptionValue(option), GetOptionValue((object)v!))) return true;
            }

            return false;
        }

        return Value is not null && Equals(GetOptionValue(option), GetOptionValue((object)Value));
    }

    private string OptionClass(object option)
    {
        return BuildClass(
            "p-autocomplete-option",
            IsSelected(option) ? "p-autocomplete-option-selected" : null,
            IsOptionDisabled(option) ? "p-disabled" : null);
    }

    // ------------------------------------------------------------ overlay
    private async Task OpenAsync()
    {
        if (_overlayVisible || Disabled || Readonly) return;
        _overlayVisible = true;
        _panelRendered = true;
        _panelClosing = false;
        _panelAnimationClass = "p-anchored-overlay-enter-active";
        _focusedOptionIndex = -1;
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

    // ------------------------------------------------------------ input events
    private async Task OnSingleInput(ChangeEventArgs e)
    {
        if (Disabled || Readonly) return;
        _inputValue = e.Value?.ToString() ?? string.Empty;
        _focusedOptionIndex = -1;
        await SearchAsync(_inputValue);
    }

    private async Task OnMultipleInput(ChangeEventArgs e)
    {
        if (Disabled || Readonly) return;
        _searchValue = e.Value?.ToString() ?? string.Empty;
        _focusedOptionIndex = -1;
        await SearchAsync(_searchValue);
    }

    private async Task SearchAsync(string query)
    {
        if (query.Length < MinLength)
        {
            if (!_overlayVisible) return;
            await CloseAsync();
            return;
        }

        if (!_overlayVisible)
        {
            await OpenAsync();
        }

        await CompleteMethod.InvokeAsync(new OpAutoCompleteCompleteEvent { Query = query });
    }

    private async Task OnInputFocus(FocusEventArgs e)
    {
        if (Disabled) return;
        _focus = true;
        await OnFocus.InvokeAsync(e);
        var query = Multiple ? _searchValue : (_inputValue ?? string.Empty);
        await CompleteMethod.InvokeAsync(new OpAutoCompleteCompleteEvent { Query = query });
        if (!_overlayVisible && !Multiple)
        {
            await OpenAsync();
        }
    }

    private async Task OnInputBlur(FocusEventArgs e)
    {
        _focus = false;
        await OnBlur.InvokeAsync(e);

        if (Multiple)
        {
            if (AddOnBlur && !string.IsNullOrWhiteSpace(_searchValue))
            {
                await AddChipAsync(_searchValue);
            }
        }
        else if (ForceSelection && !string.IsNullOrEmpty(_inputValue))
        {
            var matches = AllSuggestions.Any(o => GetOptionLabel(o) == _inputValue);
            if (!matches)
            {
                _inputValue = string.Empty;
                Value = default;
                await ValueChanged.InvokeAsync(default);
                await OnChange.InvokeAsync(default);
            }
        }

        StateHasChanged();
    }

    // ------------------------------------------------------------ dropdown
    private async Task OnDropdownClickHandler(MouseEventArgs e)
    {
        await OnDropdownClick.InvokeAsync(e);
        if (Disabled) return;

        var query = DropdownMode == "current" ? (Multiple ? _searchValue : (_inputValue ?? string.Empty)) : string.Empty;
        await CompleteMethod.InvokeAsync(new OpAutoCompleteCompleteEvent { Query = query });
        await OpenAsync();
        await _inputRef.FocusAsync();
    }

    // ------------------------------------------------------------ keyboard
    private async Task OnInputKeydown(KeyboardEventArgs e)
    {
        if (Disabled || Readonly) return;

        if (Multiple)
        {
            if (!string.IsNullOrEmpty(Separator) && e.Key == Separator)
            {
                await AddChipAsync(_searchValue);
                return;
            }

            if (e.Key == "Backspace" && string.IsNullOrEmpty(_searchValue) && SelectedOptions.Count > 0)
            {
                await RemoveChipAsync(SelectedOptions[^1]);
                return;
            }

            if (e.Key == "Tab" && AddOnTab && !string.IsNullOrWhiteSpace(_searchValue))
            {
                await AddChipAsync(_searchValue);
                return;
            }
        }

        switch (e.Key)
        {
            case "ArrowDown":
                if (!_overlayVisible)
                {
                    await OpenAsync();
                }
                else if (!VirtualScroll)
                {
                    _focusedOptionIndex = FindNextOptionIndex(_focusedOptionIndex);
                    StateHasChanged();
                }

                break;
            case "ArrowUp":
                if (_overlayVisible && !VirtualScroll)
                {
                    _focusedOptionIndex = FindNextOptionIndex(_focusedOptionIndex, backward: true);
                    StateHasChanged();
                }

                break;
            case "Enter":
                if (_overlayVisible && !VirtualScroll)
                {
                    await SelectFocusedOptionAsync();
                }
                else if (_overlayVisible)
                {
                    await CloseAsync();
                }
                else if (Multiple && !string.IsNullOrWhiteSpace(_searchValue))
                {
                    await AddChipAsync(_searchValue);
                }

                break;
            case "Home":
                if (_overlayVisible && !VirtualScroll)
                {
                    _focusedOptionIndex = FindFirstEnabledIndex();
                    StateHasChanged();
                }

                break;
            case "End":
                if (_overlayVisible && !VirtualScroll)
                {
                    _focusedOptionIndex = FindLastEnabledIndex();
                    StateHasChanged();
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

    private async Task SelectFocusedOptionAsync()
    {
        var visible = VisibleOptions;
        if (_focusedOptionIndex >= 0 && _focusedOptionIndex < visible.Count)
        {
            var option = visible[_focusedOptionIndex];
            if (!IsOptionDisabled(option))
            {
                await OnOptionClick(option);
                return;
            }
        }

        await CloseAsync();
    }

    private List<object> VisibleOptions
    {
        get
        {
            var list = new List<object>();
            if (Group)
            {
                foreach (var opt in AllSuggestions)
                {
                    if (IsOptionGroup(opt)) list.Add(opt);
                    else list.Add(opt);
                }
            }
            else
            {
                foreach (var opt in AllSuggestions)
                {
                    list.Add(opt);
                }
            }

            return list;
        }
    }

    // ------------------------------------------------------------ selection
    private async Task OnOptionClick(object option)
    {
        if (IsOptionDisabled(option)) return;

        if (Multiple)
        {
            if (option is TValue tv)
            {
                await ToggleOptionAsync(tv);
                _searchValue = "";
                await _inputRef.FocusAsync();
            }
        }
        else
        {
            if (option is TValue tv)
            {
                Value = tv;
                _inputValue = GetOptionLabel(option);
                await ValueChanged.InvokeAsync(tv);
                await OnChange.InvokeAsync(tv);
                await OnSelect.InvokeAsync(new OpAutoCompleteSelectEvent { Option = option });
            }

            await CloseAsync();
        }
    }

    private async Task ToggleOptionAsync(TValue option)
    {
        var list = SelectedOptions;
        var existing = list.FirstOrDefault(v => Equals(GetOptionValue((object)v!), GetOptionValue((object)option)));
        if (existing is not null)
        {
            list.Remove(existing);
            await OnUnselect.InvokeAsync(new OpAutoCompleteSelectEvent { Option = option });
        }
        else
        {
            list.Add(option);
            await OnSelect.InvokeAsync(new OpAutoCompleteSelectEvent { Option = option });
        }

        MultipleValue = list;
        await MultipleValueChanged.InvokeAsync(list);
        await OnChange.InvokeAsync(option);
        StateHasChanged();
    }

    private async Task RemoveChipAsync(TValue option)
    {
        var list = SelectedOptions;
        var existing = list.FirstOrDefault(v => Equals(GetOptionValue((object)v!), GetOptionValue((object)option)));
        if (existing is not null)
        {
            list.Remove(existing);
            await OnUnselect.InvokeAsync(new OpAutoCompleteSelectEvent { Option = option });
        }

        MultipleValue = list;
        await MultipleValueChanged.InvokeAsync(list);
        await OnChange.InvokeAsync(option);
    }

    private async Task AddChipAsync(string text)
    {
        if (string.IsNullOrWhiteSpace(text)) return;
        var token = text.Trim();

        if (token is TValue tv)
        {
            var list = SelectedOptions;
            var existing = list.FirstOrDefault(v => Equals(GetOptionValue((object)v!), GetOptionValue((object)tv)));
            if (existing is null)
            {
                list.Add(tv);
                MultipleValue = list;
                await MultipleValueChanged.InvokeAsync(list);
                await OnChange.InvokeAsync(tv);
                await OnSelect.InvokeAsync(new OpAutoCompleteSelectEvent { Option = tv });
            }
        }

        _searchValue = "";
        StateHasChanged();
    }

    private async Task Clear(MouseEventArgs? _ = null)
    {
        if (Multiple)
        {
            MultipleValue = Array.Empty<TValue>();
            _searchValue = "";
            await MultipleValueChanged.InvokeAsync(Array.Empty<TValue>());
        }
        else
        {
            Value = default;
            _inputValue = string.Empty;
            await ValueChanged.InvokeAsync(default);
        }

        await OnClear.InvokeAsync();
    }

    private static string BuildClass(params string?[] classes)
        => string.Join(' ', classes.Where(c => !string.IsNullOrWhiteSpace(c)));
}
