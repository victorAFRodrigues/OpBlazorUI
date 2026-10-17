using Microsoft.AspNetCore.Components;
using OpBlazorUI.Components.Common;
using OpBlazorUI.Models;

namespace OpBlazorUI.Components.Menu;

public partial class OpMenubar : OpComponentBase
{
    private readonly HashSet<string> _open = new(StringComparer.Ordinal);

    [Parameter] public IReadOnlyList<OpMenuItem> Model { get; set; } = Array.Empty<OpMenuItem>();
    [Parameter] public RenderFragment? StartTemplate { get; set; }
    [Parameter] public RenderFragment? EndTemplate { get; set; }
    [Parameter] public EventCallback<OpMenuItem> OnItemClick { get; set; }

    /// <summary>Template de item (equivalente ao <c>#item</c> do upstream).</summary>
    [Parameter] public RenderFragment<OpMenuItemContext>? ItemTemplate { get; set; }

    private string RootClass => Class("p-menubar p-component", StyleClass);

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
}
