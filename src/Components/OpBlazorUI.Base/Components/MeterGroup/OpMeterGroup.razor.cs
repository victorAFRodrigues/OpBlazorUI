using System.Globalization;
using Microsoft.AspNetCore.Components;
using OpBlazorUI.Base.Components.Common;
using OpBlazorUI.Base.Models;

namespace OpBlazorUI.Base.Components.MeterGroup;

public partial class OpMeterGroup : OpComponentBase
{
    [Parameter] public IReadOnlyList<OpMeterItem> Items { get; set; } = Array.Empty<OpMeterItem>();

    [Parameter] public double Min { get; set; }
    [Parameter] public double Max { get; set; } = 100;

    /// <summary>Layout do componente: <c>horizontal</c> ou <c>vertical</c>.</summary>
    [Parameter] public string Orientation { get; set; } = "horizontal";

    /// <summary>Posição dos rótulos: <c>start</c> ou <c>end</c>.</summary>
    [Parameter] public string LabelPosition { get; set; } = "end";

    /// <summary>Orientação dos rótulos: <c>horizontal</c> ou <c>vertical</c>.</summary>
    [Parameter] public string LabelOrientation { get; set; } = "horizontal";

    [Parameter] public RenderFragment<OpMeterContext>? MeterTemplate { get; set; }
    [Parameter] public RenderFragment<OpMeterItem>? IconTemplate { get; set; }
    [Parameter] public RenderFragment<OpMeterGroupLabelContext>? LabelTemplate { get; set; }
    [Parameter] public RenderFragment<OpMeterGroupLabelContext>? StartTemplate { get; set; }
    [Parameter] public RenderFragment<OpMeterGroupLabelContext>? EndTemplate { get; set; }

    private string RootClass => Class(
        "p-metergroup p-component",
        $"p-metergroup-{Orientation}",
        LabelPosition == "start" ? "p-metergroup-label-start" : "p-metergroup-label-end",
        StyleClass);

    private string LabelListClass => Class(
        "p-metergroup-label-list",
        $"p-metergroup-label-list-{LabelOrientation}");

    private OpMeterGroupLabelContext LabelContext() =>
        new(Items, TotalPercent, Items.Select(i => Percent(i.Value)).ToList());

    internal int Percent(double meter)
    {
        if (Math.Abs(Max - Min) < double.Epsilon)
        {
            return 100;
        }

        var percent = (meter - Min) / (Max - Min) * 100;
        return (int)Math.Round(Math.Max(0, Math.Min(100, percent)));
    }

    private string PercentValue(double meter) => Percent(meter).ToString(CultureInfo.InvariantCulture) + "%";

    private int TotalPercent => Percent(Items.Sum(i => i.Value));

    private string MeterStyle(OpMeterItem item)
    {
        var parts = new List<string>();
        if (!string.IsNullOrEmpty(item.Color)) parts.Add($"background-color:{item.Color}");
        if (Orientation == "horizontal") parts.Add($"width:{PercentValue(item.Value)}");
        else parts.Add($"height:{PercentValue(item.Value)}");
        return string.Join(";", parts);
    }

    private string? MarkerStyle(OpMeterItem item) =>
        string.IsNullOrEmpty(item.Color) ? null : $"background-color:{item.Color}";

    private string? IconStyle(OpMeterItem item) =>
        string.IsNullOrEmpty(item.Color) ? null : $"color:{item.Color}";
}

public sealed record OpMeterContext(OpMeterItem Item, int Index, string Orientation, int Percent);

public sealed record OpMeterGroupLabelContext(IReadOnlyList<OpMeterItem> Items, int TotalPercent, IReadOnlyList<int> Percentages);
