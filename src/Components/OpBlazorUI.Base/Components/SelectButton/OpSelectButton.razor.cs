using System.Reflection;
using Microsoft.AspNetCore.Components;
using OpBlazorUI.Base.Components.Forms;

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

    private string GetOptionLabel(object option)
    {
        if (option is null) return string.Empty;
        if (string.IsNullOrEmpty(OptionLabel)) return option is string s ? s : option.ToString() ?? string.Empty;
        return GetPropertyValue(option, OptionLabel)?.ToString() ?? option.ToString() ?? string.Empty;
    }

    private object? GetOptionValue(object option)
    {
        if (option is null) return null;
        if (!string.IsNullOrEmpty(OptionValue)) return GetPropertyValue(option, OptionValue);
        if (!string.IsNullOrEmpty(OptionLabel)) return GetPropertyValue(option, OptionLabel);
        return option;
    }

    private bool IsOptionDisabled(object option)
    {
        if (option is null || string.IsNullOrEmpty(OptionDisabled)) return false;
        return GetPropertyValue(option, OptionDisabled) is bool b && b;
    }

    private static object? GetPropertyValue(object obj, string propertyPath)
    {
        var property = obj.GetType().GetProperty(propertyPath,
            BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
        return property?.GetValue(obj);
    }

    private bool IsSelected(object option)
    {
        if (Multiple)
        {
            var values = MultipleValue ?? Array.Empty<TValue>();
            foreach (var v in values)
            {
                if (Equals(GetOptionValue(option), GetOptionValue((object)v!))) return true;
            }

            return false;
        }

        return Value is not null && Equals(GetOptionValue(option), GetOptionValue((object)Value));
    }

    private RenderFragment<bool>? GetContentTemplate(object option, int index)
    {
        if (ItemTemplate is null || option is not TValue tv) return null;
        return _ => builder => ItemTemplate((tv, index))(builder);
    }

    private async Task OnOptionSelect(object option)
    {
        if (Disabled || IsOptionDisabled(option) || option is not TValue tv) return;

        if (Multiple)
        {
            var list = (MultipleValue ?? Array.Empty<TValue>()).ToList();
            var idx = list.FindIndex(v => Equals(GetOptionValue((object)v!), GetOptionValue(option)));
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
                list.Add(tv);
            }

            MultipleValue = list;
            await MultipleValueChanged.InvokeAsync(list);
            await OnChange.InvokeAsync(tv);
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
                CurrentValue = tv;
                await OnChange.InvokeAsync(tv);
            }
        }
    }
}