using Microsoft.AspNetCore.Components;

namespace OpBlazorUI.Base.Components.Button.Parts;

public partial class OpButtonLabel : ComponentBase
{
    [Parameter] public string? Label { get; set; }
}