using Microsoft.AspNetCore.Components;
using OpBlazorUI.Components.Common;

namespace OpBlazorUI.Components.Stepper;

/// <summary>
/// Contêiner dos painéis de conteúdo no layout horizontal. Cada painel é um <see cref="OpStepPanel"/>
/// ligado a um <see cref="OpStep"/> pelo mesmo <c>Value</c>.
/// </summary>
public partial class OpStepPanels : OpComponentBase
{
    [Parameter] public RenderFragment? ChildContent { get; set; }

    private string RootClass => Class("p-steppanels", StyleClass);
}
