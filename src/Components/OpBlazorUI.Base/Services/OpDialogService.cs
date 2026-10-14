using Microsoft.AspNetCore.Components;
using OpBlazorUI.Base.Components.DynamicDialog;

namespace OpBlazorUI.Base.Services;

/// <summary>
/// Abre diálogos dinâmicos: um componente qualquer é carregado dentro de um <c>OpDialog</c> pelo
/// <see cref="OpDynamicDialogHost"/>, que deve ser renderizado uma única vez no layout.
/// </summary>
public sealed class OpDialogService
{
    private readonly List<OpDynamicDialogInstance> _instances = new();

    internal event Action? Changed;

    public IReadOnlyList<OpDynamicDialogInstance> Instances => _instances;

    /// <summary>Abre um diálogo com o componente <typeparamref name="TComponent"/> como conteúdo.</summary>
    public OpDynamicDialogRef Open<TComponent>(OpDynamicDialogConfig? config = null)
        where TComponent : IComponent
        => Open(typeof(TComponent), config);

    /// <summary>Abre um diálogo com o tipo de componente informado como conteúdo.</summary>
    public OpDynamicDialogRef Open(Type componentType, OpDynamicDialogConfig? config = null)
    {
        ArgumentNullException.ThrowIfNull(componentType);

        var effective = config ?? new OpDynamicDialogConfig();
        var reference = new OpDynamicDialogRef(effective);
        var instance = new OpDynamicDialogInstance(componentType, effective, reference);
        reference.CloseAction = result => Close(instance, result);

        _instances.Add(instance);
        Changed?.Invoke();
        return reference;
    }

    /// <summary>Abre o diálogo e aguarda o resultado de <c>Close</c>.</summary>
    public Task<object?> OpenAsync<TComponent>(OpDynamicDialogConfig? config = null)
        where TComponent : IComponent
        => Open<TComponent>(config).Result;

    internal void Close(OpDynamicDialogInstance instance, object? result)
    {
        if (!_instances.Remove(instance))
        {
            return;
        }

        instance.Reference.Complete(result);
        Changed?.Invoke();
    }
}
