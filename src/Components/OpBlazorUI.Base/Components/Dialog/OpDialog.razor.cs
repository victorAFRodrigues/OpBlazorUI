using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using OpBlazorUI.Base.Components.Common;

namespace OpBlazorUI.Base.Components.Dialog;

public partial class OpDialog : OpModalBase
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

    [Parameter] public string? ContentStyle { get; set; }

    [Parameter] public string? ContentStyleClass { get; set; }

    [Parameter] public string? MaskStyle { get; set; }

    /// <summary>Aplica o z-index da camada modal (1100 + <see cref="BaseZIndex"/>), acima de topbars e overlays.</summary>
    [Parameter] public bool AutoZIndex { get; set; } = true;

    [Parameter] public int BaseZIndex { get; set; }

    [Parameter] public string? MaskStyleClass { get; set; }

    [Parameter] public string CloseIcon { get; set; } = "pi pi-times";

    [Parameter] public string CloseAriaLabel { get; set; } = "Close";

    [Parameter] public EventCallback OnShow { get; set; }

    [Parameter] public EventCallback OnHide { get; set; }

    [Parameter] public RenderFragment? ChildContent { get; set; }

    [Parameter] public RenderFragment? HeaderTemplate { get; set; }

    [Parameter] public RenderFragment? FooterTemplate { get; set; }

    [Parameter] public RenderFragment? CloseIconTemplate { get; set; }

    protected override void OnInitialized()
    {
        _headerId = $"op-dialog-title-{Guid.NewGuid():N}";
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
        "p-dialog p-component",
        "p-dialog-enter-active",
        _maximized ? "p-dialog-maximized" : null,
        StyleClass);

    // inlineStyles do upstream: sem modal, a máscara deixa os cliques passarem para a página.
    private string MaskInlineStyle => $"pointer-events: {(Modal ? "auto" : "none")}; {MaskStyle}".TrimEnd();

    private string RootInlineStyle => $"pointer-events: auto; {Style}".TrimEnd();

    private string MaskClass => OpCss.BuildClass(
        "p-dialog-mask",
        Modal ? "p-overlay-mask p-overlay-mask-enter-active" : null,
        Position != "center" ? $"p-dialog-{Position}" : null,
        MaskStyleClass);

    private async Task Close(MouseEventArgs _)
    {
        await CloseAsync();
    }

    private async Task CloseAsync()
    {
        _maximized = false;

        // Restaura o foco antes de o diálogo sair do DOM (elemento ainda anexado para o JS).
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
        if (Modal && DismissableMask)
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
