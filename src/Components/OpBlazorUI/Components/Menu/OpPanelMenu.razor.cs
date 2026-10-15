using Microsoft.AspNetCore.Components;
using OpBlazorUI.Components.Common;
using OpBlazorUI.Models;

namespace OpBlazorUI.Components.Menu;

public partial class OpPanelMenu : OpComponentBase
{
    private readonly HashSet<OpMenuItem> _open = new();
    private readonly string _uid = "oppm-" + Guid.NewGuid().ToString("N")[..8];
    private ElementReference _rootRef;

    [Parameter] public IReadOnlyList<OpMenuItem> Model { get; set; } = Array.Empty<OpMenuItem>();

    /// <summary>Permite mais de um painel aberto ao mesmo tempo (padrão do upstream: apenas um).</summary>
    [Parameter] public bool Multiple { get; set; }

    [Parameter] public EventCallback<OpMenuItem> OnItemClick { get; set; }

    /// <summary>Template de item (equivalente ao <c>#item</c> do upstream).</summary>
    [Parameter] public RenderFragment<OpMenuItemContext>? ItemTemplate { get; set; }

    private string RootClass => Class("p-panelmenu p-component", StyleClass);

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        await Interop.InvokeVoidAsync(
            OpInterop.MenuInterop, "initMenuFocusKeyboard", _rootRef,
            "a.p-panelmenu-header-link, a.p-panelmenu-item-link");
    }

    // Modelo controlado (Expanded definido) tem prioridade sobre o estado interno.
    private bool IsOpen(OpMenuItem panel) => panel.Expanded ?? _open.Contains(panel);

    private Task ToggleAsync(OpMenuItem panel)
    {
        if (panel.Expanded is not null)
        {
            // Painel controlado pelo modelo: o estado vem de Expanded.
            return Task.CompletedTask;
        }

        if (_open.Contains(panel))
        {
            _open.Remove(panel);
        }
        else
        {
            if (!Multiple)
            {
                // Modo single: apenas um painel-raiz ativo. Mantém a cadeia de ancestrais do
                // painel aberto (senão o item aninhado nem seria renderizado) e fecha o resto.
                var chain = new List<OpMenuItem>();
                _open.Clear();

                if (!TryBuildPath(Model, panel, chain))
                {
                    chain.Add(panel);
                }

                foreach (var node in chain)
                {
                    _open.Add(node);
                }
            }
            else
            {
                _open.Add(panel);
            }
        }

        return Task.CompletedTask;
    }

    private static bool TryBuildPath(IEnumerable<OpMenuItem> items, OpMenuItem target, List<OpMenuItem> path)
    {
        foreach (var item in items)
        {
            path.Add(item);

            if (ReferenceEquals(item, target))
            {
                return true;
            }

            if (item.Items is { Count: > 0 } && TryBuildPath(item.Items, target, path))
            {
                return true;
            }

            path.RemoveAt(path.Count - 1);
        }

        return false;
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
}
