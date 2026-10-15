using Microsoft.AspNetCore.Components;
using OpBlazorUI.Components.Common;

namespace OpBlazorUI.Components.Stepper;

/// <summary>
/// Lista de cabeçalhos do layout horizontal. Registra os <see cref="OpStep"/> filhos para que cada
/// passo saiba se deve renderizar o separador (linha conectora) até o próximo.
/// </summary>
public partial class OpStepList : OpComponentBase
{
    private readonly List<OpStep> _steps = new();

    [Parameter] public RenderFragment? ChildContent { get; set; }

    internal int Count => _steps.Count;

    private string RootClass => Class("p-steplist", StyleClass);

    internal void Register(OpStep step)
    {
        if (_steps.Contains(step))
        {
            return;
        }

        step.Index = _steps.Count;
        _steps.Add(step);
        _ = InvokeAsync(StateHasChanged);
    }
}
