namespace OpBlazorUI.Base.Components.DynamicDialog;

/// <summary>Referência de um diálogo dinâmico. Injetável no componente de conteúdo como CascadingParameter.</summary>
public sealed class OpDynamicDialogRef
{
    private readonly TaskCompletionSource<object?> _result = new();

    internal OpDynamicDialogRef(OpDynamicDialogConfig config)
    {
        Config = config;
    }

    internal Action<object?>? CloseAction { get; set; }

    public OpDynamicDialogConfig Config { get; }

    /// <summary>Concluída quando o diálogo fecha, com o valor passado a <see cref="Close"/>.</summary>
    public Task<object?> Result => _result.Task;

    /// <summary>Fecha o diálogo, opcionalmente retornando um resultado ao chamador.</summary>
    public void Close(object? result = null) => CloseAction?.Invoke(result);

    internal void Complete(object? result)
    {
        if (!_result.Task.IsCompleted)
        {
            _result.SetResult(result);
        }
    }
}

/// <summary>Instância em exibição no host. Uso interno do <see cref="OpDynamicDialogHost"/>.</summary>
public sealed class OpDynamicDialogInstance
{
    internal OpDynamicDialogInstance(Type componentType, OpDynamicDialogConfig config, OpDynamicDialogRef reference)
    {
        ComponentType = componentType;
        Config = config;
        Reference = reference;
        Parameters = new Dictionary<string, object?>(config.InputValues);
    }

    public Type ComponentType { get; }
    public OpDynamicDialogConfig Config { get; }
    public OpDynamicDialogRef Reference { get; }
    public Dictionary<string, object?> Parameters { get; }
}
