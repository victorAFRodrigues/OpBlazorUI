using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;

namespace OpBlazorUI.Base.Components.Overlay;

public partial class OpOverlay : ComponentBase
{
    [Parameter] public bool Visible { get; set; }

    [Parameter] public EventCallback<bool> VisibleChanged { get; set; }

    [Parameter] public string Mode { get; set; } = "overlay";

    [Parameter] public string? Style { get; set; }

    [Parameter] public string? StyleClass { get; set; }

    [Parameter] public string? ContentStyle { get; set; }

    [Parameter] public string? ContentStyleClass { get; set; }

    [Parameter] public string? Target { get; set; }

    [Parameter] public string? AppendTo { get; set; } = "self";

    [Parameter] public bool Dismissable { get; set; }

    [Parameter] public bool HideOnEscape { get; set; } = true;

    [Parameter] public EventCallback OnShow { get; set; }

    [Parameter] public EventCallback OnHide { get; set; }

    [Parameter] public EventCallback OnBeforeShow { get; set; }

    [Parameter] public EventCallback OnBeforeHide { get; set; }

    [Parameter] public RenderFragment? ChildContent { get; set; }

    [Parameter(CaptureUnmatchedValues = true)]
    public Dictionary<string, object>? AdditionalAttributes { get; set; }

    private bool IsModal => Mode == "modal";

    private string RootClass => OpCss.BuildClass(
        IsModal ? "p-overlay-modal" : "p-overlay",
        StyleClass);

    private string ContentClass => OpCss.BuildClass("p-overlay-content", ContentStyleClass);

    private string MaskClass => OpCss.BuildClass(
        "p-overlay-mask",
        "p-overlay-mask-enter-active");

    private string? MaskStyle => null;

    private async Task OnMaskClick(MouseEventArgs e)
    {
        if (Dismissable)
        {
            await HideAsync();
        }
    }

    private async Task OnKeydown(KeyboardEventArgs e)
    {
        if (HideOnEscape && e.Key == "Escape")
        {
            await HideAsync();
        }
    }

    public async Task ShowAsync()
    {
        await OnBeforeShow.InvokeAsync();
        Visible = true;
        await VisibleChanged.InvokeAsync(true);
        await OnShow.InvokeAsync();
    }

    public async Task HideAsync()
    {
        await OnBeforeHide.InvokeAsync();
        Visible = false;
        await VisibleChanged.InvokeAsync(false);
        await OnHide.InvokeAsync();
    }

    public async Task ToggleAsync()
    {
        if (Visible)
        {
            await HideAsync();
        }
        else
        {
            await ShowAsync();
        }
    }
}
