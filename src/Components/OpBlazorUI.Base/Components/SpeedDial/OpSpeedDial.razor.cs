using System.Globalization;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;
using OpBlazorUI.Base.Components.Common;
using OpBlazorUI.Base.Models;

namespace OpBlazorUI.Base.Components.SpeedDial;

public sealed class SpeedDialButtonContext
{
    public Action? Toggle { get; init; }
}

/// <summary>Contexto do <c>ItemTemplate</c>: o item, o índice e a ação de clique (executa o comando e fecha).</summary>
public sealed class SpeedDialItemContext
{
    public required OpMenuItem Item { get; init; }
    public int Index { get; init; }
    public required Action Click { get; init; }
}

public partial class OpSpeedDial : OpComponentBase
{
    private string _id = "";
    private bool _visible;
    private bool _focusFirstAction;

    // ---------------------------------------------------------------- params
    [Parameter] public IReadOnlyList<OpMenuItem>? Model { get; set; }
    [Parameter] public bool Visible { get; set; }
    [Parameter] public EventCallback<bool> VisibleChanged { get; set; }
    [Parameter] public int TransitionDelay { get; set; } = 30;
    [Parameter] public string Type { get; set; } = "linear";
    [Parameter] public int Radius { get; set; }
    [Parameter] public string Direction { get; set; } = "up";
    [Parameter] public bool Mask { get; set; }
    [Parameter] public bool Disabled { get; set; }
    [Parameter] public bool HideOnClickOutside { get; set; } = true;
    // Sem ShowIcon, o botão usa o glyph "plus" da fonte (como o PlusIcon do upstream).
    [Parameter] public string? ShowIcon { get; set; }
    [Parameter] public string? HideIcon { get; set; }
    [Parameter] public bool RotateAnimation { get; set; } = true;
    [Parameter] public string? AriaLabel { get; set; }
    [Parameter] public string? AriaLabelledBy { get; set; }
    [Parameter] public string? Style { get; set; }
    [Parameter] public string? ButtonStyleClass { get; set; }
    [Parameter] public string? ButtonStyle { get; set; }
    [Parameter] public string? MaskStyleClass { get; set; }
    [Parameter] public string? MaskStyle { get; set; }
    [Parameter] public bool Tooltip { get; set; }
    [Parameter] public string TooltipPosition { get; set; } = "right";
    [Parameter] public string? ButtonSeverity { get; set; }
    [Parameter] public bool ButtonRaised { get; set; }
    [Parameter] public bool ButtonText { get; set; }
    [Parameter] public bool ButtonOutlined { get; set; }
    [Parameter] public string? ButtonSize { get; set; }
    [Parameter] public int ButtonTabIndex { get; set; }

    // events
    [Parameter] public EventCallback<MouseEventArgs> OnClick { get; set; }
    [Parameter] public EventCallback OnShow { get; set; }
    [Parameter] public EventCallback OnHide { get; set; }

    // templates
    [Parameter] public RenderFragment<SpeedDialButtonContext>? ButtonTemplate { get; set; }
    [Parameter] public RenderFragment<SpeedDialItemContext>? ItemTemplate { get; set; }
    [Parameter] public RenderFragment? IconTemplate { get; set; }

    private ElementReference _root;
    private DotNetObjectReference<OpSpeedDial>? _selfRef;
    private bool _listening;
    private bool _disposed;

    // ------------------------------------------------------------ lifecycle
    protected override void OnInitialized()
    {
        _id = $"op-speeddial-{Guid.NewGuid():N}";
        _visible = _lastVisibleParam = Visible;
    }

    // Só sincroniza quando o parâmetro muda: um re-render do pai (ex.: o OnClick do ButtonTemplate)
    // não pode fechar o menu que acabou de abrir sem @bind-Visible.
    private bool _lastVisibleParam;

