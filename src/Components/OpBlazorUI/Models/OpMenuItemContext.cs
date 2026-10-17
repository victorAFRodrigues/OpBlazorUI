namespace OpBlazorUI.Models;

/// <summary>
/// Contexto dos templates de item dos menus, equivalente ao
/// <c>let-item</c> / <c>let-root</c> do template <c>#item</c> do PrimeNG.
/// </summary>
public sealed class OpMenuItemContext
{
    /// <summary>Item do modelo sendo renderizado.</summary>
    public OpMenuItem Item { get; init; } = new();

    /// <summary>Indica que o item abre um submenu.</summary>
    public bool HasSubmenu { get; init; }

    /// <summary>Indica que o item pertence ao nível raiz do menu.</summary>
    public bool Root { get; init; }
}
