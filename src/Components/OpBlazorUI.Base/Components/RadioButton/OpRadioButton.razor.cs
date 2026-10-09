using System.Linq.Expressions;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Components.Web;

namespace OpBlazorUI.Base.Components.RadioButton;

public partial class OpRadioButton<TValue> : ComponentBase
{
    [Parameter] public TValue? Value { get; set; }
    [Parameter] public TValue? ModelValue { get; set; }
    [Parameter] public EventCallback<TValue?> ModelValueChanged { get; set; }
    [Parameter] public Expression<Func<TValue?>>? ModelValueExpression { get; set; }
    [Parameter] public string? Name { get; set; }
    [Parameter] public string? Id { get; set; }
    [Parameter] public string? AriaLabel { get; set; }
    [Parameter] public string? AriaLabelledBy { get; set; }
    [Parameter] public bool Required { get; set; }
    [Parameter] public bool Disabled { get; set; }
    [Parameter] public bool Invalid { get; set; }
    [Parameter] public string? Size { get; set; }
    [Parameter] public string Variant { get; set; } = "outlined";
    [Parameter] public int TabIndex { get; set; }
    [Parameter] public string? StyleClass { get; set; }

    [Parameter] public EventCallback<TValue?> OnChange { get; set; }
    [Parameter] public EventCallback<FocusEventArgs> OnFocus { get; set; }
    [Parameter] public EventCallback<FocusEventArgs> OnBlur { get; set; }

    [Parameter(CaptureUnmatchedValues = true)]
    public Dictionary<string, object>? AdditionalAttributes { get; set; }

    [CascadingParameter] private EditContext? CascadedEditContext { get; set; }

    private EditContext? _editContext;
    private FieldIdentifier _fieldIdentifier;

    protected override void OnParametersSet()
    {
        // Integra com o EditForm pelo ModelValue (é o campo ligado a @bind-ModelValue). Todos os
        // radios do grupo compartilham o mesmo FieldIdentifier.
        if (CascadedEditContext is not null && !ReferenceEquals(_editContext, CascadedEditContext))
        {
            _editContext = CascadedEditContext;
            if (ModelValueExpression is not null)
            {
                _fieldIdentifier = FieldIdentifier.Create(ModelValueExpression);
            }
        }
    }

    private bool Checked => EqualityComparer<TValue?>.Default.Equals(Value, ModelValue);

    private string RootClass => BuildClass(
        "p-radiobutton p-component",
        Checked ? "p-radiobutton-checked" : null,
        Disabled ? "p-disabled" : null,
        Invalid ? "p-invalid" : null,
        Variant == "filled" ? "p-variant-filled" : null,
        Size == "small" ? "p-radiobutton-sm p-inputfield-sm" : null,
        Size == "large" ? "p-radiobutton-lg p-inputfield-lg" : null,
        StyleClass);

    private async Task HandleChange(ChangeEventArgs _)
    {
        if (Disabled) return;
        ModelValue = Value;
        await ModelValueChanged.InvokeAsync(Value);
        await OnChange.InvokeAsync(Value);
        _editContext?.NotifyFieldChanged(_fieldIdentifier);
    }

    private static string BuildClass(params string?[] classes)
        => string.Join(' ', classes.Where(c => !string.IsNullOrWhiteSpace(c)));
}
