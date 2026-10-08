using Microsoft.AspNetCore.Components;
using OpBlazorUI.Base.Components.Common;
using OpBlazorUI.Base.Models;

namespace OpBlazorUI.Base.Components.Menu;

public partial class OpContextMenu : OpComponentBase
{
    private readonly HashSet<string> _open = new(StringComparer.Ordinal);
    private ElementReference _panel;
    private ElementReference? _anchor;
    private bool _visible;

    [Parameter] public IReadOnlyList<OpMenuItem> Model { get; set; } = Array.Empty<OpMenuItem>();
    [Parameter] public EventCallback<OpMenuItem> OnItemClick { get; set; }
    [Parameter] public EventCallback OnShow { get; set; }
    [Parameter] public EventCallback OnHide { get; set; }

    private string RootClass => Class("p-contextmenu p-component", StyleClass);

    private bool IsOpen(string path) => _open.Contains(path);

    private Task ToggleAsync(string path)
    {
        if (!_open.Remove(path))
        {
            _open.Add(path);
        }

        return Task.CompletedTask;
    }

    /// <summary>Exibe o menu ancorado ao elemento informado (ou ao elemento anterior).</summary>
    public async Task ShowAsync(ElementReference? anchor = null)
    {
        _anchor = anchor;
        _visible = true;
        _open.Clear();
        await OnShow.InvokeAsync();
        StateHasChanged();
    }

    public async Task HideAsync()
    {
        if (!_visible)
        {
            return;
        }

        _visible = false;
        _open.Clear();
        await OnHide.InvokeAsync();
        StateHasChanged();
    }

    public async Task ToggleAsync(ElementReference? anchor = null)
    {
        if (_visible)
        {
            await HideAsync();
        }
        else
        {
            await ShowAsync(anchor);
        }
    }

    private async Task HandleItemClickAsync(OpMenuItem item)
    {
        await OnItemClick.InvokeAsync(item);
        await HideAsync();
    }
}
