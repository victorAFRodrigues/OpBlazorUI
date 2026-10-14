using System.Globalization;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using OpBlazorUI.Base.Components.Common;

namespace OpBlazorUI.Base.Components.ImageCompare;

public partial class OpImageCompare : OpComponentBase
{
    [Parameter] public double Value { get; set; } = 50;
    [Parameter] public double Min { get; set; }
    [Parameter] public double Max { get; set; } = 100;
    [Parameter] public int TabIndex { get; set; }
    [Parameter] public string? AriaLabel { get; set; }
    [Parameter] public string? AriaLabelledBy { get; set; }
    [Parameter] public string? Style { get; set; }

    [Parameter] public RenderFragment? Left { get; set; }
    [Parameter] public RenderFragment? Right { get; set; }

    [Parameter] public EventCallback<double> ValueChanged { get; set; }
    [Parameter] public EventCallback<double> OnChange { get; set; }

    private string RootClass => Class("p-imagecompare", StyleClass);

    private string RootStyle =>
        $"--p-imagecompare-scope-x:{Value.ToString("0.####", CultureInfo.InvariantCulture)}%{(string.IsNullOrEmpty(Style) ? "" : ";" + Style)}";

    private async Task OnInput(ChangeEventArgs e)
    {
        if (double.TryParse(e.Value?.ToString(), NumberStyles.Any, CultureInfo.InvariantCulture, out var value))
        {
            Value = Math.Clamp(value, Min, Max);
            await ValueChanged.InvokeAsync(Value);
            await OnChange.InvokeAsync(Value);
        }
    }
}
