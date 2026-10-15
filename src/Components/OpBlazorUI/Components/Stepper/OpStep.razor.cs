using Microsoft.AspNetCore.Components;
using OpBlazorUI.Components.Common;

namespace OpBlazorUI.Components.Stepper;

/// <summary>
/// Um passo do Stepper: o cabeçalho (número + título) e a linha conectora até o próximo passo.
/// No layout horizontal fica dentro de um <see cref="OpStepList"/>; no vertical, de um
/// <see cref="OpStepItem"/> (onde o <c>Value</c> pode ser herdado).
/// </summary>
public partial class OpStep : OpComponentBase
{
    [CascadingParameter] public OpStepper? Stepper { get; set; }

    [CascadingParameter] public OpStepList? StepList { get; set; }

    [CascadingParameter] public OpStepItem? StepItem { get; set; }

    /// <summary>Valor do passo. No layout vertical é herdado do <see cref="OpStepItem"/> quando omitido.</summary>
    [Parameter] public int? Value { get; set; }

    /// <summary>Desabilita o passo (além do bloqueio imposto por <see cref="OpStepper.Linear"/>).</summary>
    [Parameter] public bool Disabled { get; set; }

    /// <summary>Rótulo do passo, exibido ao lado do número.</summary>
    [Parameter] public RenderFragment? ChildContent { get; set; }

    /// <summary>Cabeçalho customizado; quando informado, substitui o botão padrão.</summary>
    [Parameter] public RenderFragment<OpStepContext>? ContentTemplate { get; set; }

    internal int Index { get; set; }

    internal int? ResolvedValue => Value ?? StepItem?.Value;

    internal bool Active => Stepper is not null && Stepper.Value == ResolvedValue;

    internal bool DisabledState => Stepper is not null && !Active && (Stepper.Linear || Disabled);

    internal bool ShowSeparator => StepList is not null && Index != StepList.Count - 1;

    private string RootClass => Class("p-step", Active ? "p-step-active" : null, DisabledState ? "p-disabled" : null, StyleClass);

    private OpStepContext Context => new()
    {
        ActivateCallback = ActivateAsync,
        Value = ResolvedValue,
        Active = Active
    };

    protected override void OnInitialized() => StepList?.Register(this);

    private Task ActivateAsync()
    {
        if (DisabledState || Stepper is null)
        {
            return Task.CompletedTask;
        }

        return Stepper.ActivateAsync(ResolvedValue ?? 0);
    }
}
