using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using OpBlazorUI.Base.Components.Forms;

namespace OpBlazorUI.Base.Components.InputSwitch;

public partial class OpInputSwitch : OpInputBase<bool>
{
    [Parameter] public string? Name { get; set; }
    [Parameter] public string? Id { get; set; }
    [Parameter] public string? AriaLabel { get; set; }
    [Parameter] public string? AriaLabelledBy { get; set; }
    [Parameter] public bool Required { get; set; }
    [Parameter] public int TabIndex { get; set; }

    [Parameter] public EventCallback<bool> OnChange { get; set; }
    [Parameter] public EventCallback<FocusEventArgs> OnFocus { get; set; }
    [Parameter] public EventCallback<FocusEventArgs> OnBlur { get; set; }

    [Parameter] public RenderFragment<bool>? HandleTemplate { get; set; }

    private string RootClass => OpCss.BuildClass(
        "p-toggleswitch p-component",
        CurrentValue ? "p-toggleswitch-checked" : null,
        Disabled ? "p-disabled" : null,
        IsInvalid ? "p-invalid" : null,
        StyleClass);

    private async Task HandleChange(ChangeEventArgs e)
    {
        if (Disabled) return;

        var next = (bool)(e.Value ?? false);
        CurrentValue = next;
        await OnChange.InvokeAsync(next);
    }
}
