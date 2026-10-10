using Microsoft.AspNetCore.Components;
using OpBlazorUI.Base.Components.Common;
using OpBlazorUI.Base.Models;

namespace OpBlazorUI.Base.Components.Menu;

public partial class OpPanelMenu : OpComponentBase
{
    private readonly HashSet<OpMenuItem> _open = new();
    private readonly string _uid = "oppm-" + Guid.NewGuid().ToString("N")[..8];
    private ElementReference _rootRef;

    [Parameter] public IReadOnlyList<OpMenuItem> Model { get; set; } = Array.Empty<OpMenuItem>();
    [Parameter] public bool Multiple { get; set; } = true;
    [Parameter] public EventCallback<OpMenuItem> OnItemClick { get; set; }

    private string RootClass => Class("p-panelmenu p-component", StyleClass);

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        await Interop.InvokeVoidAsync(
            OpInterop.MenuInterop, "initMenuFocusKeyboard", _rootRef,
            "a.p-panelmenu-header-link, a.p-panelmenu-item-link");
    }

    private bool IsOpen(OpMenuItem panel) => _open.Contains(panel);

    private Task ToggleAsync(OpMenuItem panel)
    {
        if (_open.Contains(panel))
        {
            _open.Remove(panel);
        }
        else
        {
            if (!Multiple)
            {
                _open.Clear();
            }

            _open.Add(panel);
        }

        return Task.CompletedTask;
    }

    private async Task SelectItemAsync(OpMenuItem item)
    {
        if (item.Disabled)
        {
            return;
        }

        item.Command?.Invoke();
        await OnItemClick.InvokeAsync(item);
    }
}
