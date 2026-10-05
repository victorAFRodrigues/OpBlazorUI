using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using OpBlazorUI.Base.Components.Forms;

namespace OpBlazorUI.Base.Components.InputText;

public partial class OpInputText : OpInputBase<string>
{
    [Parameter] public string? Placeholder { get; set; }
    [Parameter] public string Variant { get; set; } = "outlined";
    [Parameter] public string? Size { get; set; }
    [Parameter] public bool Readonly { get; set; }
    [Parameter] public bool Fluid { get; set; }
    [Parameter] public string Type { get; set; } = "text";
    [Parameter] public string? Id { get; set; }
    [Parameter] public string? Name { get; set; }
    [Parameter] public string? AriaLabel { get; set; }
    [Parameter] public int? MaxLength { get; set; }
    [Parameter] public string? Autocomplete { get; set; }

    [Parameter] public string? Mask { get; set; }
    [Parameter] public bool AutoClear { get; set; } = true;
    [Parameter] public string SlotChar { get; set; } = "_";
    [Parameter] public string? KeyFilter { get; set; }

    [Parameter] public EventCallback<string?> OnInput { get; set; }
    [Parameter] public EventCallback<string?> OnChange { get; set; }
    [Parameter] public EventCallback<FocusEventArgs> OnFocus { get; set; }
    [Parameter] public EventCallback<FocusEventArgs> OnBlur { get; set; }
    [Parameter] public EventCallback<KeyboardEventArgs> OnKeyDown { get; set; }

    private string RootClass => OpCss.BuildClass(
        "p-inputtext p-component",
        !string.IsNullOrEmpty(CurrentValue) ? "p-filled" : null,
        Size == "small" ? "p-inputtext-sm" : null,
        Size == "large" ? "p-inputtext-lg" : null,
        IsInvalid ? "p-invalid" : null,
        Variant == "filled" ? "p-variant-filled" : null,
        Fluid ? "p-inputtext-fluid" : null,
        StyleClass);

    private async Task HandleInput(ChangeEventArgs e)
    {
        var text = e.Value?.ToString() ?? string.Empty;
        text = MaskFilter.ApplyKeyFilter(text, KeyFilter);
        text = MaskFilter.ApplyMask(text, Mask, SlotChar);
        CurrentValue = text;
        await OnInput.InvokeAsync(text);
    }

    private async Task HandleChange(ChangeEventArgs e)
    {
        await OnChange.InvokeAsync(e.Value?.ToString());
    }

    private async Task HandleBlur(FocusEventArgs e)
    {
        if (Mask is { Length: > 0 } && AutoClear && MaskFilter.IsIncomplete(CurrentValue, SlotChar))
        {
            CurrentValue = string.Empty;
        }

        await OnBlur.InvokeAsync(e);
    }
}
