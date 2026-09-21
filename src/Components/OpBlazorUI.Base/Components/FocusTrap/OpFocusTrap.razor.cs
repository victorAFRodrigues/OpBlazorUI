using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace OpBlazorUI.Base.Components.FocusTrap;

public partial class OpFocusTrap : ComponentBase, IAsyncDisposable
{
    private const string JsModule = "./_content/OpBlazorUI.Base/optimus.interop.js";

    private ElementReference _root;
    private Lazy<Task<IJSObjectReference>>? _module;
    private bool _initialized;
    private bool _disposed;

    [Parameter] public bool Disabled { get; set; }

    [Parameter] public string? StyleClass { get; set; }

    [Parameter] public RenderFragment? ChildContent { get; set; }

    [Parameter(CaptureUnmatchedValues = true)]
    public Dictionary<string, object>? AdditionalAttributes { get; set; }

    [Inject] private IJSRuntime Js { get; set; } = default!;

    private string RootClass => OpCss.BuildClass("p-focustrap", StyleClass);

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender && !_initialized && !_disposed)
        {
            _initialized = true;
            var module = await GetModuleAsync();
            if (module is not null)
            {
                await module.InvokeVoidAsync("focusTrapInit", _root);
            }
        }
    }

    private async Task<IJSObjectReference?> GetModuleAsync()
    {
        if (_module is null)
        {
            _module = new Lazy<Task<IJSObjectReference>>(() =>
                Js.InvokeAsync<IJSObjectReference>("import", JsModule).AsTask());
        }

        try
        {
            return await _module.Value;
        }
        catch
        {
            return null;
        }
    }

    public async ValueTask DisposeAsync()
    {
        if (_disposed) return;
        _disposed = true;

        if (_initialized)
        {
            try
            {
                var module = await GetModuleAsync();
                if (module is not null)
                {
                    await module.InvokeVoidAsync("focusTrapDispose", _root);
                }
            }
            catch
            {
                // ignore
            }
        }

        if (_module is { IsValueCreated: true })
        {
            try
            {
                var module = await _module.Value;
                await module.DisposeAsync();
            }
            catch
            {
                // ignore
            }
        }
    }
}
