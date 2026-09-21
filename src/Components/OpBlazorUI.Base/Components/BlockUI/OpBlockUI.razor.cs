using Microsoft.AspNetCore.Components;

namespace OpBlazorUI.Base.Components.BlockUI;

public partial class OpBlockUI : ComponentBase
{
    [Parameter] public bool Blocked { get; set; }

    [Parameter] public bool AutoZIndex { get; set; } = true;

    [Parameter] public int BaseZIndex { get; set; }

    [Parameter] public string? StyleClass { get; set; }

    [Parameter] public string? Style { get; set; }

    [Parameter] public RenderFragment? ChildContent { get; set; }

    [Parameter] public RenderFragment? ContentTemplate { get; set; }

    [Parameter(CaptureUnmatchedValues = true)]
    public Dictionary<string, object>? AdditionalAttributes { get; set; }

    private string RootClass => OpCss.BuildClass("p-blockui p-component", StyleClass);

    private string MaskClass => OpCss.BuildClass(
        "p-blockui-mask",
        "p-overlay-mask",
        "p-overlay-mask-enter-active");
}
