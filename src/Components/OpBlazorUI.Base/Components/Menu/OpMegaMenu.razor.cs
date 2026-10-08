using Microsoft.AspNetCore.Components;
using OpBlazorUI.Base.Components.Common;
using OpBlazorUI.Base.Models;

namespace OpBlazorUI.Base.Components.Menu;

public partial class OpMegaMenu : OpComponentBase
{
    private readonly HashSet<OpMenuItem> _open = new();

    [Parameter] public IReadOnlyList<OpMenuItem> Model { get; set; } = Array.Empty<OpMenuItem>();
    [Parameter] public RenderFragment? StartTemplate { get; set; }
    [Parameter] public RenderFragment? EndTemplate { get; set; }
    [Parameter] public EventCallback<OpMenuItem> OnItemClick { get; set; }

    private string RootClass => Class("p-megamenu p-component", StyleClass);

    private bool IsOpen(OpMenuItem item) => _open.Contains(item);

    private async Task OnSelectAsync(OpMenuItem item, bool hasChildren)
    {
        if (hasChildren)
        {
            if (!_open.Remove(item))
            {
                _open.Clear();
                _open.Add(item);
            }

            return;
        }

        await SelectItemAsync(item);
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
