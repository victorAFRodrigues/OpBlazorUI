using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using OpBlazorUI.Base.Components.Common;
using OpBlazorUI.Base.Models;

namespace OpBlazorUI.Base.Components.Steps;

public partial class OpSteps : OpComponentBase
{
    [Parameter] public IReadOnlyList<OpMenuItem> Model { get; set; } = Array.Empty<OpMenuItem>();
    [Parameter] public int ActiveIndex { get; set; }
    [Parameter] public bool Readonly { get; set; }

    [Parameter] public EventCallback<int> ActiveIndexChanged { get; set; }
    [Parameter] public EventCallback<OpMenuItem> OnSelect { get; set; }

    private string RootClass => Class("p-steps p-component", Readonly ? "p-steps-readonly" : null, StyleClass);

    private async Task SelectAsync(int index)
    {
        if (Readonly || index < 0 || index >= Model.Count || Model[index].Disabled)
        {
            return;
        }

        ActiveIndex = index;
        await ActiveIndexChanged.InvokeAsync(index);
        await OnSelect.InvokeAsync(Model[index]);
    }

    private async Task OnKeyDownAsync(KeyboardEventArgs e, int index)
    {
        if (e.Code is not ("Enter" or "Space" or "NumpadEnter"))
        {
            return;
        }

        await SelectAsync(index);
    }
}
