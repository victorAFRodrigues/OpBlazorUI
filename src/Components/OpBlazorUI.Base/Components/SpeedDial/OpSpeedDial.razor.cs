using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using OpBlazorUI.Base.Models;

namespace OpBlazorUI.Base.Components.SpeedDial;

public sealed class SpeedDialButtonContext
{
    public Action? Toggle { get; init; }
}

public partial class OpSpeedDial : ComponentBase
{
    private string _id = "";
    private bool _visible;

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
    [Parameter] public string? ShowIcon { get; set; } = "pi pi-plus";
    [Parameter] public string? HideIcon { get; set; }
    [Parameter] public bool RotateAnimation { get; set; } = true;
    [Parameter] public string? AriaLabel { get; set; }
    [Parameter] public string? AriaLabelledBy { get; set; }
    [Parameter] public string? StyleClass { get; set; }
    [Parameter] public string? Style { get; set; }
    [Parameter] public string? ButtonStyleClass { get; set; }
    [Parameter] public string? ButtonStyle { get; set; }
    [Parameter] public string? MaskStyleClass { get; set; }
    [Parameter] public string? MaskStyle { get; set; }
    [Parameter] public bool Tooltip { get; set; }
    [Parameter] public string TooltipPosition { get; set; } = "right";
    [Parameter] public string? ButtonSeverity { get; set; }
    [Parameter] public bool ButtonRounded { get; set; }
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
    [Parameter] public RenderFragment<OpMenuItem>? ItemTemplate { get; set; }
    [Parameter] public RenderFragment? IconTemplate { get; set; }

    [Parameter(CaptureUnmatchedValues = true)]
    public Dictionary<string, object>? AdditionalAttributes { get; set; }

    // ------------------------------------------------------------ lifecycle
    protected override void OnInitialized()
    {
        _id = $"op-speeddial-{Guid.NewGuid():N}";
        _visible = Visible;
    }

    protected override void OnParametersSet()
    {
        _visible = Visible;
    }

    // ------------------------------------------------------------ computed
    private IReadOnlyList<OpMenuItem> Items => Model ?? Array.Empty<OpMenuItem>();

    /// <summary>
    /// Estilo do container raiz. Para tipos circulares (circle, semi-circle,
    /// quarter-circle), o container é dimensionado para 2×<see cref="Radius"/>
    /// e o botão centralizado, de modo que os itens irradiem do centro do botão.
    /// </summary>
    private string RootStyle
    {
        get
        {
            var circular = Type is "circle" or "semi-circle" or "quarter-circle";
            if (!circular || Radius <= 0)
            {
                return Style ?? string.Empty;
            }

            var diameter = Radius * 2;
            var center = "display:flex; align-items:center; justify-content:center;";
            var size = $"width:{diameter}px; height:{diameter}px;";
            var basis = $"position:relative; {size} {center}";

            return string.IsNullOrEmpty(Style)
                ? basis
                : $"{basis} {Style};";
        }
    }

    private string RootClass => BuildClass(
        "p-speeddial p-component",
        _visible ? "p-speeddial-open" : null,
        Type != "linear" ? $"p-speeddial-{Type}" : null,
        $"p-speeddial-{Direction}",
        StyleClass);

    private string ListStyle
    {
        get
        {
            if (Type == "linear")
            {
                return Direction switch
                {
                    "up" => "position:absolute; bottom:calc(100% + 0.5rem); left:50%; transform:translateX(-50%); flex-direction:column;",
                    "down" => "position:absolute; top:calc(100% + 0.5rem); left:50%; transform:translateX(-50%); flex-direction:column;",
                    "left" => "position:absolute; right:calc(100% + 0.5rem); top:50%; transform:translateY(-50%); flex-direction:row-reverse;",
                    "right" => "position:absolute; left:calc(100% + 0.5rem); top:50%; transform:translateY(-50%); flex-direction:row;",
                    _ => "position:absolute; bottom:calc(100% + 0.5rem); left:50%; transform:translateX(-50%); flex-direction:column;"
                };
            }

            return "position:absolute; inset:0;";
        }
    }

    private string ButtonClass => BuildClass(
        "p-speeddial-button",
        _visible && RotateAnimation && string.IsNullOrEmpty(HideIcon) ? "p-speeddial-rotate" : null,
        ButtonStyleClass);

    private string ButtonIconClass
    {
        get
        {
            if (_visible && !string.IsNullOrEmpty(HideIcon)) return HideIcon!;
            return ShowIcon ?? string.Empty;
        }
    }

    // ------------------------------------------------------------ positioning
    private string ItemStyle(int index)
    {
        var count = Items.Count;
        var delay = index * TransitionDelay;
        var style = $"transition-delay:{delay}ms;";

        if (Type != "linear")
        {
            var (x, y) = CalculatePoint(index, count, Radius, Type, Direction);
            var scale = _visible ? 1 : 0;
            style += $"position:absolute; left:50%; top:50%; transform:translate(calc(-50% + {x}px), calc(-50% + {y}px)) scale({scale});";
        }

        return style;
    }

    private static (double X, double Y) CalculatePoint(int index, int count, int radius, string type, string direction)
    {
        var r = (double)radius;
        if (count <= 1) return (0, 0);

        var denom = count - 1;

        return type switch
        {
            "circle" => Circle(index, count, r),
            "semi-circle" => SemiCircle(index, denom, r, direction),
            "quarter-circle" => QuarterCircle(index, denom, r, direction),
            _ => (0, 0)
        };
    }

    private static (double X, double Y) Circle(int index, int count, double r)
    {
        var theta = -Math.PI / 2 - 2 * Math.PI / count * index;
        return (r * Math.Cos(theta), r * Math.Sin(theta));
    }

    private static (double X, double Y) SemiCircle(int index, int denom, double r, string direction)
    {
        var t = Math.PI * index / denom;
        return direction switch
        {
            "up" => (r * Math.Cos(t), -r * Math.Sin(t)),
            "down" => (r * Math.Cos(t), r * Math.Sin(t)),
            "left" => (-r * Math.Sin(t), -r * Math.Cos(t)),
            "right" => (r * Math.Sin(t), -r * Math.Cos(t)),
            _ => (r * Math.Cos(t), -r * Math.Sin(t))
        };
    }

    private static (double X, double Y) QuarterCircle(int index, int denom, double r, string direction)
    {
        var t = Math.PI / 2 * index / denom;
        return direction switch
        {
            "up-left" => (-r * Math.Sin(t), -r * Math.Cos(t)),
            "up-right" => (r * Math.Cos(t), -r * Math.Sin(t)),
            "down-left" => (-r * Math.Cos(t), r * Math.Sin(t)),
            "down-right" => (r * Math.Sin(t), r * Math.Cos(t)),
            _ => (r * Math.Cos(t), -r * Math.Sin(t))
        };
    }

    // ------------------------------------------------------------ interaction
    private async Task Toggle()
    {
        if (Disabled) return;
        await SetVisibleAsync(!_visible);
    }

    private async Task SetVisibleAsync(bool value)
    {
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

    private async Task OnButtonClick(MouseEventArgs e)
    {
        await OnClick.InvokeAsync(e);
        await Toggle();
    }

    private async Task OnItemClick(OpMenuItem item)
    {
        if (item.Disabled) return;

        if (item.Url is not null)
        {
            // navegação/link
        }

        item.Command?.Invoke();
        await SetVisibleAsync(false);
    }

    private static string BuildClass(params string?[] classes)
        => string.Join(' ', classes.Where(c => !string.IsNullOrWhiteSpace(c)));
}
