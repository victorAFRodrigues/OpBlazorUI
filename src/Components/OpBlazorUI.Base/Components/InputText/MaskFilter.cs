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

        var clean = StripLiterals(value, mask, slotChar);
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

        if (index < clean.Length) result.Append(clean[index..]);

        return result.ToString();
    }

    public static bool IsIncomplete(string? value, string slotChar) => value?.Contains(slotChar) == true;

    private static string StripLiterals(string value, string mask, string slotChar)
    {
        var literals = new HashSet<char>();
        foreach (var c in mask)
        {
            if (c != '?' && !IsPlaceholder(c)) literals.Add(c);
        }

        var sb = new StringBuilder(value.Length);
        foreach (var c in value)
        {
            if (slotChar.Length == 1 && c == slotChar[0]) continue;
            if (literals.Contains(c)) continue;
            sb.Append(c);
        }

        return sb.ToString();
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
