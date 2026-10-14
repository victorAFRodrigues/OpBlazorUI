using OpBlazorUI.Components.Button;

namespace OpBlazorUI.Components.Common;

/// <summary>
/// Lê os dicionários de "props" dos botões das confirmações (<c>AcceptButtonProps</c>/
/// <c>RejectButtonProps</c>), na mesma forma do <c>ButtonProps</c> do Optimus UI/PrimeNG, e devolve
/// os valores para os parâmetros do <see cref="OpButton"/>. Mantém o <c>OpConfirmDialog</c> e o
/// <c>OpConfirmPopup</c> alinhados no mapeamento.
/// </summary>
/// <remarks>
/// As chaves são lidas sem diferenciar maiúsculas/minúsculas e valores ausentes ou inválidos caem no
/// padrão do <see cref="OpButton"/> (sem variante). O upstream não define severidade padrão para o
/// rejeitar: quem chama configura (ex.: <c>severity = "secondary"</c>, <c>outlined = true</c>).
/// </remarks>
internal static class OpButtonProps
{
    public static string? GetSeverity(Dictionary<string, object>? props) => GetString(props, "severity");

    public static string? GetVariant(Dictionary<string, object>? props) => GetString(props, "variant");

    public static bool GetOutlined(Dictionary<string, object>? props) => GetBool(props, "outlined");

    public static bool GetText(Dictionary<string, object>? props) => GetBool(props, "text");

    public static bool GetRaised(Dictionary<string, object>? props) => GetBool(props, "raised");

    public static bool GetRounded(Dictionary<string, object>? props) => GetBool(props, "rounded");

    private static string? GetString(Dictionary<string, object>? props, string key) =>
        TryGet(props, key, out var value) ? value?.ToString() : null;

    private static bool GetBool(Dictionary<string, object>? props, string key) =>
        TryGet(props, key, out var value) && value is bool flag && flag;

    // As chaves do ButtonProps do upstream são camelCase ("outlined"); aceitamos qualquer caixa.
    private static bool TryGet(Dictionary<string, object>? props, string key, out object? value)
    {
        if (props is null)
        {
            value = null;
            return false;
        }

        if (props.TryGetValue(key, out value))
        {
            return true;
        }

        foreach (var pair in props)
        {
            if (string.Equals(pair.Key, key, StringComparison.OrdinalIgnoreCase))
            {
                value = pair.Value;
                return true;
            }
        }

        value = null;
        return false;
    }
}
