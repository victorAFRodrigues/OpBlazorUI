using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using OpBlazorUI.Base.Components.Common;
using OpBlazorUI.Base.Models;

namespace OpBlazorUI.Base.Components.Tabs;

public partial class OpTabMenu : OpComponentBase
{
    private readonly List<ElementReference> _refs = new();

    [Parameter] public IReadOnlyList<OpMenuItem> Model { get; set; } = Array.Empty<OpMenuItem>();
    [Parameter] public int ActiveIndex { get; set; }
    [Parameter] public string? AriaLabel { get; set; }

    [Parameter] public EventCallback<int> ActiveIndexChanged { get; set; }
    [Parameter] public EventCallback<OpMenuItem> OnItemClick { get; set; }

    private string RootClass => Class("p-tabmenu p-component", StyleClass);

    protected override void OnParametersSet()
    {
        while (_refs.Count < Model.Count)
        {
            _refs.Add(default);
        }
    }

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

    private async Task OnKeyDownAsync(KeyboardEventArgs e, int index)
    {
        switch (e.Code)
        {
            case "ArrowRight":
                await MoveFocusAsync(index, 1);
                break;
            case "ArrowLeft":
                await MoveFocusAsync(index, -1);
                break;
            case "Home":
                await MoveFocusToAsync(FindEnabled(0, 1));
                break;
            case "End":
                await MoveFocusToAsync(FindEnabled(Model.Count - 1, -1));
                break;
            case "Enter":
            case "Space":
            case "NumpadEnter":
                await SelectAsync(new MouseEventArgs(), index);
                await FocusAsync(index);
                break;
        }
    }

    private async Task MoveFocusAsync(int from, int step) => await MoveFocusToAsync(FindEnabled(from + step, step));

    private async Task MoveFocusToAsync(int target)
    {
        if (target < 0)
        {
            return;
        }

        await SelectAsync(new MouseEventArgs(), target);
        await FocusAsync(target);
    }

    private async Task FocusAsync(int index)
    {
        if (index < 0 || index >= _refs.Count)
        {
            return;
        }

        try
        {
            await _refs[index].FocusAsync();
        }
        catch
        {
            // elemento já removido
        }
    }

    private int FindEnabled(int from, int step)
    {
        var count = Model.Count;
        if (count == 0)
        {
            return -1;
        }

        var i = ((from % count) + count) % count;
        for (var n = 0; n < count; n++)
        {
            if (!Model[i].Disabled)
            {
                return i;
            }

            i = (i + step + count) % count;
        }

        return -1;
    }
}
