using Microsoft.AspNetCore.Components;
using OpBlazorUI.Base.Models;

namespace OpBlazorUI.Base.Components.Menu;

/// <summary>Renderizador recursivo de listas de <see cref="OpMenuItem"/> usado pela família de menus.</summary>
public partial class OpMenuItems : ComponentBase
{
    [Parameter, EditorRequired] public IReadOnlyList<OpMenuItem> Items { get; set; } = Array.Empty<OpMenuItem>();
    [Parameter] public string Prefix { get; set; } = "menu";
    [Parameter] public bool Root { get; set; }
    [Parameter] public int Level { get; set; }
    [Parameter] public string PathPrefix { get; set; } = "";
    [Parameter] public Func<string, bool> IsOpen { get; set; } = _ => false;
    [Parameter] public EventCallback<string> OnToggle { get; set; }
    [Parameter] public EventCallback<OpMenuItem> OnItemClick { get; set; }

    private string ListClass => Root
        ? $"p-{Prefix}-root-list"
        : $"p-{Prefix}-submenu p-{Prefix}-submenu-open";

    private string? ListStyle => Root
        ? null
        : Level <= 1
            ? "display: block; position: absolute; top: 100%; left: 0; z-index: 10;"
            : "display: block; position: absolute; top: 0; left: 100%; z-index: 10;";

    private string SubmenuIconClass => $"p-{Prefix}-submenu-icon pi " + (Root ? "pi-angle-down" : "pi-angle-right");

    private string ItemClass(OpMenuItem item, bool hasChildren, bool open) => string.Join(' ',
        new[]
        {
            $"p-{Prefix}-item",
            item.Disabled ? "p-disabled" : null,
            hasChildren && open ? $"p-{Prefix}-item-active" : null,
        }.Where(c => !string.IsNullOrWhiteSpace(c)));

    private async Task OnSelectAsync(OpMenuItem item, bool hasChildren, string path)
    {
        if (item.Disabled)
        {
            return;
        }

        if (hasChildren)
        {
            await OnToggle.InvokeAsync(path);
            return;
        }

        item.Command?.Invoke();
        await OnItemClick.InvokeAsync(item);
    }
}
