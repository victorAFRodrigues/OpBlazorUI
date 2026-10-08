using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using OpBlazorUI.Base.Components.Common;
using OpBlazorUI.Base.Models;

namespace OpBlazorUI.Base.Components.Tabs;

public partial class OpTabMenu : OpComponentBase
{
    [Parameter] public IReadOnlyList<OpMenuItem> Model { get; set; } = Array.Empty<OpMenuItem>();
    [Parameter] public int ActiveIndex { get; set; }
    [Parameter] public string? AriaLabel { get; set; }

    [Parameter] public EventCallback<int> ActiveIndexChanged { get; set; }
    [Parameter] public EventCallback<OpMenuItem> OnItemClick { get; set; }

    private string RootClass => Class("p-tabmenu p-component", StyleClass);

    private async Task SelectAsync(MouseEventArgs e, int index)
    {
        if (index < 0 || index >= Model.Count || Model[index].Disabled)
        {
            return;
        }

        ActiveIndex = index;
        await ActiveIndexChanged.InvokeAsync(index);

        var item = Model[index];
        item.Command?.Invoke();
        await OnItemClick.InvokeAsync(item);
    }
}
