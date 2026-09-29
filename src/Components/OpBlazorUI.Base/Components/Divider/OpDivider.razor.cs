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
        AlignClass,
        StyleClass);

    // Mesmas regras do upstream: horizontal aceita left/center/right (padrão left), vertical
    // aceita top/center/bottom (padrão center); valores de outro layout são ignorados.
    private string AlignClass => Layout == "vertical"
        ? Align switch { "top" => "p-divider-top", "bottom" => "p-divider-bottom", _ => "p-divider-center" }
        : Align switch { "center" => "p-divider-center", "right" => "p-divider-right", _ => "p-divider-left" };

    // Como o inlineStyles do upstream: o tema não alinha o conteúdo pelas classes p-divider-{align}.
    private string? RootStyle => Layout == "vertical"
        ? Align switch
        {
            "top" => "align-items: flex-start;",
            "bottom" => "align-items: flex-end;",
            _ => "align-items: center;"
        }
        : Align switch
        {
            "left" => "justify-content: flex-start;",
            "right" => "justify-content: flex-end;",
            _ => "justify-content: center;"
        };
}
