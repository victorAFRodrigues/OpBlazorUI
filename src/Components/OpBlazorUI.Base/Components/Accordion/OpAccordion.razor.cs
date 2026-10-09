using Microsoft.AspNetCore.Components;
using OpBlazorUI.Base.Components.Common;

namespace OpBlazorUI.Base.Components.Accordion;

public partial class OpAccordion : OpComponentBase
{
    private readonly List<OpAccordionPanel> _panels = new();

    [Parameter] public bool Multiple { get; set; }
    [Parameter] public IReadOnlyList<int>? ActiveIndexes { get; set; }
    [Parameter] public RenderFragment? ChildContent { get; set; }

    [Parameter] public EventCallback<IReadOnlyList<int>> ActiveIndexesChanged { get; set; }

    private IReadOnlyList<int> ActiveIndexesList => ActiveIndexes ?? Array.Empty<int>();

    private string RootClass => Class("p-accordion p-component", StyleClass);

    internal void Register(OpAccordionPanel panel)
    {
        if (_panels.Contains(panel))
        {
            return;
        }

        panel.Index = _panels.Count;
        _panels.Add(panel);
        _ = InvokeAsync(StateHasChanged);
    }

    internal void Unregister(OpAccordionPanel panel)
    {
        if (!_panels.Remove(panel))
        {
            return;
        }

        // Renumera para um painel registrado depois não colidir com o índice de um existente.
        for (var i = 0; i < _panels.Count; i++)
        {
            _panels[i].Index = i;
        }

        _ = InvokeAsync(StateHasChanged);
    }

    internal bool IsActive(int index) => ActiveIndexesList.Contains(index);

    internal async Task ToggleAsync(int index)
    {
        var list = new List<int>(ActiveIndexesList);

        if (!list.Remove(index))
        {
            if (!Multiple)
            {
                list.Clear();
            }

            list.Add(index);
        }

        ActiveIndexes = list;
        await ActiveIndexesChanged.InvokeAsync(list);
        StateHasChanged();
    }
}
