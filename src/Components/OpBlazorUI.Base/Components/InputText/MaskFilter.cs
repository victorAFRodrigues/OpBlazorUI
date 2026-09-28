using System.Text;
using System.Text.RegularExpressions;

namespace OpBlazorUI.Base.Components.InputText;

/// <summary>Lógica de máscara e filtro de teclas do InputText.</summary>
internal static class MaskFilter
{
    private static readonly Dictionary<string, string> KeyFilterPresets = new(StringComparer.OrdinalIgnoreCase)
    {
        ["int"] = "[0-9]",
        ["num"] = "[0-9.]",
        ["money"] = "[0-9.,\\s]",
        ["hex"] = "[0-9a-fA-F]",
        ["alpha"] = "[a-zA-Z]",
        ["alphanum"] = "[a-zA-Z0-9]"
    };

    public static string ApplyKeyFilter(string? value, string? keyFilter)
    {
        if (string.IsNullOrEmpty(keyFilter) || string.IsNullOrEmpty(value)) return value ?? string.Empty;

        var pattern = KeyFilterPresets.TryGetValue(keyFilter, out var preset) ? preset : keyFilter;
        Regex regex;
        try
        {
            regex = new Regex(pattern);
        }
        catch
        {
            return value;
        }

        var sb = new StringBuilder(value.Length);
        foreach (var ch in value)
        {
            if (regex.IsMatch(ch.ToString())) sb.Append(ch);
        }

        return sb.ToString();
    }

    public static string ApplyMask(string? value, string? mask, string slotChar)
    {
        if (string.IsNullOrEmpty(mask)) return value ?? string.Empty;
        value ??= string.Empty;

        var clean = ExtractRaw(value, mask, slotChar);
        var result = new StringBuilder();
        var index = 0;
        var optional = false;

        for (var i = 0; i < mask.Length; i++)
        {
            var c = mask[i];
            if (c == '?')
            {
                optional = true;
                continue;
            }

            if (IsPlaceholder(c))
            {
                if (index < clean.Length)
                {
                    var v = clean[index];
                    if (Matches(v, c))
                    {
                        result.Append(v);
                        index++;
                    }
                    else if (optional)
                    {
                        break;
                    }
                    else
                    {
                        result.Append(slotChar);
                    }
                }
                else
                {
                    if (optional) break;
                    result.Append(slotChar);
                }
            }
            else
            {
                if (optional && index >= clean.Length) break;
                result.Append(c);
            }
        }

        return result.ToString();
    }

    public static bool IsIncomplete(string? value, string slotChar) => value?.Contains(slotChar) == true;

    // Alinha o valor com a máscara: um literal só é descartado na posição esperada, então
    // dígitos iguais a literais (ex.: o "5" em "+55 (99) ...") são mantidos. Caracteres que não
    // casam com o próximo placeholder, ou que excedem a máscara, são descartados.
    private static string ExtractRaw(string value, string mask, string slotChar)
    {
        var sb = new StringBuilder(value.Length);
        var mi = 0;
        foreach (var ch in value)
        {
            while (mi < mask.Length && mask[mi] == '?') mi++;
            if (mi < mask.Length && !IsPlaceholder(mask[mi]) && ch == mask[mi])
            {
                mi++;
                continue;
            }

            if (slotChar.Length == 1 && ch == slotChar[0]) continue;

            var next = NextPlaceholder(mask, mi);
            if (next < 0 || !Matches(ch, mask[next])) continue;
            sb.Append(ch);
            mi = next + 1;
        }

        return sb.ToString();
    }

    private static int NextPlaceholder(string mask, int from)
    {
        for (var i = from; i < mask.Length; i++)
        {
            if (IsPlaceholder(mask[i])) return i;
        }

        return -1;
    }

    private static bool IsPlaceholder(char c) => c is '9' or 'a' or '*';

    private static bool Matches(char v, char p) => p switch
    {
        '9' => char.IsDigit(v),
        'a' => char.IsLetter(v),
        '*' => char.IsLetterOrDigit(v),
        _ => false
    };
}
