using Microsoft.AspNetCore.Components;
using OpBlazorUI.Base.Components.Forms;

namespace OpBlazorUI.Base.Components.ToggleButton;

public partial class OpToggleButton : OpInputBase<bool>
{
    [Parameter] public string? OnLabel { get; set; }
    [Parameter] public string? OffLabel { get; set; }
    [Parameter] public string? OnIcon { get; set; }
    [Parameter] public string? OffIcon { get; set; }
    [Parameter] public string IconPos { get; set; } = "left";
    [Parameter] public string? Size { get; set; }
    [Parameter] public bool Fluid { get; set; }
    [Parameter] public string? AriaLabel { get; set; }

    [Parameter] public EventCallback<bool> OnChange { get; set; }

    [Parameter] public RenderFragment<bool>? ContentTemplate { get; set; }

    private bool HasOnLabel => !string.IsNullOrEmpty(OnLabel);
    private bool HasOffLabel => !string.IsNullOrEmpty(OffLabel);
    private bool HasIcon => !string.IsNullOrEmpty(OnIcon) || !string.IsNullOrEmpty(OffIcon);

    private string RootClass => OpCss.BuildClass(
        "p-togglebutton p-component",
        CurrentValue ? "p-togglebutton-checked" : null,
        IsInvalid ? "p-invalid" : null,
        Disabled ? "p-disabled" : null,
        Size == "small" ? "p-togglebutton-sm p-inputfield-sm" : null,
        Size == "large" ? "p-togglebutton-lg p-inputfield-lg" : null,
        Fluid ? "p-togglebutton-fluid" : null,
        StyleClass);

    private string IconClass => OpCss.BuildClass(
        "p-togglebutton-icon",
        IconPos == "left" ? "p-togglebutton-icon-left" : null,
        IconPos == "right" ? "p-togglebutton-icon-right" : null,
        CurrentValue ? OnIcon : OffIcon);

    private async Task Toggle()
    {
        if (Disabled) return;
        CurrentValue = !CurrentValue;
        await OnChange.InvokeAsync(CurrentValue);
    }
}
