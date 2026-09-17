using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;

namespace OpBlazorUI.Base.Components.InputText;

public partial class OpInputText : ComponentBase
{
    [Parameter] public string? Value { get; set; }
    [Parameter] public EventCallback<string?> ValueChanged { get; set; }
    [Parameter] public string? Placeholder { get; set; }
    [Parameter] public string Variant { get; set; } = "outlined";
    [Parameter] public string? Size { get; set; }
    [Parameter] public bool Invalid { get; set; }
    [Parameter] public bool Disabled { get; set; }
    [Parameter] public bool Readonly { get; set; }
    [Parameter] public bool Fluid { get; set; }
    [Parameter] public string Type { get; set; } = "text";
    [Parameter] public string? Id { get; set; }
    [Parameter] public string? Name { get; set; }
    [Parameter] public string? AriaLabel { get; set; }
    [Parameter] public int? MaxLength { get; set; }
    [Parameter] public string? Autocomplete { get; set; }
    [Parameter] public string? StyleClass { get; set; }

    [Parameter] public EventCallback<string?> OnInput { get; set; }
    [Parameter] public EventCallback<string?> OnChange { get; set; }
    [Parameter] public EventCallback<FocusEventArgs> OnFocus { get; set; }
    [Parameter] public EventCallback<FocusEventArgs> OnBlur { get; set; }
    [Parameter] public EventCallback<KeyboardEventArgs> OnKeyDown { get; set; }

    [Parameter(CaptureUnmatchedValues = true)]
    public Dictionary<string, object>? AdditionalAttributes { get; set; }

    private string RootClass => BuildClass(
        "p-inputtext p-component",
        !string.IsNullOrEmpty(Value) ? "p-filled" : null,
        Size == "small" ? "p-inputtext-sm" : null,
        Size == "large" ? "p-inputtext-lg" : null,
        Invalid ? "p-invalid" : null,
        Variant == "filled" ? "p-variant-filled" : null,
        Fluid ? "p-inputtext-fluid" : null,
        StyleClass);

    private async Task HandleInput(ChangeEventArgs e)
    {
        var text = e.Value?.ToString();
        Value = text;
        await ValueChanged.InvokeAsync(text);
        await OnInput.InvokeAsync(text);
    }

    private async Task HandleChange(ChangeEventArgs e)
    {
        await OnChange.InvokeAsync(e.Value?.ToString());
    }

    private static string BuildClass(params string?[] classes)
        => string.Join(' ', classes.Where(c => !string.IsNullOrWhiteSpace(c)));
}