using Microsoft.AspNetCore.Components;

namespace OpBlazorUI.Base.Components.Common;

/// <summary>
/// Base para overlays modais que precisam prender o foco e restaurá-lo ao gatilho ao fechar.
/// O trap é aplicado direto no elemento raiz via JS (sem wrapper), preservando o DOM do upstream.
/// </summary>
public abstract class OpModalBase : OpComponentBase
{
    protected Task FocusTrapInitAsync(ElementReference element, string? initialFocusSelector = null)
        => Interop.InvokeVoidAsync(OpInterop.OptimusInterop, "focusTrapInit", element, initialFocusSelector, true)
            .AsTask();

    protected Task FocusTrapDisposeAsync(ElementReference element)
        => Interop.InvokeVoidAsync(OpInterop.OptimusInterop, "focusTrapDispose", element).AsTask();
}
