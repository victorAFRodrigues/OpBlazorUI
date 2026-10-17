namespace OpBlazorUI.Models;

public sealed class OpMenuItem
{
    public string? Label { get; set; }
    public string? Icon { get; set; }
    public string? Url { get; set; }
    public string? Target { get; set; }
    public Action? Command { get; set; }
    public List<OpMenuItem>? Items { get; set; }
    public bool Disabled { get; set; }
    public bool Separator { get; set; }
    public bool Visible { get; set; } = true;
    public string? StyleClass { get; set; }

    /// <summary>Etiqueta numérica exibida à direita do item (ex.: contador).</summary>
    public string? Badge { get; set; }

    /// <summary>Atalho de teclado exibido à direita do item (apenas informativo).</summary>
    public string? Shortcut { get; set; }

    /// <summary>
    /// Estado de expansão do painel (usado pelo PanelMenu no modo controlado).
    /// <c>null</c> deixa o painel controlar a própria expansão.
    /// </summary>
    public bool? Expanded { get; set; }

    public bool HasChildren => Items is { Count: > 0 };
}

public sealed class OpMenuGroup
{
    public string? Label { get; set; }
    public List<OpMenuItem> Items { get; set; } = new();
}