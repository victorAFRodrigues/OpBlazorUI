using Microsoft.JSInterop;

namespace OpBlazorUI.Base.Components.Slider;

/// <summary>
/// Ponte entre o interop de arraste do slider e o componente. Mantém o
/// <see cref="DotNetObjectReference{T}"/> e converte as chamadas JS em callbacks.
/// </summary>
public sealed class OpSliderInterop : IDisposable
{
    private readonly DotNetObjectReference<OpSliderInterop> _reference;
    private readonly Func<double, int, Task> _onSlide;
    private readonly Func<Task> _onSlideEnd;

    public OpSliderInterop(Func<double, int, Task> onSlide, Func<Task> onSlideEnd)
    {
        _onSlide = onSlide;
        _onSlideEnd = onSlideEnd;
        _reference = DotNetObjectReference.Create(this);
    }

    public DotNetObjectReference<OpSliderInterop> Reference => _reference;

    [JSInvokable]
    public Task OnSlide(double percent, int index) => _onSlide(percent, index);

    [JSInvokable]
    public Task OnSlideEnd() => _onSlideEnd();

    public void Dispose() => _reference.Dispose();
}

internal static class SliderMath
{
    public static double Snap(double value, double min, double max, double? step)
    {
        var clamped = Math.Clamp(value, min, max);

        if (step is > 0)
        {
            var steps = Math.Round((clamped - min) / step.Value, MidpointRounding.AwayFromZero);
            clamped = min + steps * step.Value;

            var decimals = Decimals(step.Value);
            if (decimals > 0)
            {
                clamped = Math.Round(clamped, decimals);
            }
        }
        else
        {
            // Sem Step, arredonda para inteiro (como o step padrão 1 do PrimeNG).
            clamped = Math.Round(clamped, MidpointRounding.AwayFromZero);
        }

        return Math.Clamp(clamped, min, max);
    }

    public static double FromPercent(double percent, double min, double max, double? step)
        => Snap(min + (max - min) * percent, min, max, step);

    public static double ToPercent(double value, double min, double max)
    {
        if (max <= min)
        {
            return 0;
        }

        return Math.Clamp((value - min) / (max - min) * 100, 0, 100);
    }

    private static int Decimals(double step)
    {
        var text = step.ToString(System.Globalization.CultureInfo.InvariantCulture);
        var dot = text.IndexOf('.');
        return dot < 0 ? 0 : text.Length - dot - 1;
    }
}
