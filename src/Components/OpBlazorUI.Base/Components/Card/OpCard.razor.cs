using Microsoft.AspNetCore.Components;

namespace OpBlazorUI.Base.Components.Card;

public partial class OpCard : ComponentBase
{
    [Parameter] public string? Header { get; set; }

    [Parameter] public string? Subheader { get; set; }

    [Parameter] public string? Style { get; set; }

    [Parameter] public string? StyleClass { get; set; }

    [Parameter] public RenderFragment? ChildContent { get; set; }

    [Parameter] public RenderFragment? HeaderTemplate { get; set; }

    [Parameter] public RenderFragment? TitleTemplate { get; set; }

    [Parameter] public RenderFragment? SubtitleTemplate { get; set; }

    [Parameter] public RenderFragment? FooterTemplate { get; set; }

    [Parameter(CaptureUnmatchedValues = true)]
    public Dictionary<string, object>? AdditionalAttributes { get; set; }

    private bool HasCaption =>
        TitleTemplate != null || SubtitleTemplate != null ||
        !string.IsNullOrEmpty(Header) || !string.IsNullOrEmpty(Subheader);

    private string RootClass => OpCss.BuildClass("p-card p-component", StyleClass);
}
