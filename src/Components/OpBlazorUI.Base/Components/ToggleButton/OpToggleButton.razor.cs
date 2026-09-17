using Microsoft.AspNetCore.Components;

namespace OpBlazorUI.Base.Components.ToggleButton;

public partial class OpToggleButton : ComponentBase
{
    [Parameter] public bool Checked { get; set; }
    [Parameter] public EventCallback<bool> CheckedChanged { get; set; }
    [Parameter] public string? OnLabel { get; set; }
    [Parameter] public string? OffLabel { get; set; }
    [Parameter] public string? OnIcon { get; set; }
    [Parameter] public string? OffIcon { get; set; }
    [Parameter] public string IconPos { get; set; } = "left";
    [Parameter] public bool Disabled { get; set; }
    [Parameter] public bool Invalid { get; set; }
    [Parameter] public string? Size { get; set; }
    [Parameter] public bool Fluid { get; set; }
    [Parameter] public string? AriaLabel { get; set; }
    [Parameter] public string? StyleClass { get; set; }

    [Parameter] public EventCallback<bool> OnChange { get; set; }

    [Parameter] public RenderFragment<bool>? ContentTemplate { get; set; }

    private bool HasOnLabel => !string.IsNullOrEmpty(OnLabel);
    private bool HasOffLabel => !string.IsNullOrEmpty(OffLabel);
    private bool HasIcon => !string.IsNullOrEmpty(OnIcon) || !string.IsNullOrEmpty(OffIcon);

    private string RootClass => BuildClass(
        "p-togglebutton p-component",
        Checked ? "p-togglebutton-checked" : null,
        Invalid ? "p-invalid" : null,
        Disabled ? "p-disabled" : null,
        Size == "small" ? "p-togglebutton-sm p-inputfield-sm" : null,
        Size == "large" ? "p-togglebutton-lg p-inputfield-lg" : null,
        Fluid ? "p-togglebutton-fluid" : null,
        StyleClass);

    private string IconClass => BuildClass(
        "p-togglebutton-icon",
        IconPos == "left" ? "p-togglebutton-icon-left" : null,
        IconPos == "right" ? "p-togglebutton-icon-right" : null,
        Checked ? OnIcon : OffIcon);

    private async Task Toggle()
    {
        if (Disabled) return;
        Checked = !Checked;
        await CheckedChanged.InvokeAsync(Checked);
        await OnChange.InvokeAsync(Checked);
    }

    private static string BuildClass(params string?[] classes)
        => string.Join(' ', classes.Where(c => !string.IsNullOrWhiteSpace(c)));
}