using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using OpBlazorUI.Base.Components.Common;

namespace OpBlazorUI.Base.Components.Accordion;

public partial class OpAccordionPanel : OpComponentBase, IDisposable
{
    private readonly string _uid = "opacc_" + Guid.NewGuid().ToString("N")[..8];

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

    public void Dispose() => Parent?.Unregister(this);

    private async Task ToggleAsync()
    {
        if (Disabled || Parent is null)
        {
            return;
        }

        await Parent.ToggleAsync(Index);
    }

    private Task OnKeyDownAsync(KeyboardEventArgs e)
        => e.Code is "Enter" or "Space" or "NumpadEnter" ? ToggleAsync() : Task.CompletedTask;
}
