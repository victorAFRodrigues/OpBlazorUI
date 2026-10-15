using Microsoft.AspNetCore.Components;
using OpBlazorUI.Components.Common;

namespace OpBlazorUI.Components.Stepper;

/// <summary>
/// Contêiner do fluxo em etapas. No layout horizontal recebe um <see cref="OpStepList"/> e um
/// <see cref="OpStepPanels"/>; no vertical recebe diretamente os <see cref="OpStepItem"/>.
/// </summary>
/// <remarks>
/// Porte do <c>p-stepper</c> do Optimus UI/PrimeNG v21: a API é composta por componentes-filhos
/// (StepList, Step, StepPanels, StepPanel, StepItem), ligados dois a dois por <see cref="Value"/>.
/// </remarks>
public partial class OpStepper : OpComponentBase
{
    private readonly List<OpStepItem> _stepItems = new();
    private readonly string _instanceId = $"op_id_{Guid.NewGuid():N}";

    /// <summary>Valor (etapa) ativa. Use <c>@@bind-Value</c> para duas vias.</summary>
    [Parameter] public int Value { get; set; }

    /// <summary>Disparado quando a etapa ativa muda.</summary>
    [Parameter] public EventCallback<int> ValueChanged { get; set; }

    /// <summary>
    /// Quando <c>true</c>, os cabeçalhos que não são o passo atual ficam desabilitados: é preciso
    /// concluir a etapa atual para avançar (a navegação continua possível pelos botões do painel).
    /// </summary>
    [Parameter] public bool Linear { get; set; }

    [Parameter] public RenderFragment? ChildContent { get; set; }

    /// <summary>Id do elemento raiz; quando não informado, um id gerado é usado (base dos ids de ARIA).</summary>
    internal string DomId => Id ?? _instanceId;

    internal int StepItemCount => _stepItems.Count;

    internal string StepId(int? value) => $"{DomId}_step_{value}";

    internal string StepPanelId(int? value) => $"{DomId}_steppanel_{value}";

    private string RootClass => Class("p-stepper p-component", StyleClass);

    internal async Task ActivateAsync(int value)
    {
        if (value == Value)
        {
            return;
        }

        Value = value;
        await ValueChanged.InvokeAsync(value);
    }

    internal void RegisterStepItem(OpStepItem item)
    {
        if (_stepItems.Contains(item))
        {
            return;
        }

        item.Index = _stepItems.Count;
        _stepItems.Add(item);
        _ = InvokeAsync(StateHasChanged);
    }
}
