using Microsoft.AspNetCore.Components;
using OpBlazorUI.Base.Components.Common;

namespace OpBlazorUI.Base.Components.Fieldset;

public partial class OpFieldset : OpComponentBase
{
    private readonly string _uid = "opf_" + Guid.NewGuid().ToString("N")[..8];

    [Parameter] public string? Legend { get; set; }
    [Parameter] public bool Toggleable { get; set; }
    [Parameter] public bool Collapsed { get; set; }
    [Parameter] public string? Style { get; set; }

    [Parameter] public RenderFragment? HeaderTemplate { get; set; }
    [Parameter] public RenderFragment? ExpandIcon { get; set; }
    [Parameter] public RenderFragment? CollapseIcon { get; set; }
    [Parameter] public RenderFragment? ChildContent { get; set; }

    [Parameter] public EventCallback<bool> CollapsedChanged { get; set; }
    [Parameter] public EventCallback<bool> OnToggle { get; set; }

    private string RootId => Id ?? _uid;
    private string HeaderId => RootId + "_header";
    private string ContentId => RootId + "_content";

    private string RootClass => Class(
        "p-fieldset p-component",
        Toggleable ? "p-fieldset-toggleable" : null,
        StyleClass);

    private string ContentContainerClass => Class(
        "p-fieldset-content-container",
        Collapsed ? "p-fieldset-collapsed" : null);

    private string ToggleIconClass => Class(
        "p-fieldset-toggle-icon",
        Collapsed ? "pi pi-plus" : "pi pi-minus");

    private async Task ToggleAsync()
    {
        Collapsed = !Collapsed;
        await CollapsedChanged.InvokeAsync(Collapsed);
        await OnToggle.InvokeAsync(Collapsed);
    }
}
