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
    /// Estilo do container raiz. Para os tipos circulares o container é
    /// dimensionado para o menor retângulo que comporta o leque de itens e o
    /// botão é encostado na aresta/canto que serve de centro do arco:
    /// circle (2R×2R, botão ao centro), semi-circle (2R×R ou R×2R, botão na
    /// aresta reta) e quarter-circle (R×R, botão no canto).
    /// </summary>
    private string RootStyle
    {
        get
        {
            if (Radius <= 0)
            {
                return Style ?? string.Empty;
            }

            var basis = Type switch
            {
                "circle" => Box(Radius * 2, Radius * 2, Justify.Center, Align.Center),
                "semi-circle" => Direction switch
                {
                    "up" => Box(Radius * 2, Radius, Justify.Center, Align.End),
                    "down" => Box(Radius * 2, Radius, Justify.Center, Align.Start),
                    "left" => Box(Radius, Radius * 2, Justify.End, Align.Center),
                    "right" => Box(Radius, Radius * 2, Justify.Start, Align.Center),
                    _ => Box(Radius * 2, Radius, Justify.Center, Align.End)
                },
                "quarter-circle" => Direction switch
                {
                    "up-left" => Box(Radius, Radius, Justify.End, Align.End),
                    "up-right" => Box(Radius, Radius, Justify.Start, Align.End),
                    "down-left" => Box(Radius, Radius, Justify.End, Align.Start),
                    "down-right" => Box(Radius, Radius, Justify.Start, Align.Start),
                    _ => Box(Radius, Radius, Justify.End, Align.End)
                },
                _ => Style ?? string.Empty
            };

            return string.IsNullOrEmpty(Style)
                ? basis
                : $"{basis} {Style};";
        }
    }

    private static class Justify
    {
        public const string Start = "flex-start";
        public const string Center = "center";
        public const string End = "flex-end";
    }

    private static class Align
    {
        public const string Start = "flex-start";
        public const string Center = "center";
        public const string End = "flex-end";
    }

    private static string Box(int width, int height, string justify, string align) =>
        $"position:relative; width:{width}px; height:{height}px; display:flex; justify-content:{justify}; align-items:{align};";

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
        var style = $"transition-delay:{index * TransitionDelay}ms;";

        if (Type == "linear")
        {
            return style;
        }

        var (x, y) = CalculatePoint(index, Items.Count, Radius, Type, Direction);
        var scale = _visible ? 1 : 0;
        var origin = ItemOrigin;

        style += $"position:absolute; {origin} transform:translate(calc(-50% + {x}px), calc(-50% + {y}px)) scale({scale});";

        return style;
    }

    /// <summary>
    /// Origem polar dos itens: o ponto do container que coincide com o centro
    /// do botão, de onde os itens irradiam.
    /// </summary>
    private string ItemOrigin => Type switch
    {
        "circle" => "left:50%; top:50%;",
        "semi-circle" => Direction switch
        {
            "up" => "left:50%; top:100%;",
            "down" => "left:50%; top:0%;",
            "left" => "left:100%; top:50%;",
            "right" => "left:0%; top:50%;",
            _ => "left:50%; top:100%;"
        },
        "quarter-circle" => Direction switch
        {
            "up-left" => "left:100%; top:100%;",
            "up-right" => "left:0%; top:100%;",
            "down-left" => "left:100%; top:0%;",
            "down-right" => "left:0%; top:0%;",
            _ => "left:100%; top:100%;"
        },
        _ => "left:50%; top:50%;"
    };

    private static (double X, double Y) CalculatePoint(int index, int count, int radius, string type, string direction)
    {
        var r = (double)radius;

        // Com um único item o passo é zero: ele ocupa o início do arco em vez de sobrepor o botão.
        var denom = Math.Max(count - 1, 1);
        var total = Math.Max(count, 1);
        var semiStep = Math.PI / denom;
        var quarterStep = Math.PI / (2 * denom);

        return type switch
        {
            "circle" => Polar(-Math.PI / 2 - 2 * Math.PI / total * index, r),
            "semi-circle" => SemiCircle(index, semiStep, r, direction),
            "quarter-circle" => QuarterCircle(index, quarterStep, r, direction),
            _ => (0, 0)
        };
    }

    private static (double X, double Y) Polar(double theta, double r) => (r * Math.Cos(theta), r * Math.Sin(theta));

    private static (double X, double Y) SemiCircle(int index, double step, double r, string direction)
    {
        // Ângulos medidos a partir da origem polar (centro do botão),
        // no sentido anti-horário, cobrindo exatamente 180°.
        return direction switch
        {
            "up" => Polar(Math.PI + step * index, r),
            "down" => Polar(step * index, r),
            "left" => Polar(3 * Math.PI / 2 - step * index, r),
            "right" => Polar(-Math.PI / 2 + step * index, r),
            _ => Polar(Math.PI + step * index, r)
        };
    }

    private static (double X, double Y) QuarterCircle(int index, double step, double r, string direction)
    {
        // Quarter arcs cover exactly 90°, starting at the axis closest to the
        // button corner and rotating into the target quadrant.
        var t = step * index;
        return direction switch
        {
            "up-left" => Polar(3 * Math.PI / 2 - t, r),
            "up-right" => Polar(-Math.PI / 2 + t, r),
            "down-left" => Polar(Math.PI - t, r),
            "down-right" => Polar(t, r),
            _ => Polar(-Math.PI / 2 + t, r)
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
