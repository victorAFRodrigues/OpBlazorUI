using Microsoft.AspNetCore.Components;

namespace OpBlazorUI.Base.Models;

public sealed class OpStepItem
{
    public string? Label { get; set; }
    public RenderFragment? Content { get; set; }
    public bool Disabled { get; set; }
}
