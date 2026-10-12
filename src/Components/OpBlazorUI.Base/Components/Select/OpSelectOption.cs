using System.Collections;
using System.Reflection;

namespace OpBlazorUI.Base.Components.Select;

/// <summary>
/// Helpers compartilhados para listas de opções (Select, MultiSelect, Listbox, AutoComplete,
/// SelectButton, CascadeSelect).
///
/// Centraliza a leitura de label/valor para que <c>OptionValue</c> seja aplicado de forma
/// consistente (item G da auditoria): a seleção grava o <b>valor</b> da opção e a comparação é
/// feita contra o <c>Value</c> primitivo, sem reaplicar <c>OptionValue</c> sobre ele.
/// </summary>
internal static class OpSelectOption
{
    private const BindingFlags Flags = BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase;

    public static object? GetProperty(object? obj, string? propertyName)
    {
        if (obj is null || string.IsNullOrEmpty(propertyName)) return null;
        return obj.GetType().GetProperty(propertyName, Flags)?.GetValue(obj);
    }

    public static string GetLabel(object? option, string? optionLabel)
    {
        if (option is null) return string.Empty;
        if (string.IsNullOrEmpty(optionLabel)) return option.ToString() ?? string.Empty;
        return GetProperty(option, optionLabel)?.ToString() ?? option.ToString() ?? string.Empty;
    }

    /// <summary>Extrai o valor da opção (propriedade <paramref name="optionValue"/> ou a própria opção).</summary>
    public static object? GetValue(object? option, string? optionValue)
    {
        if (option is null) return null;
        if (string.IsNullOrEmpty(optionValue)) return option;
        return GetProperty(option, optionValue);
    }

    /// <summary>Compara uma opção com o valor de referência (já primitivo).</summary>
    public static bool Matches(object? option, object? value, string? optionValue)
    {
        if (value is null) return false;
        return Equals(GetValue(option, optionValue), value);
    }

    /// <summary>Verifica se a opção está contida em uma lista de valores.</summary>
    public static bool Contains(IEnumerable? values, object? option, string? optionValue)
    {
        if (values is null) return false;
        var ov = GetValue(option, optionValue);
        foreach (var v in values)
        {
            if (Equals(ov, v)) return true;
        }

        return false;
    }

    /// <summary>Converte o valor da opção para <typeparamref name="TValue"/>.</summary>
    public static TValue? ToValue<TValue>(object? option, string? optionValue)
    {
        var ov = GetValue(option, optionValue);
        if (ov is null) return default;
        if (ov is TValue tv) return tv;

        try
        {
            var target = Nullable.GetUnderlyingType(typeof(TValue)) ?? typeof(TValue);
            return (TValue?)Convert.ChangeType(ov, target);
        }
        catch
        {
            return default;
        }
    }
}
