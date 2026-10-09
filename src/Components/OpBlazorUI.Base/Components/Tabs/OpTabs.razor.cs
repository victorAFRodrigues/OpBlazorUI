using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;
using OpBlazorUI.Base.Components.Common;

namespace OpBlazorUI.Base.Components.Tabs;

public partial class OpTabs : OpComponentBase, IOpTabHost
{
    private readonly List<OpTabPanel> _panels = new();
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
                await SelectAsync(Math.Min(index + 1, _panels.Count - 1));
                break;

            case "ArrowLeft":
                await SelectAsync(Math.Max(index - 1, 0));
                break;

            case "Home":
                await SelectAsync(0);
                break;

            case "End":
                await SelectAsync(_panels.Count - 1);
                break;
        }
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
