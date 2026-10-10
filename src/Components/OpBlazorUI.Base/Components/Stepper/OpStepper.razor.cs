using Microsoft.AspNetCore.Components;
using OpBlazorUI.Base.Components.Common;
using OpBlazorUI.Base.Models;

namespace OpBlazorUI.Base.Components.Stepper;

public partial class OpStepper : OpComponentBase
{
    [Parameter] public IReadOnlyList<OpStepItem> Model { get; set; } = Array.Empty<OpStepItem>();
    [Parameter] public int ActiveIndex { get; set; }
    [Parameter] public bool Readonly { get; set; }

    /// <summary>Navegação linear: só permite avançar para o próximo passo, sem pular etapas.</summary>
    [Parameter] public bool Linear { get; set; }

    [Parameter] public EventCallback<int> ActiveIndexChanged { get; set; }
    [Parameter] public EventCallback<OpStepItem> OnSelect { get; set; }

    private string RootClass => Class("p-stepper p-component", Readonly ? "p-stepper-readonly" : null, StyleClass);

    private async Task SelectAsync(int index)
    {
        if (Readonly || index < 0 || index >= Model.Count || Model[index].Disabled)
        {
            return;
        }

        // Linear: não deixa pular etapas para frente (voltar é sempre permitido).
        if (Linear && index > ActiveIndex + 1)
        {
            return;
        }

        ActiveIndex = index;
        await ActiveIndexChanged.InvokeAsync(index);
        await OnSelect.InvokeAsync(Model[index]);
    }
}
