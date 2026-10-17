using Microsoft.AspNetCore.Components;
using OpBlazorUI.Components.Common;
using OpBlazorUI.Models;

namespace OpBlazorUI.Components.Menu;

public partial class OpTieredMenu : OpComponentBase
{
    private readonly HashSet<string> _open = new(StringComparer.Ordinal);
    private ElementReference? _anchor;
    private bool _visible;

    [Parameter] public IReadOnlyList<OpMenuItem> Model { get; set; } = Array.Empty<OpMenuItem>();
    [Parameter] public EventCallback<OpMenuItem> OnItemClick { get; set; }

    /// <summary>Template de item (equivalente ao <c>#item</c> do upstream).</summary>
    [Parameter] public RenderFragment<OpMenuItemContext>? ItemTemplate { get; set; }

    /// <summary>Modo popup: o menu fica oculto até <see cref="ShowAsync"/> ou <see cref="ToggleAsync"/>.</summary>
    [Parameter] public bool Popup { get; set; }

    [Parameter] public EventCallback OnShow { get; set; }
    [Parameter] public EventCallback OnHide { get; set; }

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

    private Task ToggleSubmenuAsync(string path)
    {
        OpMenuPaths.Toggle(_open, path);
        return Task.CompletedTask;
    }

    // Clicar numa folha fecha todos os submenus (e o popup), como no PrimeNG.
    private async Task OnLeafClickAsync(OpMenuItem item)
    {
        CloseAll();
        if (Popup)
        {
            _visible = false;
        }

        await OnItemClick.InvokeAsync(item);
    }

    /// <summary>Exibe o menu no modo popup (ancorado ao elemento anterior quando <paramref name="anchor"/> é nulo).</summary>
    public Task ShowAsync(ElementReference? anchor = null)
    {
        _anchor = anchor;
        _open.Clear();
        _visible = true;
        StateHasChanged();
        return OnShow.InvokeAsync();
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

    public Task ToggleAsync(ElementReference? anchor = null)
        => _visible ? HideAsync() : ShowAsync(anchor);
}
