using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;

namespace OpBlazorUI.Base.Components.ScrollTop;

public partial class OpScrollTop : ComponentBase, IAsyncDisposable
{
    private const string JsModule = "./_content/OpBlazorUI.Base/optimus.interop.js";

    private ElementReference _root;
    private Lazy<Task<IJSObjectReference>>? _module;
    private DotNetObjectReference<OpScrollTop>? _selfRef;
    private bool _initialized;
    private bool _visible;
    private bool _disposed;

    [Parameter] public string Target { get; set; } = "window";

    [Parameter] public int Threshold { get; set; } = 400;

    [Parameter] public string Icon { get; set; } = "pi pi-angle-up";

    [Parameter] public string Behavior { get; set; } = "smooth";

    [Parameter] public string ButtonAriaLabel { get; set; } = "Scroll to top";

    [Parameter] public string? StyleClass { get; set; }

    [Parameter] public string? Style { get; set; }

    [Parameter] public RenderFragment? IconTemplate { get; set; }

    [Parameter(CaptureUnmatchedValues = true)]
    public Dictionary<string, object>? AdditionalAttributes { get; set; }

    [Inject] private IJSRuntime Js { get; set; } = default!;

    private string ButtonStyleClass => OpCss.BuildClass(
        "p-scrolltop",
        Target == "parent" ? "p-scrolltop-sticky" : null,
        !_visible ? "p-scrolltop-hidden" : null,
        StyleClass);

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender && !_initialized && !_disposed)
        {
            _initialized = true;
            var module = await GetModuleAsync();
            if (module is not null)
            {
                _selfRef = DotNetObjectReference.Create(this);
                try
                {
                    await module.InvokeVoidAsync("scrollTopInit", _root, _selfRef, Target);
                }
                catch
                {
                    // ignore
                }
            }
        }
    }

    [JSInvokable]
    public Task NotifyScrollTop(double top)
    {
        var visible = top > Threshold;
        if (visible != _visible)
        {
            _visible = visible;
            StateHasChanged();
        }

        return Task.CompletedTask;
    }

    private async Task OnClick(MouseEventArgs e)
    {
        var module = await GetModuleAsync();
        if (module is not null)
        {
            try
            {
                await module.InvokeVoidAsync("scrollTopTo", _root, Target, Behavior);
            }
            catch
            {
                // ignore
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
            var module = await GetModuleAsync();
            if (module is not null)
            {
                try
                {
                    await module.InvokeVoidAsync("scrollTopDispose", _root);
                }
                catch
                {
                    // ignore
                }
            }
        }

        _selfRef?.Dispose();

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
