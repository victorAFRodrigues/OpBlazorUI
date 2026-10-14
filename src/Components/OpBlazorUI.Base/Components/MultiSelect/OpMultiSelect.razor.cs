using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using OpBlazorUI.Base.Components.Forms;
using OpBlazorUI.Base.Components.Select;

namespace OpBlazorUI.Base.Components.MultiSelect;

public partial class OpMultiSelect<TValue> : OpInputBase<IReadOnlyList<TValue>>
{
    private ElementReference _rootRef;
    private ElementReference _listRef;

    private string _id = "";

    private bool _overlayVisible;
    private bool _panelRendered;
    private bool _panelClosing;
    private string? _panelAnimationClass;
    private bool _focus;
    private string _filterValue = "";
    private int _focusedOptionIndex = -1;

    private ElementReference _inputRef;

    // ---------------------------------------------------------------- params
    [Parameter] public IReadOnlyList<object>? Options { get; set; }
    [Parameter] public string? OptionLabel { get; set; }
    [Parameter] public string? OptionValue { get; set; }
    [Parameter] public string? OptionDisabled { get; set; }
    [Parameter] public string? Placeholder { get; set; }
    [Parameter] public string Display { get; set; } = "comma";
    [Parameter] public int? MaxSelectedLabels { get; set; }

    /// <summary>Número máximo de itens que podem ser selecionados (null = sem limite).</summary>
    [Parameter] public int? SelectionLimit { get; set; }

    /// <summary>Texto exibido quando <see cref="MaxSelectedLabels"/> é excedido. Use <c>{0}</c> para o total.</summary>
    [Parameter] public string? SelectedItemsLabel { get; set; }
    [Parameter] public bool Filter { get; set; }
    [Parameter] public bool ShowHeader { get; set; } = true;
    [Parameter] public bool ShowToggleAll { get; set; } = true;
    [Parameter] public bool HighlightOnSelect { get; set; } = true;
    [Parameter] public bool ShowClear { get; set; }
    [Parameter] public bool Readonly { get; set; }
    [Parameter] public bool Required { get; set; }
    [Parameter] public bool Group { get; set; }
    [Parameter] public string OptionGroupLabel { get; set; } = "label";
    [Parameter] public string OptionGroupChildren { get; set; } = "items";
    [Parameter] public string Variant { get; set; } = "outlined";
    [Parameter] public string? Size { get; set; }
    [Parameter] public bool Fluid { get; set; }
    [Parameter] public string? ScrollHeight { get; set; } = "200px";
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

    // events
    [Parameter] public EventCallback<IReadOnlyList<TValue>?> OnChange { get; set; }
    [Parameter] public EventCallback<string> OnFilter { get; set; }
    [Parameter] public EventCallback<FocusEventArgs> OnFocus { get; set; }
    [Parameter] public EventCallback<FocusEventArgs> OnBlur { get; set; }
    [Parameter] public EventCallback<MouseEventArgs> OnClick { get; set; }
    [Parameter] public EventCallback OnShow { get; set; }
    [Parameter] public EventCallback OnHide { get; set; }
    [Parameter] public EventCallback OnClear { get; set; }

    // templates
    [Parameter] public RenderFragment<TValue>? ItemTemplate { get; set; }
    [Parameter] public RenderFragment<IReadOnlyList<TValue>>? SelectedItemsTemplate { get; set; }
    [Parameter] public RenderFragment<object>? GroupTemplate { get; set; }
    [Parameter] public RenderFragment? HeaderTemplate { get; set; }
    [Parameter] public RenderFragment? FooterTemplate { get; set; }
    [Parameter] public RenderFragment? EmptyTemplate { get; set; }
    [Parameter] public RenderFragment? EmptyFilterTemplate { get; set; }
    [Parameter] public RenderFragment? DropdownIconTemplate { get; set; }
    [Parameter] public RenderFragment? LoadingIconTemplate { get; set; }
    [Parameter] public RenderFragment? ClearIconTemplate { get; set; }

    // ------------------------------------------------------------ lifecycle
    protected override void OnInitialized()
    {
        _id = InputId ?? $"op-ms-{Guid.NewGuid():N}";
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (_overlayVisible && _focusedOptionIndex >= 0)
        {
            await Interop.InvokeVoidAsync(
                OpInterop.OptimusInterop, "scrollSelectorIntoView", _listRef, ".p-multiselect-option.p-focus");
        }
    }

    // ------------------------------------------------------------ computed
    private string RootClass => OpCss.BuildClass(
        "p-multiselect p-component p-inputwrapper",
        Display == "chip" ? "p-multiselect-display-chip" : null,
        Disabled ? "p-disabled" : null,
        IsInvalid ? "p-invalid" : null,
        Variant == "filled" ? "p-variant-filled" : null,
        _focus ? "p-focus" : null,
        HasSelection ? "p-inputwrapper-filled" : null,
        _focus || _overlayVisible ? "p-inputwrapper-focus" : null,
        _overlayVisible ? "p-multiselect-open" : null,
        Fluid ? "p-multiselect-fluid" : null,
        Size == "small" ? "p-multiselect-sm p-inputfield-sm" : null,
        Size == "large" ? "p-multiselect-lg p-inputfield-lg" : null,
        StyleClass);

