using System.Globalization;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;
using OpBlazorUI.Base.Components.Forms;

namespace OpBlazorUI.Base.Components.Knob;

public partial class OpKnob : OpInputBase<double>
{
    private const double Radius = 40;
    private const double MidX = 50;
    private const double MidY = 50;
    private static readonly double MinRadians = 4 * Math.PI / 3;
    private static readonly double MaxRadians = -Math.PI / 3;

    private ElementReference _svg;
    private OpKnobInterop? _knobInterop;

    [Parameter] public double Min { get; set; }
    [Parameter] public double Max { get; set; } = 100;
    [Parameter] public double Step { get; set; } = 1;
    [Parameter] public double Size { get; set; } = 100;
    [Parameter] public double StrokeWidth { get; set; } = 14;
    [Parameter] public bool ShowValue { get; set; } = true;
    [Parameter] public bool Readonly { get; set; }
    [Parameter] public bool Required { get; set; }
    [Parameter] public int TabIndex { get; set; }
    [Parameter] public string? Name { get; set; }
    [Parameter] public string? AriaLabel { get; set; }
    [Parameter] public string? AriaLabelledBy { get; set; }

    [Parameter] public string ValueColor { get; set; } = "var(--p-knob-value-background)";
    [Parameter] public string RangeColor { get; set; } = "var(--p-knob-range-background)";
    [Parameter] public string TextColor { get; set; } = "var(--p-knob-text-color)";
    [Parameter] public string ValueTemplate { get; set; } = "{value}";

    [Parameter] public EventCallback<double> OnChange { get; set; }

    private string RootClass => OpCss.BuildClass(
        "p-knob p-component",
        Disabled ? "p-disabled" : null,
        StyleClass);

    private MarkupString TextMarkup =>
        new($"<text class=\"p-knob-text\" x=\"50\" y=\"57\" text-anchor=\"middle\" fill=\"{TextColor}\" name=\"{Name}\">{ValueToDisplay}</text>");

    private string SvgStyle => $"width:{Size.ToString(CultureInfo.InvariantCulture)}px;height:{Size.ToString(CultureInfo.InvariantCulture)}px";

    private int TabIndexValue => Readonly || Disabled ? -1 : TabIndex;

    private string ValueToDisplay => ValueTemplate.Replace("{value}", CurrentValue.ToString(CultureInfo.InvariantCulture));

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (!firstRender)
        {
            return;
        }

        _knobInterop = new OpKnobInterop(HandlePointerAsync);

        try
        {
            await Interop.InvokeVoidAsync(OpInterop.KnobInterop, "init", _knobInterop.Reference, _svg);
        }
        catch (JSDisconnectedException)
        {
            // SSR estático / circuito desconectado
        }
    }

    public override async ValueTask DisposeAsync()
    {
        try
        {
            await Interop.InvokeVoidAsync(OpInterop.KnobInterop, "dispose", _svg);
        }
        catch (JSDisconnectedException)
        {
            // ignore
        }

        _knobInterop?.Dispose();
        await base.DisposeAsync();
    }

    private Task HandlePointerAsync(double offsetX, double offsetY)
    {
        if (Disabled || Readonly)
        {
            return Task.CompletedTask;
        }

        var dx = offsetX - Size / 2.0;
        var dy = Size / 2.0 - offsetY;
        var angle = Math.Atan2(dy, dx);
        return UpdateModelAsync(angle);
    }

    private async Task UpdateModelAsync(double angle)
    {
        var start = -Math.PI / 2 - Math.PI / 6;
        double mapped;

        if (angle > MaxRadians)
        {
            mapped = MapRange(angle, MinRadians, MaxRadians, Min, Max);
        }
        else if (angle < start)
        {
            mapped = MapRange(angle + 2 * Math.PI, MinRadians, MaxRadians, Min, Max);
        }
        else
        {
            return;
        }

        var steps = Step > 0 ? Step : 1;
        var newValue = Math.Round((mapped - Min) / steps, MidpointRounding.AwayFromZero) * steps + Min;
        await SetValueAsync(newValue);
    }

    private async Task HandleKeyDownAsync(KeyboardEventArgs e)
    {
        if (Disabled || Readonly)
        {
            return;
        }

        switch (e.Code)
        {
            case "ArrowRight":
            case "ArrowUp":
                await SetValueAsync(CurrentValue + 1);
                break;

            case "ArrowLeft":
            case "ArrowDown":
                await SetValueAsync(CurrentValue - 1);
                break;

            case "Home":
                await SetValueAsync(Min);
                break;

            case "End":
                await SetValueAsync(Max);
                break;

            case "PageUp":
                await SetValueAsync(CurrentValue + 10);
                break;

            case "PageDown":
                await SetValueAsync(CurrentValue - 10);
                break;
        }
    }

    private async Task SetValueAsync(double value)
    {
        var next = Math.Clamp(value, Min, Max);
        if (Math.Abs(next - CurrentValue) < double.Epsilon)
        {
            return;
        }

        CurrentValue = next;
        await OnChange.InvokeAsync(next);
    }

    private static double MapRange(double x, double inMin, double inMax, double outMin, double outMax)
        => (x - inMin) * (outMax - outMin) / (inMax - inMin) + outMin;

    private string RangePath =>
        $"M {F(MinX())} {F(MinY())} A {F(Radius)} {F(Radius)} 0 1 1 {F(MaxX())} {F(MaxY())}";

    private string ValuePath =>
        $"M {F(ZeroX())} {F(ZeroY())} A {F(Radius)} {F(Radius)} 0 {LargeArc()} {Sweep()} {F(ValueX())} {F(ValueY())}";

    private static string F(double value) => value.ToString("0.####", CultureInfo.InvariantCulture);

    private double ZeroRadians => Min > 0 && Max > 0
        ? MapRange(Min, Min, Max, MinRadians, MaxRadians)
        : MapRange(0, Min, Max, MinRadians, MaxRadians);

    private double ValueRadians => MapRange(CurrentValue, Min, Max, MinRadians, MaxRadians);

    private static double MinX() => MidX + Math.Cos(MinRadians) * Radius;
    private static double MinY() => MidY - Math.Sin(MinRadians) * Radius;
    private static double MaxX() => MidX + Math.Cos(MaxRadians) * Radius;
    private static double MaxY() => MidY - Math.Sin(MaxRadians) * Radius;

    private double ZeroX() => MidX + Math.Cos(ZeroRadians) * Radius;
    private double ZeroY() => MidY - Math.Sin(ZeroRadians) * Radius;
    private double ValueX() => MidX + Math.Cos(ValueRadians) * Radius;
    private double ValueY() => MidY - Math.Sin(ValueRadians) * Radius;

    private int LargeArc() => Math.Abs(ZeroRadians - ValueRadians) < Math.PI ? 0 : 1;
    private int Sweep() => ValueRadians > ZeroRadians ? 0 : 1;
}
