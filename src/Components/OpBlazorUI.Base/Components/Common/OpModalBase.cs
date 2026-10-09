using Microsoft.AspNetCore.Components;

namespace OpBlazorUI.Base.Components.Common;

/// <summary>
/// Base para overlays modais que precisam prender o foco e restaurá-lo ao gatilho ao fechar.
/// O trap é aplicado direto no elemento raiz via JS (sem wrapper), preservando o DOM do upstream.
/// </summary>
public abstract class OpModalBase : OpComponentBase
{
    /// <summary>Indica que o trap de foco está ativo (para restaurar o foco uma única vez).</summary>
    protected bool IsFocusTrapActive { get; private set; }

    protected async Task InitFocusTrapAsync(ElementReference element, string? initialFocusSelector = null)
    {
        await Interop.InvokeVoidAsync(OpInterop.OptimusInterop, "focusTrapInit", element, initialFocusSelector, true);
        IsFocusTrapActive = true;
    }

    /// <summary>
    /// Restaura o foco ao gatilho e desfaz o trap. Precisa rodar com o elemento ainda no DOM
    /// (ex.: em <c>OnParametersSetAsync</c>, antes de o pai remover o overlay).
    /// </summary>
    protected async Task RestoreFocusAsync(ElementReference element)
    {
        if (!IsFocusTrapActive) return;
        IsFocusTrapActive = false;
        try
        {
            await Interop.InvokeVoidAsync(OpInterop.OptimusInterop, "focusTrapDispose", element);
        }
        catch
        {
            // elemento já fora do DOM / circuito encerrado
        }
    }
}
