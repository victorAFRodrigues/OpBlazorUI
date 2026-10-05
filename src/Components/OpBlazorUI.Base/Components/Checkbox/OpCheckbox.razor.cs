using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using OpBlazorUI.Base.Components.Forms;

namespace OpBlazorUI.Base.Components.Checkbox;

public partial class OpCheckbox : OpInputBase<bool?>
{
    [Parameter] public bool Indeterminate { get; set; }
    [Parameter] public string? Name { get; set; }
    [Parameter] public string? Id { get; set; }
    [Parameter] public string? AriaLabel { get; set; }
    [Parameter] public string? AriaLabelledBy { get; set; }
    [Parameter] public bool Required { get; set; }
    [Parameter] public bool Readonly { get; set; }
    [Parameter] public string? Size { get; set; }
    [Parameter] public string Variant { get; set; } = "outlined";
    [Parameter] public int TabIndex { get; set; }
    [Parameter] public string? CheckboxIcon { get; set; }
    [Parameter] public string? InputClass { get; set; }

    [Parameter] public EventCallback<bool> OnChange { get; set; }
    [Parameter] public EventCallback<FocusEventArgs> OnFocus { get; set; }
    [Parameter] public EventCallback<FocusEventArgs> OnBlur { get; set; }

    [Parameter] public RenderFragment<bool>? CheckboxIconTemplate { get; set; }

    private string RootClass => OpCss.BuildClass(
        "p-checkbox p-component",
        CurrentValue == true && !Indeterminate ? "p-checkbox-checked p-highlight" : null,
        Disabled ? "p-disabled" : null,
        IsInvalid ? "p-invalid" : null,
        Variant == "filled" ? "p-variant-filled" : null,
        Size == "small" ? "p-checkbox-sm p-inputfield-sm" : null,
        Size == "large" ? "p-checkbox-lg p-inputfield-lg" : null,
        StyleClass);

    private string InputCss => OpCss.BuildClass("p-checkbox-input", InputClass);

    private async Task HandleChange(ChangeEventArgs e)
    {
        if (Readonly || Disabled) return;

        var next = (bool)(e.Value ?? false);
        CurrentValue = next;
        await OnChange.InvokeAsync(next);
    }
}
