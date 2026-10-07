using Microsoft.AspNetCore.Components;

namespace OpBlazorUI.Base.Components.Button.Parts;

public partial class OpButtonLoadingIcon : ComponentBase
{
    [Parameter] public string? Icon { get; set; }
    [Parameter] public string IconPos { get; set; } = "left";
    [Parameter] public bool HasLabel { get; set; }

    [Parameter] public RenderFragment? Template { get; set; }

    // Spinner padrão: mesmas classes do ícone (como o spinnerIcon do upstream).
    private string SpinnerClass => HasLabel
        ? $"p-button-loading-icon p-button-icon p-button-icon-{IconPos}"
        : "p-button-loading-icon p-button-icon";
}
