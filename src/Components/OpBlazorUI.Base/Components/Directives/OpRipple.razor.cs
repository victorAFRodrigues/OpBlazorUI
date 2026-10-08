using Microsoft.AspNetCore.Components;
using OpBlazorUI.Base.Components.Common;

namespace OpBlazorUI.Base.Components.Directives;

/// <summary>
/// Equivalente ao <c>pRipple</c> do upstream: adiciona as classes <c>p-ripple</c>/<c>p-ink</c>
/// e injeta o efeito de ondulação no clique. Use como contêiner do conteúdo.
/// </summary>
public partial class OpRipple : OpComponentBase
{
    private ElementReference _host;

    [Parameter] public string? Style { get; set; }

    /// <summary>Centraliza a ondulação no elemento (ignora a posição do ponteiro).</summary>
    [Parameter] public bool Center { get; set; }

    [Parameter] public RenderFragment? ChildContent { get; set; }

    private string RootClass => Class("p-ripple", StyleClass);

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            await Interop.InvokeVoidAsync(
                OpInterop.DirectivesInterop, "rippleInit", _host, new { center = Center });
        }
    }

    public override async ValueTask DisposeAsync()
    {
        try
        {
            await Interop.InvokeVoidAsync(OpInterop.DirectivesInterop, "rippleDispose", _host);
        }
        catch
        {
            // ignore
        }

        await base.DisposeAsync();
    }
}
