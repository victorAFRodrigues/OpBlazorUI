using Microsoft.AspNetCore.Components;
using OpBlazorUI.Base.Components.Common;

namespace OpBlazorUI.Base.Components.Directives;

/// <summary>
/// Equivalente ao <c>pAnimateOnScroll</c>: aplica <see cref="EnterClass"/> quando o elemento
/// entra na viewport e <see cref="LeaveClass"/> quando sai.
/// Desvio consciente: o upstream aplica a diretiva ao próprio elemento hospedeiro; aqui o
/// componente renderiza um <c>&lt;div&gt;</c> contêiner que é o elemento observado/animado.
/// </summary>
public partial class OpAnimateOnScroll : OpComponentBase
{
    private ElementReference _host;

    /// <summary>Classes aplicadas ao entrar na viewport.</summary>
    [Parameter] public string? EnterClass { get; set; }

    /// <summary>Classes aplicadas ao sair da viewport.</summary>
    [Parameter] public string? LeaveClass { get; set; }

    /// <summary>Fração do elemento visível para disparar o observer (0 a 1).</summary>
    [Parameter] public double Threshold { get; set; }

    [Parameter] public RenderFragment? ChildContent { get; set; }

    private string RootClass => Class(StyleClass);

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            await Interop.InvokeVoidAsync(OpInterop.DirectivesInterop, "animateOnScrollInit", _host, new
            {
                enterClass = EnterClass,
                leaveClass = LeaveClass,
                threshold = Threshold
            });
        }
    }

    public override async ValueTask DisposeAsync()
    {
        try
        {
            await Interop.InvokeVoidAsync(OpInterop.DirectivesInterop, "animateOnScrollDispose", _host);
        }
        catch
        {
            // ignore
        }

        await base.DisposeAsync();
    }
}
