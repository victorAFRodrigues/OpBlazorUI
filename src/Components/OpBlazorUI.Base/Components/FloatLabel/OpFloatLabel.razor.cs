using Microsoft.AspNetCore.Components;

namespace OpBlazorUI.Base.Components.FloatLabel;

public partial class OpFloatLabel : ComponentBase
{
    [Parameter] public string Variant { get; set; } = "over";
    [Parameter] public string? StyleClass { get; set; }

    [Parameter] public RenderFragment? ChildContent { get; set; }

    [Parameter(CaptureUnmatchedValues = true)]
    public Dictionary<string, object>? AdditionalAttributes { get; set; }

    private string RootClass => BuildClass(
        "p-floatlabel",
        Variant == "over" ? "p-floatlabel-over" : null,
        Variant == "on" ? "p-floatlabel-on" : null,
        Variant == "in" ? "p-floatlabel-in" : null,
        StyleClass);

    private static string BuildClass(params string?[] classes)
        => string.Join(' ', classes.Where(c => !string.IsNullOrWhiteSpace(c)));
}