    private string LabelClass => OpCss.BuildClass(
        "p-multiselect-label",
        Placeholder is { Length: > 0 } && !HasSelection ? "p-placeholder" : null,
        string.IsNullOrEmpty(Placeholder) && !HasSelection ? "p-multiselect-label-empty" : null);

    private string PanelClass => OpCss.BuildClass(
        "p-multiselect-overlay p-component-overlay p-component",
        PanelStyleClass,
        _panelAnimationClass);

    private bool HasSelection => SelectedOptions.Count > 0;

    private List<TValue> SelectedOptions
    {
        get
        {
            var list = new List<TValue>();
            var values = Value ?? Array.Empty<TValue>();
            foreach (var v in values)
            {
                list.Add(v);
            }

            return list;
        }
    }

    private IEnumerable<object> AllOptions => Options ?? Array.Empty<object>();

    private string CommaLabel
    {
        get
        {
            var selected = SelectedOptions;
            if (selected.Count == 0) return string.Empty;
            if (MaxSelectedLabels is { } max && selected.Count > max)
            {
                return (SelectedItemsLabel ?? "{0} items selected").Replace("{0}", selected.Count.ToString());
            }

            return string.Join(", ", selected.Select(s => GetOptionLabel((object)s!)));
        }
    }

    private List<TValue> ChipItems
    {
        get
        {
            var selected = SelectedOptions;
            if (MaxSelectedLabels is { } max && selected.Count > max)
            {
                return new List<TValue>();
            }

            return selected;
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
        return label.Contains(_filterValue, StringComparison.CurrentCultureIgnoreCase);
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
    private string GetOptionLabel(object option) => OpSelectOption.GetLabel(option, OptionLabel);

    private object? GetOptionValue(object option) => OpSelectOption.GetValue(option, OptionValue);

    private bool IsOptionDisabled(object option)
    {
        if (option is null || string.IsNullOrEmpty(OptionDisabled)) return false;
        return OpSelectOption.GetProperty(option, OptionDisabled) is bool b && b;
    }

    private bool IsOptionGroup(object option)
    {
        if (option is null || !Group) return false;
        return OpSelectOption.GetProperty(option, OptionGroupChildren) is not null;
    }

    private string GetOptionGroupLabelValue(object group) =>
        group is null ? string.Empty : OpSelectOption.GetProperty(group, OptionGroupLabel)?.ToString() ?? string.Empty;

    private List<object> GetOptionGroupChildren(object group)
    {
        var list = new List<object>();
        if (group is null) return list;
        var children = OpSelectOption.GetProperty(group, OptionGroupChildren) as System.Collections.IEnumerable;
        if (children is null) return list;
        foreach (var child in children)
        {
            if (child is not null) list.Add(child);
        }

        return list;
    }

    private bool IsSelected(object option) => OpSelectOption.Contains(SelectedOptions, option, OptionValue);

    // Opções navegáveis por teclado: no modo agrupado são os filhos achatados (com filtro).
    private List<object> NavigableOptions
    {
        get
        {
            if (!Group) return VisibleOptions;
            var list = new List<object>();
            foreach (var group in AllOptions)
            {
                if (!IsOptionGroup(group)) continue;
                foreach (var child in GetOptionGroupChildren(group))
                {
                    if (MatchesFilter(child)) list.Add(child);
                }
            }

            return list;
        }
    }

    private string OptionClass(object option)
    {
        return OpCss.BuildClass(
            "p-multiselect-option",
            HighlightOnSelect && IsSelected(option) ? "p-multiselect-option-selected" : null,
            IsOptionDisabled(option) ? "p-disabled" : null);
    }

    // ------------------------------------------------------------ overlay
    private async Task OnRootClick(MouseEventArgs e)
    {
        await OnClick.InvokeAsync(e);
        if (Disabled || Readonly) return;
        if (!_overlayVisible)
        {
            await _inputRef.FocusAsync();
            if (!_overlayVisible)
            {
                await OpenAsync();
            }
        }
    }

    private async Task OnDropdownClick(MouseEventArgs e)
    {
        if (Disabled) return;
        if (!_overlayVisible)
        {
            await _inputRef.FocusAsync();
            if (!_overlayVisible)
            {
                await OpenAsync();
            }
        }
    }

    private async Task OnInputFocus(FocusEventArgs e)
    {
        if (Disabled) return;
        _focus = true;
        await OnFocus.InvokeAsync(e);
        if (ShowOnFocus && !_overlayVisible)
        {
            await OpenAsync();
        }
    }

    private async Task OnInputBlur(FocusEventArgs e)
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

        _focusedOptionIndex = FindFirstEnabledIndex();
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

    // Clique fora e Escape chegam pelo OpOverlayAttach (OnOutsideClick/OnEscape). O Tab fecha só a
    // partir do gatilho (OnInputKeydown): dentro do painel ele navega entre filtro, "selecionar
    // todos" e opções.

    // ------------------------------------------------------------ options interaction
    private async Task OnOptionClick(object option)
    {
        if (IsOptionDisabled(option)) return;
        if (OpSelectOption.GetValue(option, OptionValue) is null) return;
        await ToggleOptionAsync(OpSelectOption.ToValue<TValue>(option, OptionValue)!);
        await _inputRef.FocusAsync();
    }

    private async Task ToggleOptionAsync(TValue option)
    {
        var list = SelectedOptions;
        var existingIndex = list.FindIndex(v => Equals(v, option));
        if (existingIndex >= 0)
        {
            list.RemoveAt(existingIndex);
        }
        else
        {
            if (SelectionLimit is { } limit && list.Count >= limit)
            {
                return;
            }

            list.Add(option);
        }

        CurrentValue = list;
        await OnChange.InvokeAsync(list);
        StateHasChanged();
    }

    // Opções visíveis (com o filtro) que o toggle-all do cabeçalho marca ou desmarca.
    private List<TValue> ToggleableOptions => VisibleOptions
        .SelectMany(o => Group && IsOptionGroup(o) ? GetOptionGroupChildren(o).Where(MatchesFilter) : [o])
        .Where(o => !IsOptionDisabled(o))
        .Select(o => OpSelectOption.ToValue<TValue>(o, OptionValue))
        .Where(v => v is not null)
        .Select(v => v!)
        .ToList();

    private bool AllSelected
    {
        get
        {
            var options = ToggleableOptions;
            return options.Count > 0 && options.All(o => SelectedOptions.Any(v => Equals(v, o)));
        }
    }

    private async Task ToggleAllAsync()
    {
        var list = SelectedOptions;
        var options = ToggleableOptions;
        if (AllSelected)
            list.RemoveAll(v => options.Any(o => Equals(OpSelectOption.GetValue(o, OptionValue), v)));
        else
        {
            var toAdd = options.Where(o => !IsSelected(o!)).ToList();
            if (SelectionLimit is { } limit)
            {
                toAdd = toAdd.Take(Math.Max(0, limit - list.Count)).ToList();
            }

            list.AddRange(toAdd);
        }

        CurrentValue = list;
        await OnChange.InvokeAsync(list);
        StateHasChanged();
    }

    private async Task RemoveOptionAsync(TValue option)
    {
        var list = SelectedOptions;
        var existingIndex = list.FindIndex(v => Equals(v, option));
        if (existingIndex >= 0)
        {
            list.RemoveAt(existingIndex);
        }

        CurrentValue = list;
        await OnChange.InvokeAsync(list);
    }

    private void OnOptionMouseEnter(int index)
    {
        if (_focusedOptionIndex == index) return;
        _focusedOptionIndex = index;
        StateHasChanged();
    }

    private int FindFirstEnabledIndex()
    {
        var visible = NavigableOptions;
        for (var i = 0; i < visible.Count; i++)
        {
            if (!IsOptionDisabled(visible[i])) return i;
        }

        return -1;
    }

    private int FindLastEnabledIndex()
    {
        var visible = NavigableOptions;
        for (var i = visible.Count - 1; i >= 0; i--)
        {
            if (!IsOptionDisabled(visible[i])) return i;
        }

        return -1;
    }

    private int FindNextOptionIndex(int fromIndex, bool backward = false)
    {
        var visible = NavigableOptions;
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

    private async Task OnInputKeydown(KeyboardEventArgs e)
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
                    await ToggleFocusedOptionAsync();
                }
                else
                {
                    await OpenAsync();
                }

                break;
            case " ":
                // Space alterna a opção focada (ou abre o painel, como no PrimeNG).
                if (_overlayVisible)
                {
                    await ToggleFocusedOptionAsync();
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

    private void SearchOptions(string key)
    {
        var visible = NavigableOptions;
        if (visible.Count == 0) return;
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

    private async Task ToggleFocusedOptionAsync()
    {
        var visible = NavigableOptions;
        if (_focusedOptionIndex >= 0 && _focusedOptionIndex < visible.Count)
        {
            var option = visible[_focusedOptionIndex];
            if (!IsOptionDisabled(option))
            {
                await ToggleOptionAsync(OpSelectOption.ToValue<TValue>(option, OptionValue)!);
                await _inputRef.FocusAsync();
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
        CurrentValue = Array.Empty<TValue>();
        await OnClear.InvokeAsync();
    }

}