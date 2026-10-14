using System.Globalization;
using System.Text;

namespace OpBlazorUI.Base.Globalization;

/// <summary>
/// Comparação de texto independente da cultura (ordinal): remove acentos e baixa a caixa em vez de
/// usar <see cref="CultureInfo"/>, então o resultado é o mesmo em Server, WebAssembly e SSR — sem
/// depender do ICU da plataforma. É a base dos filtros/buscas internos dos componentes.
/// </summary>
public static class OpText
{
    /// <summary>Remove os acentos (marcas diacríticas) de <paramref name="text"/>.</summary>
    public static string RemoveAccents(string? text)
    {
        if (string.IsNullOrEmpty(text))
        {
            return string.Empty;
        }

        var normalized = text.Normalize(NormalizationForm.FormD);
        var builder = new StringBuilder(normalized.Length);

        foreach (var character in normalized)
        {
            if (CharUnicodeInfo.GetUnicodeCategory(character) != UnicodeCategory.NonSpacingMark)
            {
                builder.Append(character);
            }
        }

        return builder.ToString().Normalize(NormalizationForm.FormC);
    }

    /// <summary>Remove acentos e baixa a caixa (invariante), para comparações ordinais.</summary>
    public static string Normalize(string? text) => RemoveAccents(text).ToLowerInvariant();

    /// <summary>Indica se <paramref name="value"/> começa com <paramref name="prefix"/>, ignorando acentos e caixa.</summary>
    public static bool StartsWith(string? value, string? prefix)
        => Normalize(value).StartsWith(Normalize(prefix), StringComparison.Ordinal);

    /// <summary>Indica se <paramref name="value"/> contém <paramref name="filter"/>, ignorando acentos e caixa.</summary>
    public static bool Contains(string? value, string? filter)
        => Normalize(value).Contains(Normalize(filter), StringComparison.Ordinal);
}
