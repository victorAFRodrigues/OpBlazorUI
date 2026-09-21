using System.Reflection;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;

namespace OpBlazorUI.Base.Components.Listbox;

public partial class OpListbox<TValue> : ComponentBase
{
    private string _id = "";
    private string _filterValue = "";
    private int _focusedOptionIndex = -1;

    // ---------------------------------------------------------------- params
    [Parameter] public IReadOnlyList<object>? Options { get; set; }
    [Parameter] public string? OptionLabel { get; set; }
    [Parameter] public string? OptionValue { get; set; }
    [Parameter] public string? OptionDisabled { get; set; }

    [Parameter] public TValue? Value { get; set; }
    [Parameter] public EventCallback<TValue?> ValueChanged { get; set; }

    [Parameter] public IReadOnlyList<TValue>? SelectedValues { get; set; }
    [Parameter] public EventCallback<IReadOnlyList<TValue>?> SelectedValuesChanged { get; set; }

    [Parameter] public bool Multiple { get; set; }
    [Parameter] public bool Checkbox { get; set; }
    [Parameter] public bool Checkmark { get; set; }
    [Parameter] public bool HighlightOnSelect { get; set; } = true;
    [Parameter] public bool Filter { get; set; }
    [Parameter] public string? FilterBy { get; set; }
    [Parameter] public bool Group { get; set; }
    [Parameter] public string OptionGroupLabel { get; set; } = "label";
    [Parameter] public string OptionGroupChildren { get; set; } = "items";
    [Parameter] public bool ShowToggleAll { get; set; } = true;
    [Parameter] public bool Striped { get; set; }
    [Parameter] public string? ScrollHeight { get; set; }
    [Parameter] public bool VirtualScroll { get; set; }
    [Parameter] public int VirtualScrollItemSize { get; set; } = 38;
    [Parameter] public bool Disabled { get; set; }
    [Parameter] public bool Readonly { get; set; }
    [Parameter] public bool Invalid { get; set; }
    [Parameter] public bool Required { get; set; }
    [Parameter] public int TabIndex { get; set; }
    [Parameter] public string? AriaLabel { get; set; }
    [Parameter] public string? EmptyMessage { get; set; } = "No results found";
    [Parameter] public string? EmptyFilterMessage { get; set; } = "No results found";
    [Parameter] public string? StyleClass { get; set; }
    [Parameter] public string? ListStyleClass { get; set; }
    [Parameter] public string? InputId { get; set; }

    // events
    [Parameter] public EventCallback<TValue?> OnChange { get; set; }
    [Parameter] public EventCallback<IReadOnlyList<TValue>?> OnSelectionChange { get; set; }
    [Parameter] public EventCallback<string> OnFilter { get; set; }
    [Parameter] public EventCallback<FocusEventArgs> OnFocus { get; set; }
    [Parameter] public EventCallback<FocusEventArgs> OnBlur { get; set; }
    [Parameter] public EventCallback<MouseEventArgs> OnClick { get; set; }
    [Parameter] public EventCallback<MouseEventArgs> OnDblClick { get; set; }
    [Parameter] public EventCallback<bool> OnSelectAllChange { get; set; }

    // templates
    [Parameter] public RenderFragment<TValue>? ItemTemplate { get; set; }
    [Parameter] public RenderFragment<object>? GroupTemplate { get; set; }
    [Parameter] public RenderFragment? HeaderTemplate { get; set; }
    [Parameter] public RenderFragment? FooterTemplate { get; set; }
    [Parameter] public RenderFragment? EmptyTemplate { get; set; }
    [Parameter] public RenderFragment? EmptyFilterTemplate { get; set; }
    [Parameter] public RenderFragment? FilterTemplate { get; set; }
    [Parameter] public RenderFragment<bool>? CheckIconTemplate { get; set; }

    [Parameter(CaptureUnmatchedValues = true)]
    public Dictionary<string, object>? AdditionalAttributes { get; set; }

    // ------------------------------------------------------------ lifecycle
    protected override void OnInitialized()
    {
        _id = InputId ?? $"op-listbox-{Guid.NewGuid():N}";
    }

    // ------------------------------------------------------------ computed
    private string RootClass => BuildClass(
        "p-listbox p-component",
        Disabled ? "p-disabled" : null,
        Invalid ? "p-invalid" : null,
        Striped ? "p-listbox-striped" : null,
        StyleClass);

    private string ListClass => BuildClass("p-listbox-list", ListStyleClass);

    private string ListContainerStyle => $"max-height:{(ScrollHeight ?? "auto")};";

    private bool IsMultiple => Multiple || Checkbox;

    private bool HasSelection => IsMultiple ? SelectedOptions.Count > 0 : SelectedSingleOption is not null;

    private List<TValue> SelectedOptions
    {
        get
        {
            var list = new List<TValue>();
            var values = SelectedValues ?? Array.Empty<TValue>();
            foreach (var v in values)
            {
                list.Add(v);
            }

            return list;
        }
    }

