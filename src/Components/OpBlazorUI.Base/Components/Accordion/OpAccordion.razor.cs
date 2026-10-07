using Microsoft.AspNetCore.Components;
using OpBlazorUI.Base.Components.Common;

namespace OpBlazorUI.Base.Components.Accordion;

public partial class OpAccordion : OpComponentBase
{
    private readonly List<OpAccordionPanel> _panels = new();

    [Parameter] public bool Multiple { get; set; }
    [Parameter] public IReadOnlyList<int> ActiveIndexes { get; set; } = Array.Empty<int>();
    [Parameter] public RenderFragment? ChildContent { get; set; }

    [Parameter] public EventCallback<IReadOnlyList<int>> ActiveIndexesChanged { get; set; }

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

    internal void Unregister(OpAccordionPanel panel) => _panels.Remove(panel);

    internal bool IsActive(int index) => ActiveIndexes.Contains(index);

    internal async Task ToggleAsync(int index)
    {
        var list = new List<int>(ActiveIndexes);

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
