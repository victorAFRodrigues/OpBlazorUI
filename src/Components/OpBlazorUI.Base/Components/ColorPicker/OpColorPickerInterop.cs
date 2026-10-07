using Microsoft.JSInterop;

namespace OpBlazorUI.Base.Components.ColorPicker;

/// <summary>Ponte do interop do ColorPicker (arraste do seletor/hue e fechamento por clique fora).</summary>
public sealed class OpColorPickerInterop : IDisposable
{
    private readonly DotNetObjectReference<OpColorPickerInterop> _reference;
    private readonly Func<double, double, Task> _onColor;
    private readonly Func<double, Task> _onHue;
    private readonly Func<Task> _onOutsideClick;

    public OpColorPickerInterop(Func<double, double, Task> onColor, Func<double, Task> onHue, Func<Task> onOutsideClick)
    {
        _onColor = onColor;
        _onHue = onHue;
        _onOutsideClick = onOutsideClick;
        _reference = DotNetObjectReference.Create(this);
    }

    public DotNetObjectReference<OpColorPickerInterop> Reference => _reference;

    [JSInvokable]
    public Task OnPickColor(double saturation, double brightness) => _onColor(saturation, brightness);

    [JSInvokable]
    public Task OnPickHue(double hue) => _onHue(hue);

    [JSInvokable]
    public Task OnOutsideClick() => _onOutsideClick();

    public void Dispose() => _reference.Dispose();
}
