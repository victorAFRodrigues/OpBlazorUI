using Microsoft.AspNetCore.Components;
using OpBlazorUI.Base.Components.Common;

namespace OpBlazorUI.Base.Components.Card;

public partial class OpCard : OpComponentBase
{
    [Parameter] public string? Header { get; set; }

    [Parameter] public string? Subheader { get; set; }

    [Parameter] public string? Style { get; set; }

    [Parameter] public RenderFragment? ChildContent { get; set; }

    [Parameter] public RenderFragment? HeaderTemplate { get; set; }

    [Parameter] public RenderFragment? TitleTemplate { get; set; }

    [Parameter] public RenderFragment? SubtitleTemplate { get; set; }

    [Parameter] public RenderFragment? FooterTemplate { get; set; }

    private string RootClass => OpCss.BuildClass("p-card p-component", StyleClass);
}
