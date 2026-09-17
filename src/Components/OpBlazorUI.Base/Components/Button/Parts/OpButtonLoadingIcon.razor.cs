using Microsoft.AspNetCore.Components;

namespace OpBlazorUI.Base.Components.Button.Parts;

public partial class OpButtonLoadingIcon : ComponentBase
{
    [Parameter] public string? Icon { get; set; }

    [Parameter] public RenderFragment? Template { get; set; }
}