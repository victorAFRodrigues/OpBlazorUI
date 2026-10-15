namespace OpBlazorUI.Components.Stepper;

/// <summary>
/// Contexto do template de cabeçalho de um <see cref="OpStep"/> (equivalente ao template
/// <c>#content</c> do <c>p-step</c> no upstream).
/// </summary>
public sealed class OpStepContext
{
    /// <summary>Ativa este passo.</summary>
    public Func<Task> ActivateCallback { get; init; } = static () => Task.CompletedTask;

    /// <summary>Valor do passo.</summary>
    public int? Value { get; init; }

    /// <summary>Indica se o passo está ativo.</summary>
    public bool Active { get; init; }
}

/// <summary>
/// Contexto do template de conteúdo de um <see cref="OpStepPanel"/> (equivalente ao template
/// <c>#content</c> do <c>p-step-panel</c> no upstream).
/// </summary>
public sealed class OpStepPanelContext
{
    /// <summary>Ativa o passo de valor indicado (o <c>activateCallback</c> do upstream).</summary>
    public Func<int, Task> ActivateCallback { get; init; } = static _ => Task.CompletedTask;

    /// <summary>Valor do passo correspondente ao painel.</summary>
    public int? Value { get; init; }

    /// <summary>Indica se o painel está ativo.</summary>
    public bool Active { get; init; }
}
