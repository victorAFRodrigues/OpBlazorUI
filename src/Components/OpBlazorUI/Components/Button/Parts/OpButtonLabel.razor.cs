using Microsoft.AspNetCore.Components;

namespace OpBlazorUI.Components.Button.Parts;

public partial class OpButtonLabel : ComponentBase
{
    [Parameter] public string? Label { get; set; }
}