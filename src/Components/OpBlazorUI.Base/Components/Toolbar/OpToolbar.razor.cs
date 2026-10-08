using Microsoft.AspNetCore.Components;
using OpBlazorUI.Base.Components.Common;

namespace OpBlazorUI.Base.Components.Toolbar;

public partial class OpToolbar : OpComponentBase
{
    [Parameter] public string? AriaLabel { get; set; }
    [Parameter] public RenderFragment? StartTemplate { get; set; }
    [Parameter] public RenderFragment? CenterTemplate { get; set; }
    [Parameter] public RenderFragment? EndTemplate { get; set; }
    [Parameter] public RenderFragment? ChildContent { get; set; }

    private string RootClass => Class("p-toolbar p-component", StyleClass);
}
