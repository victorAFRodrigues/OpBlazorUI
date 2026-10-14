using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;
using OpBlazorUI.Base.Components.Forms;

namespace OpBlazorUI.Base.Components.ColorPicker;

public partial class OpColorPicker : OpInputBase<string>
{
    private ElementReference _root;
    private ElementReference _selector;
    private ElementReference _hue;
    private OpColorPickerInterop? _colorInterop;
    private bool _panelVisible;
    private bool _interopInitialized;
    private string? _lastParsed;
    private double _h;
    private double _s = 100;
    private double _b = 100;

    [Parameter] public string Format { get; set; } = "hex";
    [Parameter] public bool Inline { get; set; }
    [Parameter] public int TabIndex { get; set; }
    [Parameter] public string? InputId { get; set; }
    [Parameter] public string? AriaLabel { get; set; }
    [Parameter] public string DefaultColor { get; set; } = "ff0000";

    [Parameter] public EventCallback<string> OnChange { get; set; }
    [Parameter] public EventCallback OnShow { get; set; }
    [Parameter] public EventCallback OnHide { get; set; }

    private string RootClass => OpCss.BuildClass(
        "p-colorpicker p-component",
        Disabled ? "p-disabled" : null,
        StyleClass);

    private string PanelClass => OpCss.BuildClass(
        "p-colorpicker-panel",
        Inline ? "p-colorpicker-panel-inline" : null);

    private string? PanelStyle => Inline ? null : "top: calc(100% + 0.25rem); left: 0; z-index: 1000;";

    private string CurrentHex => OpColorMath.HsbToHex(_h, _s, _b);

    private string HueHex => OpColorMath.HsbToHex(_h, 100, 100);

    private string ColorHandleStyle =>
        $"left: {Math.Round(150.0 * _s / 100):0}px; top: {Math.Round(150.0 * (100 - _b) / 100):0}px";

    private string HueHandleStyle =>
        $"top: {Math.Round(150 - 150.0 * _h / 360):0}px";

    protected override void OnParametersSet()
    {
        if (!string.Equals(_lastParsed, CurrentValue, StringComparison.Ordinal))
        {
            _lastParsed = CurrentValue;
            (_h, _s, _b) = OpColorMath.Parse(CurrentValue, Format, DefaultColor);
        }
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        var shouldInitialize = !Disabled && (Inline || _panelVisible);

        if (!shouldInitialize)
        {
            // Painel fechado/desabilitado: remove os listeners do document uma única vez.
            if (_interopInitialized)
            {
                _interopInitialized = false;
                await InvokeDisposeAsync();
            }

            return;
        }

        if (_interopInitialized)
        {
            return;
        }

        _colorInterop ??= new OpColorPickerInterop(HandlePickColorAsync, HandlePickHueAsync, HandleOutsideClickAsync);

        try
        {
            await Interop.InvokeVoidAsync(OpInterop.ColorPickerInterop, "init", _colorInterop.Reference, _root, _selector, _hue, !Inline);
            _interopInitialized = true;
        }
        catch (JSDisconnectedException)
        {
            // SSR estático / circuito desconectado
        }
    }

    private async Task InvokeDisposeAsync()
    {
        try
        {
            await Interop.InvokeVoidAsync(OpInterop.ColorPickerInterop, "dispose", _root);
        }
        catch (JSDisconnectedException)
        {
            // SSR estático / circuito desconectado
        }
    }

    public override async ValueTask DisposeAsync()
    {
        if (_interopInitialized)
        {
            _interopInitialized = false;
            await InvokeDisposeAsync();
        }

        _colorInterop?.Dispose();
        await base.DisposeAsync();
    }

    private async Task TogglePanel()
    {
        if (Inline || Disabled)
        {
            return;
        }

        _panelVisible = !_panelVisible;

        if (_panelVisible)
        {
            await OnShow.InvokeAsync();
        }
        else
        {
            await OnHide.InvokeAsync();
        }
    }

    private Task HandleInputKeyDown(KeyboardEventArgs e)
    {
        switch (e.Code)
        {
            case "Space":
            case "Enter":
                return TogglePanel();

            case "Escape":
            case "Tab":
                if (_panelVisible)
                {
                    return HideAsync();
                }
                break;
        }

        return Task.CompletedTask;
    }

    private Task HandleOutsideClickAsync() => HideAsync();

    private async Task HideAsync()
    {
        if (!_panelVisible)
        {
            return;
        }

        _panelVisible = false;
        await OnHide.InvokeAsync();
    }

    private async Task HandlePickColorAsync(double saturation, double brightness)
    {
        _s = Math.Clamp(saturation, 0, 100);
        _b = Math.Clamp(brightness, 0, 100);
        await CommitAsync();
    }

    private async Task HandlePickHueAsync(double hue)
    {
        _h = Math.Clamp(hue, 0, 360);
        await CommitAsync();
    }

    private async Task CommitAsync()
    {
        var value = OpColorMath.Serialize(_h, _s, _b, Format);
        _lastParsed = value;
        CurrentValue = value;
        await OnChange.InvokeAsync(value);
    }
}
