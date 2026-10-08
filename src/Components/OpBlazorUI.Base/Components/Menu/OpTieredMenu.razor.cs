using Microsoft.AspNetCore.Components;
using OpBlazorUI.Base.Components.Common;
using OpBlazorUI.Base.Models;

namespace OpBlazorUI.Base.Components.Menu;

public partial class OpTieredMenu : OpComponentBase
{
    private readonly HashSet<string> _open = new(StringComparer.Ordinal);

    [Parameter] public IReadOnlyList<OpMenuItem> Model { get; set; } = Array.Empty<OpMenuItem>();
    [Parameter] public EventCallback<OpMenuItem> OnItemClick { get; set; }

    /// <summary>Fecha todos os submenus abertos.</summary>
    public void CloseAll()
    {
        _open.Clear();
        StateHasChanged();
    }

    private Task CloseAllAsync()
    {
        CloseAll();
        return Task.CompletedTask;
    }

    private string RootClass => Class("p-tieredmenu p-component", StyleClass);

    private bool IsOpen(string path) => _open.Contains(path);

    private Task ToggleAsync(string path)
    {
        if (!_open.Remove(path))
        {
            _open.Add(path);
        }

        return Task.CompletedTask;
    }
}
