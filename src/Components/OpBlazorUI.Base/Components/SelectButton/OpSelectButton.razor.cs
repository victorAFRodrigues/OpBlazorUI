using Microsoft.AspNetCore.Components;
using OpBlazorUI.Base.Components.Forms;
using OpBlazorUI.Base.Components.Select;

namespace OpBlazorUI.Base.Components.SelectButton;

public partial class OpSelectButton<TValue> : OpInputBase<TValue>
{
    [Parameter] public IReadOnlyList<object>? Options { get; set; }
    [Parameter] public string? OptionLabel { get; set; }
    [Parameter] public string? OptionValue { get; set; }
    [Parameter] public string? OptionDisabled { get; set; }
    [Parameter] public bool Multiple { get; set; }
    [Parameter] public IReadOnlyList<TValue>? MultipleValue { get; set; }
    [Parameter] public EventCallback<IReadOnlyList<TValue>?> MultipleValueChanged { get; set; }
    [Parameter] public bool AllowEmpty { get; set; } = true;
    [Parameter] public bool Fluid { get; set; }
    [Parameter] public string? Size { get; set; }

    [Parameter] public RenderFragment<(TValue Option, int Index)>? ItemTemplate { get; set; }

    [Parameter] public EventCallback<TValue?> OnChange { get; set; }

    private IEnumerable<object> AllOptions => Options ?? Array.Empty<object>();

    private string RootClass => OpCss.BuildClass(
        "p-selectbutton p-component",
        IsInvalid ? "p-invalid" : null,
        Fluid ? "p-selectbutton-fluid" : null,
        StyleClass);

    private string GetOptionLabel(object option) => OpSelectOption.GetLabel(option, OptionLabel);

    private object? GetOptionValue(object option)
    {
        if (option is null) return null;
        if (!string.IsNullOrEmpty(OptionValue)) return OpSelectOption.GetValue(option, OptionValue);
        if (!string.IsNullOrEmpty(OptionLabel)) return OpSelectOption.GetValue(option, OptionLabel);
        return option;
    }

    private bool IsOptionDisabled(object option)
    {
        if (option is null || string.IsNullOrEmpty(OptionDisabled)) return false;
        return OpSelectOption.GetProperty(option, OptionDisabled) is bool b && b;
    }

    private bool IsSelected(object option)
    {
        if (Multiple)
        {
            var values = MultipleValue ?? Array.Empty<TValue>();
            foreach (var v in values)
            {
                if (Equals(GetOptionValue(option), v)) return true;
            }

            return false;
        }

        return Value is not null && Equals(GetOptionValue(option), Value);
    }

    private RenderFragment<bool>? GetContentTemplate(object option, int index)
    {
        if (ItemTemplate is null) return null;
        var value = OpSelectOption.ToValue<TValue>(option, !string.IsNullOrEmpty(OptionValue) ? OptionValue : OptionLabel);
        return _ => builder => ItemTemplate((value!, index))(builder);
    }

    private async Task OnOptionSelect(object option)
    {
        if (Disabled || IsOptionDisabled(option)) return;

        var value = OpSelectOption.ToValue<TValue>(option, !string.IsNullOrEmpty(OptionValue) ? OptionValue : OptionLabel);

        if (Multiple)
        {
            var list = (MultipleValue ?? Array.Empty<TValue>()).ToList();
            var idx = list.FindIndex(v => Equals(v, value));
            if (idx >= 0)
            {
                if (AllowEmpty || list.Count > 1)
                {
                    list.RemoveAt(idx);
                }
                else
                {
                    return;
                }
            }
            else
            {
                list.Add(value!);
            }

            MultipleValue = list;
            await MultipleValueChanged.InvokeAsync(list);
            await OnChange.InvokeAsync(value);
        }
        else
        {
            if (IsSelected(option))
            {
                if (!AllowEmpty) return;
                CurrentValue = default;
                await OnChange.InvokeAsync(default);
            }
            else
            {
                CurrentValue = value;
                await OnChange.InvokeAsync(value);
            }
        }
    }
}