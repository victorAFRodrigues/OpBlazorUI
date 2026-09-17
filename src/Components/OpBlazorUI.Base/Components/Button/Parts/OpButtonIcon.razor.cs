using Microsoft.AspNetCore.Components;

namespace OpBlazorUI.Base.Components.Button.Parts;

public partial class OpButtonIcon : ComponentBase
{
    [Parameter] public string? Icon { get; set; }
    [Parameter] public string IconPos { get; set; } = "left";
    [Parameter] public bool HasLabel { get; set; }

    [Parameter] public RenderFragment? Template { get; set; }

    private string IconClass => BuildClass(
        "p-button-icon",
        HasLabel ? $"p-button-icon-{IconPos}" : null,
        Icon);

    private static string BuildClass(params string?[] classes)
        => string.Join(' ', classes.Where(c => !string.IsNullOrWhiteSpace(c)));
}