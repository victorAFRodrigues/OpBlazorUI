using Microsoft.JSInterop;

namespace OpBlazorUI.Base.Components.Splitter;

/// <summary>Ponte entre o arraste do Splitter e o componente.</summary>
public sealed class OpSplitterInterop : IDisposable
{
    private readonly DotNetObjectReference<OpSplitterInterop> _reference;
    private readonly Func<double[], Task> _onResizeStart;
    private readonly Func<double[], Task> _onResizeEnd;

    public OpSplitterInterop(Func<double[], Task> onResizeStart, Func<double[], Task> onResizeEnd)
    {
        _onResizeStart = onResizeStart;
        _onResizeEnd = onResizeEnd;
        _reference = DotNetObjectReference.Create(this);
    }

    public DotNetObjectReference<OpSplitterInterop> Reference => _reference;

    [JSInvokable]
    public Task OnResizeStart(double[] sizes) => _onResizeStart(sizes);

    [JSInvokable]
    public Task OnResizeEnd(double[] sizes) => _onResizeEnd(sizes);

    public void Dispose() => _reference.Dispose();
}
