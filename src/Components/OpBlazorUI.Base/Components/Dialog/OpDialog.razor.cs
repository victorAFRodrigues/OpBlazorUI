using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;

namespace OpBlazorUI.Base.Components.Dialog;

public partial class OpDialog : ComponentBase
{
    private string _headerId = "";
    private bool _maximized;
    private bool _lastRenderedVisible;
    private ElementReference _root;

    [Parameter] public bool Visible { get; set; }

    [Parameter] public EventCallback<bool> VisibleChanged { get; set; }

    [Parameter] public string? Header { get; set; }

    [Parameter] public bool Modal { get; set; }

    [Parameter] public bool Closable { get; set; } = true;

    [Parameter] public bool CloseOnEscape { get; set; } = true;

    [Parameter] public bool DismissableMask { get; set; }

    [Parameter] public bool ShowHeader { get; set; } = true;

    [Parameter] public string Position { get; set; } = "center";

    [Parameter] public bool Maximizable { get; set; }

    [Parameter] public bool FocusOnShow { get; set; } = true;

    [Parameter] public string? Style { get; set; }

    [Parameter] public string? StyleClass { get; set; }

    [Parameter] public string? ContentStyle { get; set; }

    [Parameter] public string? ContentStyleClass { get; set; }

    [Parameter] public string? MaskStyle { get; set; }

    [Parameter] public string? MaskStyleClass { get; set; }

    [Parameter] public string CloseIcon { get; set; } = "pi pi-times";

    [Parameter] public string CloseAriaLabel { get; set; } = "Close";

    [Parameter] public EventCallback OnShow { get; set; }

    [Parameter] public EventCallback OnHide { get; set; }

    [Parameter] public RenderFragment? ChildContent { get; set; }

    [Parameter] public RenderFragment? HeaderTemplate { get; set; }

    [Parameter] public RenderFragment? FooterTemplate { get; set; }

    [Parameter] public RenderFragment? CloseIconTemplate { get; set; }

    [Parameter(CaptureUnmatchedValues = true)]
    public Dictionary<string, object>? AdditionalAttributes { get; set; }

    protected override void OnInitialized()
    {
        _headerId = $"op-dialog-title-{Guid.NewGuid():N}";
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
        "p-dialog p-component",
        "p-dialog-enter-active",
        _maximized ? "p-dialog-maximized" : null,
        StyleClass);

    private string MaskClass => OpCss.BuildClass(
        "p-overlay-mask",
        "p-dialog-mask",
        Position != "center" ? $"p-dialog-{Position}" : null,
        "p-overlay-mask-enter-active",
        MaskStyleClass);

    private string? MaskStyleValue => OpCss.BuildClass(
        _maximized ? "padding:0;" : null,
        MaskStyle);

    private async Task Close(MouseEventArgs _)
    {
        await CloseAsync();
    }

    private async Task CloseAsync()
    {
        _maximized = false;
        Visible = false;
        await VisibleChanged.InvokeAsync(false);
        await OnHide.InvokeAsync();
    }

    private async Task OnMaskClick(MouseEventArgs e)
    {
        if (DismissableMask)
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

    private void ToggleMaximize(MouseEventArgs _)
    {
        _maximized = !_maximized;
    }
}
