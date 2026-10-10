using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using OpBlazorUI.Base.Components.Common;

namespace OpBlazorUI.Base.Components.Accordion;

public partial class OpAccordionPanel : OpComponentBase
{
    private readonly string _uid = "opacc_" + Guid.NewGuid().ToString("N")[..8];
    private ElementReference _headerRef;

    [CascadingParameter] internal OpAccordion? Parent { get; set; }

    [Parameter] public string? Header { get; set; }
    [Parameter] public bool Disabled { get; set; }
    [Parameter] public RenderFragment? HeaderTemplate { get; set; }
    [Parameter] public RenderFragment? ChildContent { get; set; }

    internal int Index { get; set; }

    private bool Active => Parent?.IsActive(Index) ?? false;

    private string HeaderId => _uid + "_header";
    private string ContentId => _uid + "_content";

    private string PanelClass => Class("p-accordionpanel", Active ? "p-accordionpanel-active" : null, StyleClass);

    private string ToggleIconClass => Class("p-accordionheader-toggle-icon", Active ? "pi pi-chevron-up" : "pi pi-chevron-down");

    protected override void OnInitialized() => Parent?.Register(this);

    public override ValueTask DisposeAsync()
    {
        Parent?.Unregister(this);
        return base.DisposeAsync();
    }

    private async Task ToggleAsync()
    {
        if (Disabled || Parent is null)
        {
            return;
        }

        await Parent.ToggleAsync(Index);
    }

    internal async Task FocusAsync()
    {
        try
        {
            await _headerRef.FocusAsync();
        }
        catch
        {
            // elemento já removido
        }
    }

    private async Task OnKeyDownAsync(KeyboardEventArgs e)
    {
        switch (e.Code)
        {
            case "Enter":
            case "Space":
            case "NumpadEnter":
                await ToggleAsync();
                break;

            case "ArrowUp":
            case "ArrowDown":
            case "Home":
            case "End":
                Parent?.MoveFocus(Index, e.Code);
                break;
        }
    }
}