    protected override void OnParametersSet()
    {
        if (Visible == _lastVisibleParam) return;
        _lastVisibleParam = Visible;
        _visible = Visible;
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (_disposed) return;
        try
        {
            if (firstRender && Type != "linear")
            {
                await Interop.InvokeVoidAsync(OpInterop.OptimusInterop, "speedDialItemDiff", _root);
            }

            var listen = _visible && HideOnClickOutside;
            if (listen && !_listening)
            {
                _listening = true;
                _selfRef ??= DotNetObjectReference.Create(this);
                await Interop.InvokeVoidAsync(
                    OpInterop.OptimusInterop, "addOutsideClickListener", _root, null, _selfRef);
            }
            else if (!listen && _listening)
            {
                _listening = false;
                await Interop.InvokeVoidAsync(OpInterop.OptimusInterop, "removeOutsideClickListener", _root);
            }

            if (_focusFirstAction && _visible)
            {
                _focusFirstAction = false;
                await Interop.InvokeVoidAsync(OpInterop.OptimusInterop, "focusNextWithin", _root, ".p-speeddial-action", 1);
            }
        }
        catch (JSDisconnectedException)
        {
        }
    }

    // ------------------------------------------------------------ computed
    private IReadOnlyList<OpMenuItem> Items => Model ?? Array.Empty<OpMenuItem>();

    // Classes e inlineStyles como em speeddialstyle.ts.
    private string RootClass => OpCss.BuildClass(
        $"p-speeddial p-component p-speeddial-{Type}",
        Type != "circle" ? $"p-speeddial-direction-{Direction}" : null,
        _visible ? "p-speeddial-open" : null,
        Disabled ? "p-disabled" : null,
        StyleClass);

    private string? FlexDirection => Direction switch
    {
        "up" => "column-reverse",
        "down" => "column",
        "left" => "row-reverse",
        "right" => "row",
        _ => null
    };

    private string RootStyle
    {
        get
        {
            var style = Direction switch
            {
                "up" or "down" => "align-items: center;",
                "left" or "right" => "justify-content: center;",
                _ => ""
            };
            if (FlexDirection is not null) style += $" flex-direction: {FlexDirection};";
            // Desvio consciente do upstream: nos tipos circulares a lista não ocupa espaço (itens absolutos),
            // mas o gap do root ainda desloca o botão da origem dos itens (arco fora do centro em
            // circle/direction=up e em quarter-circle *-left). Sem gap, o root coincide com o botão.
            if (Type != "linear") style += " gap: 0;";
            return $"{style} {Style}".Trim();
        }
    }

    private string? ListStyle => FlexDirection is null ? null : $"flex-direction: {FlexDirection};";

    private string ButtonClass => OpCss.BuildClass(
        "p-speeddial-button",
        RotateAnimation && string.IsNullOrEmpty(HideIcon) ? "p-speeddial-rotate" : null,
        ButtonStyleClass);

    private string ButtonIconClass
    {
        get
        {
            if (_visible && !string.IsNullOrEmpty(HideIcon)) return HideIcon!;
            return ShowIcon ?? string.Empty;
        }
    }

    private bool UseDefaultIcon => string.IsNullOrEmpty(ButtonIconClass);

    private static readonly RenderFragment DefaultIcon = builder =>
    {
        builder.OpenElement(0, "i");
        builder.AddAttribute(1, "class", "p-icon pi pi-plus p-button-icon");
        builder.CloseElement();
    };

    private string MaskClass => OpCss.BuildClass("p-speeddial-mask p-overlay-mask", MaskStyleClass);

    private static string ItemClass(OpMenuItem item) => OpCss.BuildClass(
        "p-speeddial-item",
        item.Visible ? null : "p-hidden",
        item.StyleClass);

    private SpeedDialItemContext ItemContext(OpMenuItem item, int index)
        => new() { Item = item, Index = index, Click = () => _ = OnItemClick(item) };

    // ------------------------------------------------------------ positioning
    private string ItemStyle(int index)
    {
        var count = Items.Count;
        var delay = (_visible ? index : count - index - 1) * TransitionDelay;
        return $"transition-delay: {delay}ms;{PointStyle(index, count)}";
    }

