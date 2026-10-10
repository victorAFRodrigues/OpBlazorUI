using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;

namespace OpBlazorUI.Base;

/// <summary>
/// Importa e mantém em cache os módulos JS da biblioteca para um componente e os descarta
/// junto com ele. Centraliza o <c>import</c> e o <c>IAsyncDisposable</c> que antes eram
/// repetidos (com <c>Lazy&lt;Task&lt;IJSObjectReference&gt;&gt;</c>) em cada componente.
/// </summary>
/// <remarks>
/// As chamadas são tolerantes a falhas: queda do circuito, cancelamento e chamadas depois do
/// descarte viram no-op; erros do próprio JS (<see cref="JSException"/>) e falhas de import são
/// registrados no log em vez de derrubar o circuito. Depois de <see cref="DisposeAsync"/>, nenhum
/// módulo é importado de novo.
/// </remarks>
public sealed class OpInterop : IAsyncDisposable
{
    public const string OptimusInterop = "./_content/OpBlazorUI.Base/optimus.interop.js";
    public const string OverlayInterop = "./_content/OpBlazorUI.Base/overlay.interop.js";
    public const string VirtualScrollerInterop = "./_content/OpBlazorUI.Base/virtualscroller.interop.js";
    public const string EditorInterop = "./_content/OpBlazorUI.Base/editor.interop.js";
    public const string DirectivesInterop = "./_content/OpBlazorUI.Base/directives.interop.js";
    public const string TextareaInterop = "./_content/OpBlazorUI.Base/textarea.interop.js";
    public const string SliderInterop = "./_content/OpBlazorUI.Base/slider.interop.js";
    public const string ColorPickerInterop = "./_content/OpBlazorUI.Base/colorpicker.interop.js";
    public const string TabsInterop = "./_content/OpBlazorUI.Base/tabs.interop.js";
    public const string InputInterop = "./_content/OpBlazorUI.Base/input.interop.js";
    public const string MenuInterop = "./_content/OpBlazorUI.Base/menu.interop.js";
    public const string TreeInterop = "./_content/OpBlazorUI.Base/tree.interop.js";

    private readonly IJSRuntime _js;
    private readonly ILogger? _logger;
    private readonly Dictionary<string, Task<IJSObjectReference>> _modules = new(StringComparer.Ordinal);
    private bool _disposed;

    public OpInterop(IJSRuntime js, ILogger? logger = null)
    {
        _js = js;
        _logger = logger;
    }

    public bool IsDisposed => _disposed;

    /// <summary>
    /// Importa (uma vez) o módulo. Falha de import devolve <c>null</c> em vez de lançar e não
    /// fica em cache: a próxima chamada tenta de novo.
    /// </summary>
    public async ValueTask<IJSObjectReference?> ImportAsync(string path)
    {
        if (_disposed) return null;

        if (!_modules.TryGetValue(path, out var module))
        {
            module = _js.InvokeAsync<IJSObjectReference>("import", path).AsTask();
            _modules[path] = module;
        }

        try
        {
            return await module;
        }
        catch (Exception ex)
        {
            if (_modules.TryGetValue(path, out var cached) && cached == module)
            {
                _modules.Remove(path);
            }

            if (!IsTransient(ex))
            {
                _logger?.LogWarning(ex, "Falha ao importar o módulo JS {Path}.", path);
            }

            return null;
        }
    }

    public async ValueTask InvokeVoidAsync(string path, string identifier, params object?[] args)
    {
        var module = await ImportAsync(path);
        if (module is null || _disposed) return;

        try
        {
            await module.InvokeVoidAsync(identifier, args);
        }
        catch (Exception ex) when (Handle(ex, path, identifier))
        {
        }
    }

    public async ValueTask<TValue> InvokeAsync<TValue>(string path, string identifier, params object?[] args)
    {
        var module = await ImportAsync(path);
        if (module is null || _disposed) return default!;

        try
        {
            return await module.InvokeAsync<TValue>(identifier, args);
        }
        catch (Exception ex) when (Handle(ex, path, identifier))
        {
            return default!;
        }
    }

    public async ValueTask DisposeAsync()
    {
        if (_disposed) return;
        _disposed = true;

        var modules = _modules.Values.ToList();
        _modules.Clear();

        foreach (var module in modules)
        {
            try
            {
                // Inclui imports ainda em andamento: o módulo que chegar depois também é descartado.
                await (await module).DisposeAsync();
            }
            catch
            {
                // circuito encerrado ou import com falha: nada a descartar
            }
        }
    }

    /// <summary>Queda do circuito, cancelamento ou objeto já descartado.</summary>
    private static bool IsTransient(Exception ex) =>
        ex is JSDisconnectedException or OperationCanceledException or ObjectDisposedException;

    private bool Handle(Exception ex, string path, string identifier)
    {
        if (IsTransient(ex)) return true;

        if (ex is JSException)
        {
            _logger?.LogWarning(ex, "Erro no JS ao chamar {Identifier} em {Path}.", identifier, path);
            return true;
        }

        return false;
    }
}
