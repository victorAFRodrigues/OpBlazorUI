using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq.Expressions;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;

namespace OpBlazorUI.Base.Components.Forms;

public abstract class OpInputBase<TValue> : InputBase<TValue>
{
    [Parameter] public bool Invalid { get; set; }

    [Parameter] public bool Disabled { get; set; }

    [Parameter] public string? StyleClass { get; set; }

    private bool _valueExpressionInjected;

    protected bool IsInvalid => Invalid
        || CssClass.Contains("invalid", StringComparison.Ordinal)
        || (EditContext is not null && EditContext.GetValidationMessages(FieldIdentifier).Any());

    public override Task SetParametersAsync(ParameterView parameters)
    {
        // O InputBase só exige ValueExpression no primeiro ciclo; sem ele qualquer uso sem
        // @bind-Value lançaria. Injeta um fallback uma única vez para não reconstruir o
        // ParameterView (e alocar expressão) a cada render — comum em checkboxes de listas.
        if (!_valueExpressionInjected
            && (!parameters.TryGetValue<Expression<Func<TValue>>>(nameof(ValueExpression), out var valueExpression)
                || valueExpression is null))
        {
            _valueExpressionInjected = true;

            var forwarded = new Dictionary<string, object?>();
            foreach (var parameter in parameters)
            {
                forwarded[parameter.Name] = parameter.Value;
            }

            forwarded[nameof(ValueExpression)] = (Expression<Func<TValue>>)(() => Value!);
            return base.SetParametersAsync(ParameterView.FromDictionary(forwarded));
        }

        return base.SetParametersAsync(parameters);
    }

    protected override bool TryParseValueFromString(
        string? value,
        [MaybeNullWhen(false)] out TValue result,
        [NotNullWhen(false)] out string? validationErrorMessage)
        => TryParseTypedValue(value, out result, out validationErrorMessage);

    protected virtual bool TryParseTypedValue(string? value, out TValue result, out string? validationErrorMessage)
    {
        if (BindConverter.TryConvertTo<TValue>(value, CultureInfo.CurrentCulture, out var parsed))
        {
            result = parsed;
            validationErrorMessage = null;
            return true;
        }

        result = default!;
        validationErrorMessage = $"The value '{value}' is not valid for {typeof(TValue).Name}.";
        return false;
    }
}
