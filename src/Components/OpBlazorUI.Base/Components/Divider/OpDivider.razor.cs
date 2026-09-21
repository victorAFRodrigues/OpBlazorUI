using Microsoft.AspNetCore.Components;

namespace OpBlazorUI.Base.Components.Divider;

public partial class OpDivider : ComponentBase
{
    [Parameter] public string Layout { get; set; } = "horizontal";

    [Parameter] public string Type { get; set; } = "solid";

    [Parameter] public string? Align { get; set; }

    [Parameter] public string? StyleClass { get; set; }

    [Parameter] public RenderFragment? ChildContent { get; set; }

    [Parameter(CaptureUnmatchedValues = true)]
    public Dictionary<string, object>? AdditionalAttributes { get; set; }

    private string RootClass => OpCss.BuildClass(
        "p-divider p-component",
        Layout == "vertical" ? "p-divider-vertical" : "p-divider-horizontal",
        Type switch
        {
            "dashed" => "p-divider-dashed",
            "dotted" => "p-divider-dotted",
            _ => "p-divider-solid"
        },
        !string.IsNullOrEmpty(Align) ? $"p-divider-{Align}" : null,
        StyleClass);
}
