using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace OpBlazorUI.Base.Components.Common;

/// <summary>
/// Base para overlays modais que precisam prender o foco e restaurá-lo ao gatilho ao fechar.
/// O trap é aplicado direto no elemento raiz via JS (sem wrapper), preservando o DOM do upstream.
/// </summary>
public abstract class OpModalBase : ComponentBase, IAsyncDisposable
{
    private const string JsModule = "./_content/OpBlazorUI.Base/optimus.interop.js";

    private Lazy<Task<IJSObjectReference>>? _module;

    [Inject] private IJSRuntime Js { get; set; } = default!;

    protected async Task FocusTrapInitAsync(ElementReference element, string? initialFocusSelector = null)
    {
        var module = await GetModuleAsync();
        if (module is not null)
        {
            await module.InvokeVoidAsync("focusTrapInit", element, initialFocusSelector, true);
        }
    }

    protected async Task FocusTrapDisposeAsync(ElementReference element)
    {
        var module = await GetModuleAsync();
        if (module is not null)
        {
            await module.InvokeVoidAsync("focusTrapDispose", element);
        }
    }

    private async Task<IJSObjectReference?> GetModuleAsync()
    {
        _module ??= new Lazy<Task<IJSObjectReference>>(() =>
            Js.InvokeAsync<IJSObjectReference>("import", JsModule).AsTask());

        try
        {
            return await _module.Value;
        }
        catch
        {
            return null;
        }
    }

    public virtual async ValueTask DisposeAsync()
    {
        if (_module is { IsValueCreated: true })
        {
            try
            {
                await (await _module.Value).DisposeAsync();
            }
            catch
            {
                // ignore
            }
        }
    }
}
