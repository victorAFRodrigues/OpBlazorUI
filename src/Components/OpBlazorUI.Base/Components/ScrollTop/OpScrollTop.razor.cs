using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;
using OpBlazorUI.Base.Components.Common;

namespace OpBlazorUI.Base.Components.ScrollTop;

public partial class OpScrollTop : OpComponentBase
{
    private ElementReference _root;
    private DotNetObjectReference<OpScrollTop>? _selfRef;
    private bool _initialized;
    private bool _visible;
    private bool _disposed;

    [Parameter] public string Target { get; set; } = "window";

    [Parameter] public int Threshold { get; set; } = 400;

    [Parameter] public string Icon { get; set; } = "pi pi-angle-up";

    [Parameter] public string Behavior { get; set; } = "smooth";

    [Parameter] public string ButtonAriaLabel { get; set; } = "Scroll to top";

    [Parameter] public string? Style { get; set; }

    [Parameter] public RenderFragment? IconTemplate { get; set; }

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
            _selfRef = DotNetObjectReference.Create(this);
            try
            {
                await Interop.InvokeVoidAsync(OpInterop.OptimusInterop, "scrollTopInit", _root, _selfRef, Target);
            }
            catch
            {
                // ignore
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
        try
        {
            await Interop.InvokeVoidAsync(OpInterop.OptimusInterop, "scrollTopTo", _root, Target, Behavior);
        }
        catch
        {
            // ignore
        }
    }

    public override async ValueTask DisposeAsync()
    {
        if (_disposed) return;
        _disposed = true;

        if (_initialized)
        {
            try
            {
                await Interop.InvokeVoidAsync(OpInterop.OptimusInterop, "scrollTopDispose", _root);
            }
            catch
            {
                // ignore
            }
        }

        _selfRef?.Dispose();
        await base.DisposeAsync();
    }
}
