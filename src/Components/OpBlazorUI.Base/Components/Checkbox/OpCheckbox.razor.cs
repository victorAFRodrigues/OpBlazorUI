using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;

namespace OpBlazorUI.Base.Components.Checkbox;

public partial class OpCheckbox : ComponentBase
{
    [Parameter] public bool? Checked { get; set; }
    [Parameter] public EventCallback<bool?> CheckedChanged { get; set; }
    [Parameter] public bool Indeterminate { get; set; }
    [Parameter] public string? Value { get; set; }
    [Parameter] public string? Name { get; set; }
    [Parameter] public string? Id { get; set; }
    [Parameter] public string? AriaLabel { get; set; }
    [Parameter] public string? AriaLabelledBy { get; set; }
    [Parameter] public bool Required { get; set; }
    [Parameter] public bool Readonly { get; set; }
    [Parameter] public bool Disabled { get; set; }
    [Parameter] public bool Invalid { get; set; }
    [Parameter] public string? Size { get; set; }
    [Parameter] public string Variant { get; set; } = "outlined";
    [Parameter] public int TabIndex { get; set; }
    [Parameter] public string? CheckboxIcon { get; set; }
    [Parameter] public string? StyleClass { get; set; }
    [Parameter] public string? InputClass { get; set; }

    [Parameter] public EventCallback<bool> OnChange { get; set; }
    [Parameter] public EventCallback<FocusEventArgs> OnFocus { get; set; }
    [Parameter] public EventCallback<FocusEventArgs> OnBlur { get; set; }

    [Parameter] public RenderFragment<bool>? CheckboxIconTemplate { get; set; }

    [Parameter(CaptureUnmatchedValues = true)]
    public Dictionary<string, object>? AdditionalAttributes { get; set; }

    private string RootClass => BuildClass(
        "p-checkbox p-component",
        Checked == true ? "p-checkbox-checked p-highlight" : null,
        Disabled ? "p-disabled" : null,
        Invalid ? "p-invalid" : null,
        Variant == "filled" ? "p-variant-filled" : null,
        Size == "small" ? "p-checkbox-sm p-inputfield-sm" : null,
        Size == "large" ? "p-checkbox-lg p-inputfield-lg" : null,
        StyleClass);

    private string InputCss => BuildClass("p-checkbox-input", InputClass);

    private async Task HandleChange(ChangeEventArgs e)
    {
        if (Readonly || Disabled) return;

        var next = (bool)(e.Value ?? false);
        Checked = next;
        await CheckedChanged.InvokeAsync(next);
        await OnChange.InvokeAsync(next);
    }

    private static string BuildClass(params string?[] classes)
        => string.Join(' ', classes.Where(c => !string.IsNullOrWhiteSpace(c)));
}