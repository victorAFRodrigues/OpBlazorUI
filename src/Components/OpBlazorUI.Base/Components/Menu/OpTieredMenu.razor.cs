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
        OpMenuPaths.Toggle(_open, path);
        return Task.CompletedTask;
    }

    // Clicar numa folha fecha todos os submenus, como no PrimeNG.
    private async Task OnLeafClickAsync(OpMenuItem item)
    {
        CloseAll();
        await OnItemClick.InvokeAsync(item);
    }
}
