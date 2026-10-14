namespace OpBlazorUI.Base.Components.DynamicDialog;

/// <summary>Configuração de um diálogo aberto pelo <see cref="Services.OpDialogService"/>.</summary>
public sealed class OpDynamicDialogConfig
{
    public string? Header { get; set; }
    public string? Width { get; set; }
    public string? Style { get; set; }
    public string? StyleClass { get; set; }
    public bool Modal { get; set; } = true;
    public bool Closable { get; set; } = true;
    public bool CloseOnEscape { get; set; } = true;
    public bool DismissableMask { get; set; }
    public bool Draggable { get; set; } = true;
    public bool Resizable { get; set; }

    /// <summary>Dados genéricos acessíveis ao componente de conteúdo via <c>CascadingParameter</c>.</summary>
    public object? Data { get; set; }

    /// <summary>Parâmetros passados como <c>ParameterView</c> ao componente de conteúdo.</summary>
    public Dictionary<string, object?> InputValues { get; set; } = new();
}
