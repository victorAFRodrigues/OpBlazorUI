using Microsoft.AspNetCore.Components;
using OpBlazorUI.Base.Components.Common;
using OpBlazorUI.Base.Models;

namespace OpBlazorUI.Base.Components.Breadcrumb;

public partial class OpBreadcrumb : OpComponentBase
{
    [Parameter] public IReadOnlyList<OpMenuItem> Model { get; set; } = Array.Empty<OpMenuItem>();
    [Parameter] public OpMenuItem? Home { get; set; }
    [Parameter] public string SeparatorIcon { get; set; } = "pi pi-chevron-right";
    [Parameter] public string? AriaLabel { get; set; }

    [Parameter] public RenderFragment<OpMenuItem>? HomeTemplate { get; set; }
    [Parameter] public RenderFragment<OpMenuItem>? ItemTemplate { get; set; }

    private string RootClass => Class("p-breadcrumb p-component", StyleClass);

    private string SeparatorIconClass => Class("p-breadcrumb-separator-icon", SeparatorIcon);
}
