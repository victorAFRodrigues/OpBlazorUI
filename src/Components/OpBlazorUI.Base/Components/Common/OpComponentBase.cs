using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;

namespace OpBlazorUI.Base.Components.Common;

/// <summary>
/// Base comum dos componentes da biblioteca. Padroniza <see cref="StyleClass"/> e os atributos
/// livres (<c>@attributes</c>), oferece um helper de composição de classes e o <see cref="Interop"/>
/// (módulos JS com cache por componente e descarte automático).
/// </summary>
/// <remarks>
/// O Blazor só chama <see cref="DisposeAsync"/> em componentes <see cref="IAsyncDisposable"/>:
/// um <c>IDisposable.Dispose()</c> numa classe derivada nunca roda. Limpeza vai num override
/// de <see cref="DisposeAsync"/> que chama a base.
/// </remarks>
public abstract class OpComponentBase : ComponentBase, IAsyncDisposable
{
    private OpInterop? _interop;

    [Inject] protected IJSRuntime Js { get; set; } = default!;

    [Inject] private ILoggerFactory LoggerFactory { get; set; } = default!;

    [Parameter] public string? Id { get; set; }

    [Parameter] public string? StyleClass { get; set; }

    [Parameter(CaptureUnmatchedValues = true)]
    public Dictionary<string, object>? AdditionalAttributes { get; set; }

    /// <summary>
    /// Módulos JS da biblioteca. Depois do descarte continua devolvendo a mesma instância
    /// (já descartada), cujas chamadas viram no-op, em vez de recriar e vazar módulos.
    /// </summary>
    protected OpInterop Interop => _interop ??= new OpInterop(Js, LoggerFactory.CreateLogger(GetType()));

    protected static string Class(params string?[] classes) => OpCss.BuildClass(classes);

    public virtual async ValueTask DisposeAsync()
    {
        // Mesmo sem uso prévio, a instância descartada impede recriação por código async tardio.
        _interop ??= new OpInterop(Js);
        await _interop.DisposeAsync();
    }
}
