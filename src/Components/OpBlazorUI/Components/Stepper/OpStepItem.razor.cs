using Microsoft.AspNetCore.Components;
using OpBlazorUI.Components.Common;

namespace OpBlazorUI.Components.Stepper;

/// <summary>
/// Invólucro usado no layout <b>vertical</b>: agrupa um <see cref="OpStep"/> e um
/// <see cref="OpStepPanel"/> ligados pelo mesmo <c>Value</c>, que é herdado pelos filhos.
/// É o equivalente ao <c>StepItem</c> do upstream.
/// </summary>
public partial class OpStepItem : OpComponentBase
{
    [CascadingParameter] public OpStepper? Stepper { get; set; }

    /// <summary>Valor do passo; herdado pelo <see cref="OpStep"/> e pelo <see cref="OpStepPanel"/> filhos.</summary>
    [Parameter] public int? Value { get; set; }

    [Parameter] public RenderFragment? ChildContent { get; set; }

    internal int Index { get; set; }

    internal bool Active => Stepper is not null && Stepper.Value == Value;

    internal bool IsLast => Stepper is not null && Index == Stepper.StepItemCount - 1;

    private string RootClass => Class("p-stepitem", Active ? "p-stepitem-active" : null, StyleClass);

    protected override void OnInitialized() => Stepper?.RegisterStepItem(this);
}
