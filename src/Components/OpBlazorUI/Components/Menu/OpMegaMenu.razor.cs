using Microsoft.AspNetCore.Components;
using OpBlazorUI.Components.Common;
using OpBlazorUI.Models;

namespace OpBlazorUI.Components.Menu;

public partial class OpMegaMenu : OpComponentBase
{
    private ElementReference _rootRef;
    private readonly HashSet<OpMenuItem> _open = new();

    [Parameter] public IReadOnlyList<OpMenuItem> Model { get; set; } = Array.Empty<OpMenuItem>();
    [Parameter] public RenderFragment? StartTemplate { get; set; }
    [Parameter] public RenderFragment? EndTemplate { get; set; }
    [Parameter] public EventCallback<OpMenuItem> OnItemClick { get; set; }

    /// <summary>Template de item (equivalente ao <c>#item</c> do upstream).</summary>
    [Parameter] public RenderFragment<OpMenuItemContext>? ItemTemplate { get; set; }

    /// <summary>Orientação do menu: <c>horizontal</c> (padrão) ou <c>vertical</c>.</summary>
    [Parameter] public string Orientation { get; set; } = "horizontal";

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

    private string RootClass => Class(
        Orientation == "vertical" ? "p-megamenu p-component p-megamenu-vertical" : "p-megamenu p-component",
        StyleClass);

    private bool IsVertical => Orientation == "vertical";

    /// <summary>Posiciona o overlay: abaixo (horizontal) ou ao lado (vertical).</summary>
    private string OverlayStyle => IsVertical
        ? "display: block; position: absolute; top: 0; left: 100%; z-index: 10;"
        : "display: block; position: absolute; top: 100%; left: 0; z-index: 10;";

    private string SubmenuIconClass => IsVertical
        ? "p-megamenu-submenu-icon pi pi-angle-right"
        : "p-megamenu-submenu-icon pi pi-angle-down";

    private bool IsOpen(OpMenuItem item) => _open.Contains(item);

    private async Task OnSelectAsync(OpMenuItem item, bool hasChildren)
    {
        if (hasChildren)
        {
            if (!_open.Remove(item))
            {
                _open.Clear();
                _open.Add(item);
            }

            return;
        }

        await SelectItemAsync(item);
    }

    private async Task SelectItemAsync(OpMenuItem item)
    {
        if (item.Disabled)
        {
            return;
        }

        item.Command?.Invoke();
        await OnItemClick.InvokeAsync(item);
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        await Interop.InvokeVoidAsync(
            OpInterop.MenuInterop, "initMenuFocusKeyboard", _rootRef, "a.p-megamenu-item-link");
    }
}
