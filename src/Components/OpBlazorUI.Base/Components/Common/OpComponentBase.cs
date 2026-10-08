using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace OpBlazorUI.Base.Components.Common;

/// <summary>
/// Base comum dos componentes da biblioteca. Padroniza <see cref="StyleClass"/> e os atributos
/// livres (<c>@attributes</c>), oferece um helper de composição de classes e o <see cref="Interop"/>
/// (módulos JS com cache por componente e descarte automático).
/// </summary>
public abstract class OpComponentBase : ComponentBase, IAsyncDisposable
{
    private OpInterop? _interop;

    [Inject] protected IJSRuntime Js { get; set; } = default!;

    [Parameter] public string? Id { get; set; }

    [Parameter] public string? StyleClass { get; set; }

    [Parameter(CaptureUnmatchedValues = true)]
    public Dictionary<string, object>? AdditionalAttributes { get; set; }

    protected OpInterop Interop => _interop ??= new OpInterop(Js);

    protected static string Class(params string?[] classes) => OpCss.BuildClass(classes);

    public virtual async ValueTask DisposeAsync()
    {
        if (_interop is not null)
        {
            await _interop.DisposeAsync();
            _interop = null;
        }
    }
}