    private object? SelectedSingleOption
    {
        get
        {
            if (Value is null) return null;
            foreach (var opt in FlattenedAllOptions)
            {
                if (Equals(GetOptionValue(opt), GetOptionValue((object)Value))) return opt;
            }

            return null;
        }
    }

    private IEnumerable<object> AllOptions => Options ?? Array.Empty<object>();

    private List<object> FlattenedAllOptions
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
                        list.Add(child);
                    }
                }
                else
                {
                    list.Add(opt);
                }
            }

            return list;
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
                        if (MatchesFilter(child)) list.Add(child);
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

    private bool ShowEmptyMessage => VisibleOptions.Count == 0;

    private bool AllSelected
    {
        get
        {
            var total = FlattenedAllOptions.Count;
            if (total == 0) return false;
            return SelectedOptions.Count >= total;
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
        if (IsMultiple)
        {
            foreach (var v in SelectedOptions)
            {
                if (Equals(GetOptionValue(option), GetOptionValue((object)v!))) return true;
            }

            return false;
        }

        return SelectedSingleOption is not null &&
               Equals(GetOptionValue(option), GetOptionValue(SelectedSingleOption));
    }

    private string OptionClass(object option)
    {
        var isDisabled = IsOptionDisabled(option);
        var isSel = IsSelected(option);
        return BuildClass(
            "p-listbox-option",
            isSel && HighlightOnSelect ? "p-listbox-option-selected" : null,
            isDisabled ? "p-disabled" : null);
    }

    // ------------------------------------------------------------ interaction
    private async Task OnOptionClick(object option)
    {
        if (IsOptionDisabled(option) || Disabled || Readonly) return;
        await ToggleOptionAsync(option);
    }

    private async Task OnOptionDoubleClick(object option)
    {
        if (IsOptionDisabled(option) || Disabled || Readonly) return;
        await OnDblClick.InvokeAsync(new MouseEventArgs());
    }

    private async Task ToggleOptionAsync(object option)
    {
        if (IsMultiple)
        {
            var list = SelectedOptions;
            var existing = list.FirstOrDefault(v => Equals(GetOptionValue((object)v!), GetOptionValue(option)));
            if (existing is not null)
            {
                list.Remove(existing);
            }
            else
            {
                if (option is TValue tv) list.Add(tv);
            }

            SelectedValues = list;
            await SelectedValuesChanged.InvokeAsync(list);
            await OnSelectionChange.InvokeAsync(list);
        }
        else
        {
            if (option is TValue tv)
            {
                Value = tv;
                await ValueChanged.InvokeAsync(tv);
                await OnChange.InvokeAsync(tv);
            }
        }

        StateHasChanged();
    }

    private async Task OnToggleAll(bool selected)
    {
        IReadOnlyList<TValue> list;
        if (selected)
        {
            list = FlattenedAllOptions.OfType<TValue>().ToList();
        }
        else
        {
            list = Array.Empty<TValue>();
        }

        SelectedValues = list;
        await SelectedValuesChanged.InvokeAsync(list);
        await OnSelectionChange.InvokeAsync(list);
        await OnSelectAllChange.InvokeAsync(selected);
        StateHasChanged();
    }

    private async Task OnFilterInput(ChangeEventArgs e)
    {
        _filterValue = e.Value?.ToString() ?? string.Empty;
        _focusedOptionIndex = -1;
        await OnFilter.InvokeAsync(_filterValue);
    }

    private void OnOptionMouseEnter(int index)
    {
        _focusedOptionIndex = index;
        StateHasChanged();
    }

    private async Task OnListKeydown(KeyboardEventArgs e)
    {
        if (Disabled || Readonly) return;

        switch (e.Key)
        {
            case "ArrowDown":
                _focusedOptionIndex = FindNextOptionIndex(_focusedOptionIndex);
                StateHasChanged();
                break;
            case "ArrowUp":
                _focusedOptionIndex = FindNextOptionIndex(_focusedOptionIndex, backward: true);
                StateHasChanged();
                break;
            case "Enter":
            case " ":
                await SelectFocusedOptionAsync();
                break;
            case "Home":
                _focusedOptionIndex = FindFirstEnabledIndex();
                StateHasChanged();
                break;
            case "End":
                _focusedOptionIndex = FindLastEnabledIndex();
                StateHasChanged();
                break;
            default:
                if (!string.IsNullOrEmpty(e.Key) && e.Key.Length == 1 && char.IsLetterOrDigit(e.Key[0]))
                {
                    SearchOptions(e.Key);
                }

                break;
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
                await ToggleOptionAsync(option);
            }
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

    private static string BuildClass(params string?[] classes)
        => string.Join(' ', classes.Where(c => !string.IsNullOrWhiteSpace(c)));
}
