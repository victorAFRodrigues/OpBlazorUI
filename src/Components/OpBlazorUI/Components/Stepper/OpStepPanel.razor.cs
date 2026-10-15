using Microsoft.AspNetCore.Components;
using OpBlazorUI.Components.Common;

namespace OpBlazorUI.Components.Stepper;

/// <summary>
/// Painel de conteúdo de um passo. Só é renderizado quando o passo correspondente está ativo.
/// No layout horizontal fica dentro de <see cref="OpStepPanels"/>; no vertical, dentro do
/// <see cref="OpStepItem"/> (herdando o <c>Value</c>).
/// </summary>
public partial class OpStepPanel : OpComponentBase
{
    [CascadingParameter] public OpStepper? Stepper { get; set; }

    [CascadingParameter] public OpStepItem? StepItem { get; set; }

    /// <summary>Valor do passo; no vertical é herdado do <see cref="OpStepItem"/>.</summary>
    [Parameter] public int? Value { get; set; }

    [Parameter] public RenderFragment? ChildContent { get; set; }

    /// <summary>Conteúdo do painel com acesso ao <c>ActivateCallback</c>.</summary>
    [Parameter] public RenderFragment<OpStepPanelContext>? ContentTemplate { get; set; }

    internal int? ResolvedValue => Value ?? StepItem?.Value;

    internal bool Active => Stepper is not null && Stepper.Value == ResolvedValue;

    internal bool ShowSeparator => StepItem is not null && !StepItem.IsLast;

    private string RootClass => Class("p-steppanel", Active ? "p-steppanel-active" : null, StyleClass);

    private OpStepPanelContext Context => new()
    {
        ActivateCallback = value => Stepper?.ActivateAsync(value) ?? Task.CompletedTask,
        Value = ResolvedValue,
        Active = Active
    };
}
