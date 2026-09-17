using Microsoft.AspNetCore.Components;

namespace OpBlazorUI.Base.Components.IconField;

public partial class OpIconField : ComponentBase
{
    [Parameter] public string IconPosition { get; set; } = "left";
    [Parameter] public string? StyleClass { get; set; }

    [Parameter] public RenderFragment? ChildContent { get; set; }

    [Parameter(CaptureUnmatchedValues = true)]
    public Dictionary<string, object>? AdditionalAttributes { get; set; }

    private string RootClass => BuildClass(
        "p-iconfield",
        IconPosition == "left" ? "p-iconfield-left" : null,
        IconPosition == "right" ? "p-iconfield-right" : null,
        StyleClass);

    private static string BuildClass(params string?[] classes)
        => string.Join(' ', classes.Where(c => !string.IsNullOrWhiteSpace(c)));
}