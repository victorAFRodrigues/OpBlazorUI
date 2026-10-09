using Microsoft.AspNetCore.Components;
using OpBlazorUI.Base.Components.Common;

namespace OpBlazorUI.Base.Components.Panel;

public partial class OpPanel : OpComponentBase
{
    private readonly string _uid = "opn_" + Guid.NewGuid().ToString("N")[..8];

    [Parameter] public string? Header { get; set; }
    [Parameter] public bool Toggleable { get; set; }
    [Parameter] public bool Collapsed { get; set; }
    [Parameter] public EventCallback<bool> CollapsedChanged { get; set; }

    /// <summary>Elemento que dispara o toggle: <c>icon</c> (padrão) ou <c>header</c>.</summary>
    [Parameter] public string Toggler { get; set; } = "icon";

    /// <summary>Posição dos ícones de cabeçalho: <c>start</c>, <c>end</c> (padrão) ou <c>center</c>.</summary>
    [Parameter] public string IconPos { get; set; } = "end";

    [Parameter] public bool ShowHeader { get; set; } = true;

    [Parameter] public RenderFragment? HeaderTemplate { get; set; }
    [Parameter] public RenderFragment? IconsTemplate { get; set; }
    [Parameter] public RenderFragment? ContentTemplate { get; set; }
    [Parameter] public RenderFragment? FooterTemplate { get; set; }
    [Parameter] public RenderFragment? ChildContent { get; set; }

    [Parameter] public EventCallback<OpPanelToggleEventArgs> OnBeforeToggle { get; set; }
    [Parameter] public EventCallback<OpPanelToggleEventArgs> OnAfterToggle { get; set; }

    private string RootId => Id ?? _uid;
    private string HeaderBarId => RootId + "_titlebar";
    private string HeaderId => RootId + "_header";
    private string ContentId => RootId + "_content";

    private bool IsCollapsed => Toggleable && Collapsed;
    private string ExpandedString => (!IsCollapsed).ToString().ToLowerInvariant();
    private bool StopHeaderPropagation => Toggleable && Toggler == "header";

    private string RootClass => Class(
        "p-panel p-component",
        Toggleable ? "p-panel-toggleable" : null,
        Toggleable && !Collapsed ? "p-panel-expanded" : null,
        Toggleable && Collapsed ? "p-panel-collapsed" : null,
        StyleClass);

    private string ContentContainerClass => "p-panel-content-container";

    private string IconsPositionClass => $"p-panel-icons-{IconPos}";

    private string ToggleButtonClass => "p-panel-toggle-button p-button p-button-text p-button-secondary p-button-icon-only p-button-rounded";

    private string ToggleIconClass => Class("pi", IsCollapsed ? "pi-plus" : "pi-minus");

    private async Task ToggleAsync()
    {
        if (!Toggleable)
        {
            return;
        }

        var next = !Collapsed;
        await OnBeforeToggle.InvokeAsync(new OpPanelToggleEventArgs(next));
        Collapsed = next;
        await CollapsedChanged.InvokeAsync(next);
        await OnAfterToggle.InvokeAsync(new OpPanelToggleEventArgs(next));
    }

    private async Task OnHeaderClickAsync()
    {
        if (Toggleable && Toggler == "header")
        {
            await ToggleAsync();
        }
    }
}

public sealed record OpPanelToggleEventArgs(bool Collapsed);