    // calculatePointStyle do upstream: left/top/right/bottom relativos ao root, com o
    // --item-diff-x/y (speedDialItemDiff) centralizando a ação no botão.
    private string PointStyle(int index, int count)
    {
        if (Type == "linear" || count == 0) return "";

        var radius = Radius > 0 ? Radius : count * 20;

        if (Type == "circle")
        {
            var step = 2 * Math.PI / count;
            return $" left: {Px(radius * Math.Cos(step * index), "x")}; top: {Px(radius * Math.Sin(step * index), "y")};";
        }

        var stepSize = Type == "semi-circle"
            ? Math.PI / Math.Max(count - 1, 1)
            : Math.PI / (2 * Math.Max(count - 1, 1));
        var x = Px(radius * Math.Cos(stepSize * index), "x");
        var y = Px(radius * Math.Sin(stepSize * index), "y");

        if (Type == "semi-circle")
        {
            return Direction switch
            {
                "up" => $" left: {x}; bottom: {y};",
                "down" => $" left: {x}; top: {y};",
                "left" => $" right: {y}; top: {x};",
                "right" => $" left: {y}; top: {x};",
                _ => ""
            };
        }

        return Direction switch
        {
            "up-left" => $" right: {x}; bottom: {y};",
            "up-right" => $" left: {x}; bottom: {y};",
            "down-left" => $" right: {y}; top: {x};",
            "down-right" => $" left: {y}; top: {x};",
            _ => ""
        };
    }

    private static string Px(double value, string axis)
        => string.Create(CultureInfo.InvariantCulture, $"calc({value:0.###}px + var(--item-diff-{axis}, 0px))");

    // ------------------------------------------------------------ interaction
    private async Task Toggle()
    {
        if (Disabled) return;
        await SetVisibleAsync(!_visible);
    }

    private Task OnRootKeydown(KeyboardEventArgs e)
    {
        if (Disabled)
        {
            return Task.CompletedTask;
        }

        if (!_visible)
        {
            return e.Key is "ArrowDown" or "ArrowUp" or "Enter" or " " or "Spacebar"
                ? OpenFromKeyboardAsync()
                : Task.CompletedTask;
        }

        switch (e.Key)
        {
            case "ArrowDown":
            case "ArrowRight":
                return Interop.InvokeVoidAsync(OpInterop.OptimusInterop, "focusNextWithin", _root, ".p-speeddial-action", 1).AsTask();
            case "ArrowUp":
            case "ArrowLeft":
                return Interop.InvokeVoidAsync(OpInterop.OptimusInterop, "focusNextWithin", _root, ".p-speeddial-action", -1).AsTask();
            case "Escape":
                return SetVisibleAsync(false);
            default:
                return Task.CompletedTask;
        }
    }

    private async Task OpenFromKeyboardAsync()
    {
        _focusFirstAction = true;
        await SetVisibleAsync(true);
    }

    private async Task SetVisibleAsync(bool value)
    {
        if (_visible == value) return;
        _visible = value;
        await VisibleChanged.InvokeAsync(value);
        if (value)
        {
            await OnShow.InvokeAsync();
        }
        else
        {
            await OnHide.InvokeAsync();
        }

        StateHasChanged();
    }

    [JSInvokable]
    public Task OnOutsideClick() => InvokeAsync(() => SetVisibleAsync(false));

    private async Task OnButtonClick(MouseEventArgs e)
    {
        await OnClick.InvokeAsync(e);
        await Toggle();
    }

    private async Task OnItemClick(OpMenuItem item)
    {
        if (item.Disabled) return;
        item.Command?.Invoke();
        await SetVisibleAsync(false);
    }

    public override async ValueTask DisposeAsync()
    {
        if (_disposed) return;
        _disposed = true;
        try
        {
            if (_listening)
            {
                await Interop.InvokeVoidAsync(OpInterop.OptimusInterop, "removeOutsideClickListener", _root);
            }
        }
        catch (JSDisconnectedException)
        {
        }

        _selfRef?.Dispose();
        await base.DisposeAsync();
    }
}
