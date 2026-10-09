using System.Globalization;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using OpBlazorUI.Base.Components.Common;

namespace OpBlazorUI.Base.Components.ScrollPanel;

public partial class OpScrollPanel : OpComponentBase
{
    private readonly string _uid = "osp_" + Guid.NewGuid().ToString("N")[..8];
    private ElementReference _root;
    private bool _initialized;

    /// <summary>Passo de rolagem pelas setas do teclado (px).</summary>
    [Parameter] public double Step { get; set; } = 5;

    [Parameter] public string? Style { get; set; }
    [Parameter] public RenderFragment? ContentTemplate { get; set; }
    [Parameter] public RenderFragment? ChildContent { get; set; }

    private string RootId => Id ?? _uid;
    private string ContentId => RootId + "_content";

    private string RootClass => Class("p-scrollpanel p-component", StyleClass);

    private string RootStyle => string.IsNullOrEmpty(Style) ? "position:relative" : $"position:relative;{Style}";

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            _initialized = true;
            await InitAsync();
        }
    }

    private async Task InitAsync()
    {
        try
        {
            await Interop.InvokeVoidAsync(OpInterop.ScrollPanelInterop, "init", _root, Step);
        }
        catch (JSDisconnectedException)
        {
            // SSR estático / circuito desconectado
        }
    }

    /// <summary>Define a posição vertical da rolagem (px).</summary>
    public async Task ScrollTopAsync(double scrollTop)
    {
        if (!_initialized)
        {
            return;
        }

        try
        {
            await Interop.InvokeVoidAsync(OpInterop.ScrollPanelInterop, "scrollTop", _root, scrollTop);
        }
        catch (JSDisconnectedException)
        {
            // ignore
        }
    }

    /// <summary>Recalcula o tamanho/posição das barras.</summary>
    public async Task RefreshAsync()
    {
        if (!_initialized)
        {
            return;
        }

        try
        {
            await Interop.InvokeVoidAsync(OpInterop.ScrollPanelInterop, "refresh", _root);
        }
        catch (JSDisconnectedException)
        {
            // ignore
        }
    }

    public override async ValueTask DisposeAsync()
    {
        try
        {
            await Interop.InvokeVoidAsync(OpInterop.ScrollPanelInterop, "dispose", _root);
        }
        catch (JSDisconnectedException)
        {
            // ignore
        }

        await base.DisposeAsync();
    }
}
