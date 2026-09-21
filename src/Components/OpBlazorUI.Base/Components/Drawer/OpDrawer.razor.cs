using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;

namespace OpBlazorUI.Base.Components.Drawer;

public partial class OpDrawer : ComponentBase
{
    private string _headerId = "";
    private bool _lastRenderedVisible;
    private ElementReference _root;

    [Parameter] public bool Visible { get; set; }

    [Parameter] public EventCallback<bool> VisibleChanged { get; set; }

    [Parameter] public string Position { get; set; } = "left";

    [Parameter] public bool FullScreen { get; set; }

    [Parameter] public string? Header { get; set; }

    [Parameter] public bool Modal { get; set; } = true;

    [Parameter] public bool Closable { get; set; } = true;

    [Parameter] public bool ShowHeader { get; set; } = true;

    [Parameter] public bool Dismissible { get; set; } = true;

    [Parameter] public bool CloseOnEscape { get; set; } = true;

    [Parameter] public bool FocusOnShow { get; set; } = true;

    [Parameter] public string? Style { get; set; }

    [Parameter] public string? StyleClass { get; set; }

    [Parameter] public string? MaskStyle { get; set; }

    [Parameter] public string? MaskStyleClass { get; set; }

    [Parameter] public string CloseIcon { get; set; } = "pi pi-times";

    [Parameter] public string AriaCloseLabel { get; set; } = "Close";

    [Parameter] public EventCallback OnShow { get; set; }

    [Parameter] public EventCallback OnHide { get; set; }

    [Parameter] public RenderFragment? ChildContent { get; set; }

    [Parameter] public RenderFragment? HeaderTemplate { get; set; }

    [Parameter] public RenderFragment? FooterTemplate { get; set; }

    [Parameter] public RenderFragment? CloseIconTemplate { get; set; }

    [Parameter(CaptureUnmatchedValues = true)]
    public Dictionary<string, object>? AdditionalAttributes { get; set; }

    private string EffectivePosition => FullScreen ? "full" : Position;

    protected override void OnInitialized()
    {
        _headerId = $"op-drawer-title-{Guid.NewGuid():N}";
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (Visible && !_lastRenderedVisible)
        {
            await OnShow.InvokeAsync();
            if (FocusOnShow)
            {
                try
                {
                    await _root.FocusAsync();
                }
                catch
                {
                    // ignore
                }
            }
        }

        _lastRenderedVisible = Visible;
    }

    private string RootClass => OpCss.BuildClass(
        "p-drawer p-component",
        "p-drawer-enter-active",
        StyleClass);

    private string MaskClass => OpCss.BuildClass(
        "p-overlay-mask",
        "p-drawer-mask",
        $"p-drawer-{EffectivePosition}",
        "p-drawer-open",
        "p-overlay-mask-enter-active",
        MaskStyleClass);

    private async Task Close(MouseEventArgs _)
    {
        await CloseAsync();
    }

    private async Task CloseAsync()
    {
        Visible = false;
        await VisibleChanged.InvokeAsync(false);
        await OnHide.InvokeAsync();
    }

    private async Task OnMaskClick(MouseEventArgs e)
    {
        if (Dismissible)
        {
            await CloseAsync();
        }
    }

    private async Task OnKeydown(KeyboardEventArgs e)
    {
        if (CloseOnEscape && e.Key == "Escape")
        {
            await CloseAsync();
        }
    }
}
