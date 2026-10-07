using Microsoft.AspNetCore.Components;
using OpBlazorUI.Base.Components.Common;

namespace OpBlazorUI.Base.Components.FocusTrap;

public partial class OpFocusTrap : OpComponentBase
{
    private ElementReference _root;
    private bool _initialized;
    private bool _disposed;

    [Parameter] public bool Disabled { get; set; }

    [Parameter] public RenderFragment? ChildContent { get; set; }

    private string RootClass => OpCss.BuildClass("p-focustrap", StyleClass);

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender && !_initialized && !_disposed)
        {
            _initialized = true;
            await Interop.InvokeVoidAsync(OpInterop.OptimusInterop, "focusTrapInit", _root, null, false);
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
                await Interop.InvokeVoidAsync(OpInterop.OptimusInterop, "focusTrapDispose", _root);
            }
            catch
            {
                // ignore
            }
        }

        await base.DisposeAsync();
    }
}
