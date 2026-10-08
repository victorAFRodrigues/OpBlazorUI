using Microsoft.AspNetCore.Components;
using OpBlazorUI.Base.Components.Common;

namespace OpBlazorUI.Base.Components.Directives;

/// <summary>
/// Equivalente ao <c>pAutoFocus</c>: foca o primeiro elemento focável do conteúdo ao montar
/// e devolve o foco ao elemento anterior ao desmontar.
/// Desvio consciente: o upstream aplica a diretiva ao próprio input; aqui o componente
/// renderiza um <c>&lt;span style="display: contents"&gt;</c> em volta dele.
/// </summary>
public partial class OpAutoFocus : OpComponentBase
{
    private ElementReference _host;

    [Parameter] public bool Enabled { get; set; } = true;

    [Parameter] public RenderFragment? ChildContent { get; set; }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender && Enabled)
        {
            await Interop.InvokeVoidAsync(OpInterop.DirectivesInterop, "autoFocusInit", _host);
        }
    }

    public override async ValueTask DisposeAsync()
    {
        if (Enabled)
        {
            try
            {
                await Interop.InvokeVoidAsync(OpInterop.DirectivesInterop, "autoFocusDispose", _host);
            }
            catch
            {
                // ignore
            }
        }

        await base.DisposeAsync();
    }
}
