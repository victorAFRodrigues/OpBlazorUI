using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using OpBlazorUI.Base.Components.Common;
using OpBlazorUI.Base.Models;

namespace OpBlazorUI.Base.Components.Steps;

public partial class OpSteps : OpComponentBase
{
    [Inject] private NavigationManager Nav { get; set; } = default!;

    private readonly List<ElementReference> _refs = new();

    [Parameter] public IReadOnlyList<OpMenuItem> Model { get; set; } = Array.Empty<OpMenuItem>();
    [Parameter] public int ActiveIndex { get; set; }
    [Parameter] public bool Readonly { get; set; }

    [Parameter] public EventCallback<int> ActiveIndexChanged { get; set; }
    [Parameter] public EventCallback<OpMenuItem> OnSelect { get; set; }

    private string RootClass => Class("p-steps p-component", Readonly ? "p-steps-readonly" : null, StyleClass);

    protected override void OnParametersSet()
    {
        while (_refs.Count < Model.Count)
        {
            _refs.Add(default);
        }
    }

    private async Task SelectAsync(int index)
    {
        if (Readonly || index < 0 || index >= Model.Count || Model[index].Disabled)
        {
            return;
        }

        var item = Model[index];
        ActiveIndex = index;

        item.Command?.Invoke();
        if (!string.IsNullOrEmpty(item.Url))
        {
            Nav.NavigateTo(item.Url);
        }

        await ActiveIndexChanged.InvokeAsync(index);
        await OnSelect.InvokeAsync(item);
        StateHasChanged();
    }

    // Setas/Home/End movem o foco (e selecionam) entre as etapas; Enter/Space ficam com o botão nativo.
    private async Task OnKeyDownAsync(KeyboardEventArgs e, int index)
    {
        var target = e.Code switch
        {
            "ArrowRight" or "ArrowDown" => FindEnabled(index + 1, 1),
            "ArrowLeft" or "ArrowUp" => FindEnabled(index - 1, -1),
            "Home" => FindEnabled(0, 1),
            "End" => FindEnabled(Model.Count - 1, -1),
            _ => -2
        };

        if (target == -2)
        {
            return;
        }

        if (target < 0)
        {
            return;
        }

        await SelectAsync(target);
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
