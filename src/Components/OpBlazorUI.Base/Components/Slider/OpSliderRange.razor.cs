using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;
using OpBlazorUI.Base.Components.Forms;

namespace OpBlazorUI.Base.Components.Slider;

public partial class OpSliderRange : OpInputBase<double[]>
{
    private ElementReference _root;
    private OpSliderInterop? _sliderInterop;
    private string? _initializedOrientation;

    [Parameter] public double Min { get; set; }
    [Parameter] public double Max { get; set; } = 100;
    [Parameter] public double? Step { get; set; }
    [Parameter] public string Orientation { get; set; } = "horizontal";
    [Parameter] public bool Animate { get; set; }
    [Parameter] public int TabIndex { get; set; }
    [Parameter] public string? AriaLabel { get; set; }
    [Parameter] public string? AriaLabelledBy { get; set; }

    [Parameter] public EventCallback<double[]> OnChange { get; set; }
    [Parameter] public EventCallback<double[]> OnSlideEnd { get; set; }

    // Normaliza os valores para dentro de [Min, Max] e em ordem crescente: sem isso, um valor
    // fora do intervalo (ex.: Min=10 com Value=[0,5]) fazia lower > upper e o Math.Clamp lançar.
    private double[] Values
    {
        get
        {
            var v = Value is { Length: >= 2 } ? Value : new[] { Lo, Hi };
            var a = Math.Clamp(Math.Min(v[0], v[1]), Lo, Hi);
            var b = Math.Clamp(Math.Max(v[0], v[1]), Lo, Hi);
            return new[] { a, b };
        }
    }

    private double Lo => Math.Min(Min, Max);

    private double Hi => Math.Max(Min, Max);

    private string RootClass => OpCss.BuildClass(
        "p-slider p-component",
        Orientation == "vertical" ? "p-slider-vertical" : "p-slider-horizontal",
        Animate ? "p-slider-animate" : null,
        Disabled ? "p-disabled" : null,
        StyleClass);

    private double P0 => SliderMath.ToPercent(Math.Min(Values[0], Values[1]), Min, Max);

    private double P1 => SliderMath.ToPercent(Math.Max(Values[0], Values[1]), Min, Max);

    private string RangeStyle => Orientation == "vertical"
        ? $"bottom: {OpCss.Num(P0)}%; height: {OpCss.Num(P1 - P0)}%"
        : $"inset-inline-start: {OpCss.Num(P0)}%; width: {OpCss.Num(P1 - P0)}%";

    private string HandleStyle0 => Orientation == "vertical"
        ? $"bottom: {OpCss.Num(SliderMath.ToPercent(Values[0], Min, Max))}%"
        : $"inset-inline-start: {OpCss.Num(SliderMath.ToPercent(Values[0], Min, Max))}%";

    private string HandleStyle1 => Orientation == "vertical"
        ? $"bottom: {OpCss.Num(SliderMath.ToPercent(Values[1], Min, Max))}%"
        : $"inset-inline-start: {OpCss.Num(SliderMath.ToPercent(Values[1], Min, Max))}%";

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (_initializedOrientation == Orientation && _sliderInterop is not null)
        {
            return;
        }

        _sliderInterop ??= new OpSliderInterop(HandleSlideAsync, HandleSlideEndAsync);

        try
        {
            await Interop.InvokeVoidAsync(OpInterop.SliderInterop, "init", _sliderInterop.Reference, _root, Orientation);
            _initializedOrientation = Orientation;
        }
        catch (JSDisconnectedException)
        {
            // SSR estático / circuito desconectado
        }
    }

    public override async ValueTask DisposeAsync()
    {
        _sliderInterop?.Dispose();
        await base.DisposeAsync();
    }

    private async Task HandleSlideAsync(double percent, int index)
    {
        index = index == 1 ? 1 : 0;
        var values = (double[])Values.Clone();
        var lower = index == 0 ? Lo : values[0];
        var upper = index == 0 ? values[1] : Hi;

        var raw = Lo + (Hi - Lo) * percent;
        var value = Math.Clamp(SliderMath.Snap(raw, Min, Max, Step), lower, upper);

        if (values[index].Equals(value))
        {
            return;
        }

        values[index] = value;
        CurrentValue = values;
        await OnChange.InvokeAsync(values);
    }

    private async Task HandleSlideEndAsync()
    {
        await OnSlideEnd.InvokeAsync(Values);
    }

    private async Task HandleKeyDown(KeyboardEventArgs e, int index)
    {
        if (Disabled)
        {
            return;
        }

        switch (e.Code)
        {
            case "ArrowLeft":
            case "ArrowDown":
                await AdjustAsync(index, -1);
                break;

            case "ArrowRight":
            case "ArrowUp":
                await AdjustAsync(index, 1);
                break;

            case "PageDown":
                await AdjustAsync(index, Step is null ? -10 : -1);
                break;

            case "PageUp":
                await AdjustAsync(index, Step is null ? 10 : 1);
                break;

            case "Home":
                await SetValueAsync(index, Min);
                break;

            case "End":
                await SetValueAsync(index, Max);
                break;
        }
    }

    private Task AdjustAsync(int index, int direction)
    {
        var delta = (Step ?? 1) * direction;
        return SetValueAsync(index, Values[index] + delta);
    }

    private async Task SetValueAsync(int index, double value)
    {
        var values = (double[])Values.Clone();
        var lower = index == 0 ? Lo : values[0];
        var upper = index == 0 ? values[1] : Hi;

        var next = Math.Clamp(SliderMath.Snap(value, Lo, Hi, Step), lower, upper);
        if (values[index].Equals(next))
        {
            return;
        }

        values[index] = next;
        CurrentValue = values;
        await OnChange.InvokeAsync(values);
    }
}
