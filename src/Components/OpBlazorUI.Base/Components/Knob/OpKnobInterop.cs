using Microsoft.JSInterop;

namespace OpBlazorUI.Base.Components.Knob;

/// <summary>Ponte entre o arraste do Knob e o componente.</summary>
public sealed class OpKnobInterop : IDisposable
{
    private readonly DotNetObjectReference<OpKnobInterop> _reference;
    private readonly Func<double, double, Task> _onPointer;

    public OpKnobInterop(Func<double, double, Task> onPointer)
    {
        _onPointer = onPointer;
        _reference = DotNetObjectReference.Create(this);
    }

    public DotNetObjectReference<OpKnobInterop> Reference => _reference;

    [JSInvokable]
    public Task OnPointer(double offsetX, double offsetY) => _onPointer(offsetX, offsetY);

    public void Dispose() => _reference.Dispose();
}
