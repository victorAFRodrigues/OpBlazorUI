using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq.Expressions;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;
using OpBlazorUI.Globalization;
using OpBlazorUI.Services;

namespace OpBlazorUI.Components.Forms;

public abstract class OpInputBase<TValue> : InputBase<TValue>, IAsyncDisposable
{
    [Parameter] public bool Invalid { get; set; }

    [Parameter] public bool Disabled { get; set; }

    [Parameter] public string? StyleClass { get; set; }

    /// <summary>
    /// Impede o componente de se associar ao <see cref="EditContext"/> do formulário. Use em
    /// instâncias internas (checkbox de MultiSelect/Listbox, etc.) para que não notifiquem um
    /// campo falso ao formulário.
    /// </summary>
    [Parameter] public bool IgnoreEditContext { get; set; }

    [Inject] protected IJSRuntime Js { get; set; } = default!;

    [Inject] private ILoggerFactory LoggerFactory { get; set; } = default!;

    [Inject] private OpTranslationService TranslationService { get; set; } = default!;

    private OpTranslation TrMessages => TranslationService.Translation;

    private OpInterop? _interop;
    private bool _valueExpressionInjected;

    /// <summary>
    /// Módulos JS da biblioteca, com cache por componente. Depois do descarte continua
    /// devolvendo a mesma instância (já descartada), cujas chamadas viram no-op.
    /// </summary>
    protected OpInterop Interop => _interop ??= new OpInterop(Js, LoggerFactory.CreateLogger(GetType()));

    /// <remarks>
    /// Com <see cref="IAsyncDisposable"/> o Blazor chama só este método, nunca o
    /// <c>IDisposable.Dispose()</c> do <see cref="InputBase{TValue}"/>. Por isso ele é chamado
    /// aqui: é o que desinscreve o componente do <c>OnValidationStateChanged</c> do
    /// <see cref="EditContext"/> e executa o <c>Dispose(bool)</c>.
    /// </remarks>
    public virtual async ValueTask DisposeAsync()
    {
        ((IDisposable)this).Dispose();

        _interop ??= new OpInterop(Js);
        await _interop.DisposeAsync();
    }

    protected bool IsInvalid => Invalid
        || CssClass.Contains("invalid", StringComparison.Ordinal)
        || (EditContext is not null && EditContext.GetValidationMessages(FieldIdentifier).Any());

    public override Task SetParametersAsync(ParameterView parameters)
    {
        // O InputBase só exige ValueExpression no primeiro ciclo; sem ele qualquer uso sem
        // @bind-Value lançaria. Define o fallback diretamente na propriedade (antes do base) —
        // reconstruir o ParameterView descartaria os parâmetros cascateados (EditContext).
        if (!_valueExpressionInjected
            && (!parameters.TryGetValue<Expression<Func<TValue>>>(nameof(ValueExpression), out var valueExpression)
                || valueExpression is null))
        {
            _valueExpressionInjected = true;
            ValueExpression = () => Value!;
        }

        if (!IgnoreEditContext)
        {
            return base.SetParametersAsync(parameters);
        }

        // Instâncias internas pedem para não participar do EditContext (senão notificam um campo
        // falso, cujo FieldIdentifier aponta para o próprio componente). Remove só o cascateado.
        var forwarded = new Dictionary<string, object?>();
        foreach (var parameter in parameters)
        {
            if (parameter.Name == "CascadedEditContext")
            {
                continue;
            }

            forwarded[parameter.Name] = parameter.Value;
        }

        if (ValueExpression is not null)
        {
            forwarded[nameof(ValueExpression)] = ValueExpression;
        }

        return base.SetParametersAsync(ParameterView.FromDictionary(forwarded));
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
        validationErrorMessage = string.Format(TrMessages.InvalidValue, value, typeof(TValue).Name);
        return false;
    }
}
