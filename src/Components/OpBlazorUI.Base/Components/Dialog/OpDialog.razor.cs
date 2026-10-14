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
    private ElementReference _header;

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

    /// <summary>Trava a rolagem do body enquanto o diálogo está aberto.</summary>
    [Parameter] public bool BlockScroll { get; set; }

    /// <summary>Permite arrastar o diálogo pelo cabeçalho.</summary>
    [Parameter] public bool Draggable { get; set; } = true;

    /// <summary>Permite redimensionar o diálogo pelas bordas/cantos.</summary>
    [Parameter] public bool Resizable { get; set; }

    /// <summary>Mantém o diálogo dentro da viewport ao arrastar/redimensionar.</summary>
    [Parameter] public bool KeepInViewport { get; set; } = true;

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

    protected override async Task OnParametersSetAsync()
    {
        // O pai pode fechar via binding: restaura o foco antes de o elemento sair do DOM.
        if (!Visible)
        {
            await RestoreFocusAsync(_root);
        }
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        var wasVisible = _lastRenderedVisible;

        if (Visible && !wasVisible)
        {
            await OnShow.InvokeAsync();

            // Trap antes do foco: a base JS captura o gatilho (foco atual) para restaurar depois.
            if (Modal)
            {
                try
                {
                    await InitFocusTrapAsync(_root);
                }
                catch
                {
                    // ignore
                }
            }

            if (BlockScroll)
            {
                await BlockScrollAsync();
            }

            if (!_maximized && Draggable && ShowHeader)
            {
                await Interop.InvokeVoidAsync(OpInterop.DialogInterop, "initDraggable", _root, _header, KeepInViewport);
            }

            if (!_maximized && Resizable)
            {
                await Interop.InvokeVoidAsync(OpInterop.DialogInterop, "initResizable", _root, 150, 100, KeepInViewport);
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
        else if (!Visible && wasVisible)
        {
            await UnblockScrollAsync();
        }

        _lastRenderedVisible = Visible;
    }

    private string RootClass => OpCss.BuildClass(
        "p-dialog p-component",
        "p-dialog-enter-active",
        _maximized ? "p-dialog-maximized" : null,
        Resizable && !_maximized ? "p-dialog-resizable" : null,
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
        if (Modal)
        {
            await RestoreFocusAsync(_root);
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

    // Escape chega pelo OpOverlayAttach só quando este é o overlay do topo (um Select aberto
    // dentro do diálogo fecha antes), mesmo com o foco fora do diálogo.
    private EventCallback EscapeCallback =>
        CloseOnEscape ? EventCallback.Factory.Create(this, CloseAsync) : default;

    private void ToggleMaximize(MouseEventArgs _)
    {
        _maximized = !_maximized;
    }

    public override async ValueTask DisposeAsync()
    {
        await UnblockScrollAsync();
        await RestoreFocusAsync(_root);
        await base.DisposeAsync();
    }
}
