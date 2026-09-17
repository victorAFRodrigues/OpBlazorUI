using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;

namespace OpBlazorUI.Base.Components.InputSwitch;

public partial class OpInputSwitch : ComponentBase
{
    [Parameter] public bool Checked { get; set; }
    [Parameter] public EventCallback<bool> CheckedChanged { get; set; }
    [Parameter] public string? Name { get; set; }
    [Parameter] public string? Id { get; set; }
    [Parameter] public string? AriaLabel { get; set; }
    [Parameter] public string? AriaLabelledBy { get; set; }
    [Parameter] public bool Required { get; set; }
    [Parameter] public bool Disabled { get; set; }
    [Parameter] public bool Invalid { get; set; }
    [Parameter] public int TabIndex { get; set; }
    [Parameter] public string? StyleClass { get; set; }

    [Parameter] public EventCallback<bool> OnChange { get; set; }
    [Parameter] public EventCallback<FocusEventArgs> OnFocus { get; set; }
    [Parameter] public EventCallback<FocusEventArgs> OnBlur { get; set; }

    [Parameter] public RenderFragment<bool>? HandleTemplate { get; set; }

    [Parameter(CaptureUnmatchedValues = true)]
    public Dictionary<string, object>? AdditionalAttributes { get; set; }

    private string RootClass => BuildClass(
        "p-toggleswitch p-component",
        Checked ? "p-toggleswitch-checked" : null,
        Disabled ? "p-disabled" : null,
        Invalid ? "p-invalid" : null,
        StyleClass);

    private async Task HandleChange(ChangeEventArgs e)
    {
        if (Disabled) return;

        var next = (bool)(e.Value ?? false);
        Checked = next;
        await CheckedChanged.InvokeAsync(next);
        await OnChange.InvokeAsync(next);
    }

    private static string BuildClass(params string?[] classes)
        => string.Join(' ', classes.Where(c => !string.IsNullOrWhiteSpace(c)));
}