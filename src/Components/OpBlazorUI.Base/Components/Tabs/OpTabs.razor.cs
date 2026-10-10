using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;
using OpBlazorUI.Base.Components.Common;

namespace OpBlazorUI.Base.Components.Tabs;

public partial class OpTabs : OpComponentBase, IOpTabHost
{
    private readonly List<OpTabPanel> _panels = new();
    private readonly List<ElementReference> _tabRefs = new();
    private readonly string _uid = "optabs_" + Guid.NewGuid().ToString("N")[..8];
    private ElementReference _viewport;

    [Parameter] public int ActiveIndex { get; set; }
    [Parameter] public bool Scrollable { get; set; }
    [Parameter] public string? AriaLabel { get; set; }
    [Parameter] public RenderFragment? ChildContent { get; set; }

    [Parameter] public EventCallback<int> ActiveIndexChanged { get; set; }
    [Parameter] public EventCallback<int> OnChange { get; set; }

    private IReadOnlyList<OpTabPanel> Panels => _panels;

    private OpTabPanel? ActivePanel => ActiveIndex >= 0 && ActiveIndex < _panels.Count ? _panels[ActiveIndex] : null;

    private string RootClass => Class("p-tabs p-component", StyleClass);

    private string ViewportClass => Class("p-tablist-content", Scrollable ? "p-tablist-viewport" : null);

    private string HeaderId(int index) => $"{Id ?? _uid}_{index}_header";

    private string ContentId(int index) => $"{Id ?? _uid}_{index}_content";

    private string TabClass(OpTabPanel panel, bool active)
        => Class("p-tab", active ? "p-tab-active" : null, panel.Disabled ? "p-disabled" : null);

    void IOpTabHost.Register(OpTabPanel panel)
    {
        if (_panels.Contains(panel))
        {
            return;
        }

        panel.Index = _panels.Count;
        _panels.Add(panel);
        while (_tabRefs.Count < _panels.Count)
        {
            _tabRefs.Add(default);
        }

        _ = InvokeAsync(StateHasChanged);
    }

    void IOpTabHost.Unregister(OpTabPanel panel)
    {
        var removed = panel.Index;
        if (!_panels.Remove(panel))
        {
            return;
        }

        Renumber();
        AdjustActiveIndex(removed);
        _ = InvokeAsync(StateHasChanged);
    }

    private void Renumber()
    {
        for (var i = 0; i < _panels.Count; i++)
        {
            _panels[i].Index = i;
        }
    }

    private void AdjustActiveIndex(int removedIndex)
    {
        if (removedIndex < ActiveIndex)
        {
            ActiveIndex--;
        }

        if (ActiveIndex >= _panels.Count)
        {
            ActiveIndex = Math.Max(0, _panels.Count - 1);
        }

        if (ActiveIndex < 0)
        {
            ActiveIndex = 0;
        }

        _ = ActiveIndexChanged.InvokeAsync(ActiveIndex);
    }

    private async Task SelectAsync(int index)
    {
        if (index < 0 || index >= _panels.Count || _panels[index].Disabled)
        {
            return;
        }

        ActiveIndex = index;
        await ActiveIndexChanged.InvokeAsync(index);
        await OnChange.InvokeAsync(index);
    }

    private async Task OnTabKeyDownAsync(KeyboardEventArgs e, int index)
    {
        switch (e.Code)
        {
            case "Enter":
            case "Space":
            case "NumpadEnter":
                await SelectAsync(index);
                break;

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
                await MoveFocusToAsync(FindEnabled(_panels.Count, -1));
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

        await SelectAsync(target);
        await FocusTabAsync(target);
    }

    private async Task FocusTabAsync(int index)
    {
        if (index < 0 || index >= _tabRefs.Count)
        {
            return;
        }

        try
        {
            await _tabRefs[index].FocusAsync();
        }
        catch
        {
            // elemento já removido
        }
    }

    // Percorre a partir de `from` (inclusive) na direção `step`, pulando abas desabilitadas.
    private int FindEnabled(int from, int step)
    {
        var count = _panels.Count;
        if (count == 0)
        {
            return -1;
        }

        var i = ((from % count) + count) % count;
        for (var n = 0; n < count; n++)
        {
            if (!_panels[i].Disabled)
            {
                return i;
            }

            i = (i + step + count) % count;
        }

        return -1;
    }

    private async Task ScrollAsync(int direction)
    {
        try
        {
            await Interop.InvokeVoidAsync(OpInterop.TabsInterop, "scroll", _viewport, direction * 160);
        }
        catch (JSDisconnectedException)
        {
            // SSR / circuito desconectado
        }
    }
}
