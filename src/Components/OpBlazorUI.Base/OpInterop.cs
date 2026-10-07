using Microsoft.JSInterop;

namespace OpBlazorUI.Base;

/// <summary>
/// Importa e mantém em cache os módulos JS da biblioteca para um componente e os descarta
/// junto com ele. Centraliza o <c>import</c> e o <c>IAsyncDisposable</c> que antes eram
/// repetidos (com <c>Lazy&lt;Task&lt;IJSObjectReference&gt;&gt;</c>) em cada componente.
/// </summary>
public sealed class OpInterop : IAsyncDisposable
{
    public const string OptimusInterop = "./_content/OpBlazorUI.Base/optimus.interop.js";
    public const string OverlayInterop = "./_content/OpBlazorUI.Base/overlay.interop.js";
    public const string VirtualScrollerInterop = "./_content/OpBlazorUI.Base/virtualscroller.interop.js";
    public const string EditorInterop = "./_content/OpBlazorUI.Base/editor.interop.js";

    private readonly IJSRuntime _js;
    private readonly Dictionary<string, Task<IJSObjectReference>> _modules = new(StringComparer.Ordinal);

    public OpInterop(IJSRuntime js) => _js = js;

    /// <summary>Importa (uma vez) o módulo. Falha de import devolve <c>null</c> em vez de lançar.</summary>
    public async ValueTask<IJSObjectReference?> ImportAsync(string path)
    {
        if (!_modules.TryGetValue(path, out var module))
        {
            module = _js.InvokeAsync<IJSObjectReference>("import", path).AsTask();
            _modules[path] = module;
        }

        try
        {
            return await module;
        }
        catch
        {
            return null;
        }
    }

    public async ValueTask InvokeVoidAsync(string path, string identifier, params object?[] args)
    {
        var module = await ImportAsync(path);
        if (module is not null)
        {
            await module.InvokeVoidAsync(identifier, args);
        }
    }

    public async ValueTask<TValue> InvokeAsync<TValue>(string path, string identifier, params object?[] args)
    {
        var module = await ImportAsync(path);
        return module is null ? default! : await module.InvokeAsync<TValue>(identifier, args);
    }

    public async ValueTask DisposeAsync()
    {
        foreach (var module in _modules.Values)
        {
            try
            {
                if (module.IsCompletedSuccessfully)
                {
                    await (await module).DisposeAsync();
                }
            }
            catch
            {
                // ignore
            }
        }

        _modules.Clear();
    }
}
