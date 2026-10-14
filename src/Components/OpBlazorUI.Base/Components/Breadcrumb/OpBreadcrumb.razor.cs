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

    [Parameter] public EventCallback<OpMenuItem> OnItemClick { get; set; }

    private string RootClass => Class("p-breadcrumb p-component", StyleClass);

    private string SeparatorIconClass => Class("p-breadcrumb-separator-icon", SeparatorIcon);

    private IReadOnlyList<OpMenuItem> VisibleModel => Model.Where(i => i.Visible).ToList();

    private string? TargetRel(OpMenuItem item) =>
        item.Target == "_blank" ? "noopener noreferrer" : null;

    private async Task InvokeCommand(OpMenuItem item)
    {
        if (item.Disabled)
        {
            return;
        }

        item.Command?.Invoke();
        await OnItemClick.InvokeAsync(item);
    }
}
