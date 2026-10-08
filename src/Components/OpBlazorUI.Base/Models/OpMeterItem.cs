namespace OpBlazorUI.Base.Models;

/// <summary>Item de um <c>OpMeterGroup</c>.</summary>
public sealed class OpMeterItem
{
    public string? Label { get; set; }
    public double Value { get; set; }
    public string? Color { get; set; }
    public string? Icon { get; set; }
}
