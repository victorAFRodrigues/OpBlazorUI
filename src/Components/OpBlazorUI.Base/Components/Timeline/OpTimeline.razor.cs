using Microsoft.AspNetCore.Components;
using OpBlazorUI.Base.Components.Common;

namespace OpBlazorUI.Base.Components.Timeline;

public partial class OpTimeline<TItem> : OpComponentBase
{
    [Parameter] public IReadOnlyList<TItem> Items { get; set; } = Array.Empty<TItem>();

    /// <summary>Layout: <c>vertical</c> ou <c>horizontal</c>.</summary>
    [Parameter] public string Layout { get; set; } = "vertical";

    /// <summary>Alinhamento: <c>left</c>/<c>right</c> (vertical), <c>top</c>/<c>bottom</c> (horizontal) ou <c>alternate</c>.</summary>
    [Parameter] public string Align { get; set; } = "left";

    [Parameter] public RenderFragment<TItem>? ContentTemplate { get; set; }
    [Parameter] public RenderFragment<TItem>? OppositeTemplate { get; set; }
    [Parameter] public RenderFragment<TItem>? MarkerTemplate { get; set; }

    private string RootClass => Class(
        "p-timeline p-component",
        $"p-timeline-{Layout}",
        Align != "top" ? $"p-timeline-{Align}" : null,
        StyleClass);

    private string DataP => $"{Layout} {Align}";
}
