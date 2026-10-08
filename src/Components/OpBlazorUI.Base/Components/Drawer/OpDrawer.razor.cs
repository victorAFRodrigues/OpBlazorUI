using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using OpBlazorUI.Base.Components.Common;

namespace OpBlazorUI.Base.Components.Drawer;

public partial class OpDrawer : OpModalBase
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

    [Parameter] public string? MaskStyle { get; set; }

    /// <summary>Aplica o z-index da camada modal (1100 + <see cref="BaseZIndex"/>), acima de topbars e overlays.</summary>
    [Parameter] public bool AutoZIndex { get; set; } = true;

    [Parameter] public int BaseZIndex { get; set; }

    [Parameter] public string? MaskStyleClass { get; set; }

    [Parameter] public string CloseIcon { get; set; } = "pi pi-times";

    [Parameter] public string AriaCloseLabel { get; set; } = "Close";

    [Parameter] public EventCallback OnShow { get; set; }

    [Parameter] public EventCallback OnHide { get; set; }

    [Parameter] public RenderFragment? ChildContent { get; set; }

    [Parameter] public RenderFragment? HeaderTemplate { get; set; }

    [Parameter] public RenderFragment? FooterTemplate { get; set; }

    [Parameter] public RenderFragment? CloseIconTemplate { get; set; }

    protected override void OnInitialized()
    {
        _headerId = $"op-drawer-title-{Guid.NewGuid():N}";
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (Visible && !_lastRenderedVisible)
        {
            await OnShow.InvokeAsync();

            // Trap antes do foco: a base JS captura o gatilho (foco atual) para restaurar depois.
            if (Modal)
            {
                try
                {
                    await FocusTrapInitAsync(_root);
                }
                catch
                {
                    // ignore
                }
            }

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
        else if (!Visible && _lastRenderedVisible && Modal)
        {
            try
            {
                await FocusTrapDisposeAsync(_root);
            }
            catch
            {
                // ignore
            }
        }

        _lastRenderedVisible = Visible;
    }

    private string RootClass => OpCss.BuildClass(
        "p-drawer p-component",
        FullScreen ? "p-drawer-full" : null,
        "p-drawer-open",
        $"p-drawer-{Position}",
        FullScreen ? "p-drawer-enter-full" : $"p-drawer-enter-{Position}",
        StyleClass);

    private string MaskClass => OpCss.BuildClass(
        "p-drawer-mask p-overlay-mask p-overlay-mask-enter-active",
        FullScreen ? "p-drawer-full" : null,
        MaskStyleClass);

    private async Task Close(MouseEventArgs _)
    {
        await CloseAsync();
    }

    private async Task CloseAsync()
    {
        // Restaura o foco antes de o drawer sair do DOM (elemento ainda anexado para o JS).
        if (Modal && Visible)
        {
            try
            {
                await FocusTrapDisposeAsync(_root);
            }
            catch
            {
                // ignore
            }
        }

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

    // Escape chega pelo OpOverlayAttach só quando este é o overlay do topo (um Select aberto
    // dentro do diálogo fecha antes), mesmo com o foco fora do diálogo.
    private EventCallback EscapeCallback =>
        CloseOnEscape ? EventCallback.Factory.Create(this, CloseAsync) : default;
}
