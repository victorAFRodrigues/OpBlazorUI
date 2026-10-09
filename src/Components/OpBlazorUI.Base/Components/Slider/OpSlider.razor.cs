using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;
using OpBlazorUI.Base.Components.Forms;

namespace OpBlazorUI.Base.Components.Slider;

public partial class OpSlider : OpInputBase<double>
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

    [Parameter] public EventCallback<double> OnChange { get; set; }
    [Parameter] public EventCallback<double> OnSlideEnd { get; set; }

    private string RootClass => OpCss.BuildClass(
        "p-slider p-component",
        Orientation == "vertical" ? "p-slider-vertical" : "p-slider-horizontal",
        Animate ? "p-slider-animate" : null,
        Disabled ? "p-disabled" : null,
        StyleClass);

    private string RangeStyle => Orientation == "vertical"
        ? $"height: {OpCss.Num(SliderMath.ToPercent(CurrentValue, Min, Max))}%"
        : $"width: {OpCss.Num(SliderMath.ToPercent(CurrentValue, Min, Max))}%";

    private string HandleStyle => Orientation == "vertical"
        ? $"bottom: {OpCss.Num(SliderMath.ToPercent(CurrentValue, Min, Max))}%"
        : $"inset-inline-start: {OpCss.Num(SliderMath.ToPercent(CurrentValue, Min, Max))}%";

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
        if (_initializedOrientation is not null)
        {
            await InvokeDisposeAsync();
        }

        _sliderInterop?.Dispose();
        await base.DisposeAsync();
    }

    private async Task InvokeDisposeAsync()
    {
        try
        {
            await Interop.InvokeVoidAsync(OpInterop.SliderInterop, "dispose", _root);
        }
        catch (JSDisconnectedException)
        {
            // SSR estático / circuito desconectado
        }
    }

    private async Task HandleSlideAsync(double percent, int index)
    {
        var value = SliderMath.FromPercent(percent, Min, Max, Step);
        if (value.Equals(CurrentValue))
        {
            return;
        }

        CurrentValue = value;
        StateHasChanged();
        await OnChange.InvokeAsync(value);
    }

    private async Task HandleSlideEndAsync()
    {
        await OnSlideEnd.InvokeAsync(Value);
    }

    private async Task HandleKeyDown(KeyboardEventArgs e)
    {
        if (Disabled)
        {
            return;
        }

        switch (e.Code)
        {
            case "ArrowLeft":
            case "ArrowDown":
                await AdjustAsync(-1);
                break;

            case "ArrowRight":
            case "ArrowUp":
                await AdjustAsync(1);
                break;

            case "PageDown":
                await AdjustAsync(Step is null ? -10 : -1);
                break;

            case "PageUp":
                await AdjustAsync(Step is null ? 10 : 1);
                break;

            case "Home":
                await SetValueAsync(Min);
                break;

            case "End":
                await SetValueAsync(Max);
                break;
        }
    }

    private Task AdjustAsync(int direction)
    {
        var delta = (Step ?? 1) * direction;
        return SetValueAsync(CurrentValue + delta);
    }

    private async Task SetValueAsync(double value)
    {
        var next = SliderMath.Snap(value, Min, Max, Step);
        if (next.Equals(CurrentValue))
        {
            return;
        }

        CurrentValue = next;
        StateHasChanged();
        await OnChange.InvokeAsync(next);
    }
}
