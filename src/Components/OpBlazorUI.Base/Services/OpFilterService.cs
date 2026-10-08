using System.Collections;
using System.Globalization;
using System.Reflection;
using System.Text;

namespace OpBlazorUI.Base.Services;

/// <summary>Modos de comparação suportados pelo <see cref="OpFilterService"/>.</summary>
public enum OpFilterMatchMode
{
    StartsWith = 1,
    Contains = 2,
    NotContains = 3,
    EndsWith = 4,
    Equals = 5,
    NotEquals = 6,
    In = 7,
    Between = 8,
    Lt = 9,
    Lte = 10,
    Gt = 11,
    Gte = 12,
    Is = 13,
    IsNot = 14,
    Before = 15,
    After = 16,
    DateIs = 17,
    DateIsNot = 18,
    DateBefore = 19,
    DateAfter = 20,
    Custom = 21
}

/// <summary>
/// Porte do <c>FilterService</c> do PrimeNG/Optimus UI v21: filtra coleções por um ou mais
/// campos usando um <see cref="_filters">modo de comparação</see>. A comparação de texto ignora
/// acentos e caixa. Modos customizados podem ser adicionados via <see cref="Register(string, Func{object?, object?, CultureInfo, bool})"/>.
/// </summary>
public sealed class OpFilterService
{
    private readonly Dictionary<string, Func<object?, object?, CultureInfo, bool>> _filters;

    public OpFilterService()
    {
        _filters = new Dictionary<string, Func<object?, object?, CultureInfo, bool>>(StringComparer.Ordinal)
        {
            ["startsWith"] = (value, filter, culture) => StringStarts(value, filter, culture),
            ["contains"] = (value, filter, culture) => StringContains(value, filter, culture),
            ["notContains"] = (value, filter, culture) => !StringContains(value, filter, culture),
            ["endsWith"] = (value, filter, culture) => StringEnds(value, filter, culture),
            ["equals"] = (value, filter, culture) => ValueEquals(value, filter, culture),
            ["notEquals"] = (value, filter, culture) => !ValueEquals(value, filter, culture),
            ["in"] = (value, filter, culture) => ValueIn(value, filter, culture),
            ["between"] = (value, filter, culture) => ValueBetween(value, filter, culture),
            ["lt"] = (value, filter, culture) => Compare(value, filter, culture) < 0,
            ["lte"] = (value, filter, culture) => Compare(value, filter, culture) <= 0,
            ["gt"] = (value, filter, culture) => Compare(value, filter, culture) > 0,
            ["gte"] = (value, filter, culture) => Compare(value, filter, culture) >= 0,
            ["is"] = (value, filter, culture) => ValueEquals(value, filter, culture),
            ["isNot"] = (value, filter, culture) => !ValueEquals(value, filter, culture),
            ["before"] = (value, filter, culture) => Compare(value, filter, culture) < 0,
            ["after"] = (value, filter, culture) => Compare(value, filter, culture) > 0,
            ["dateIs"] = (value, filter, culture) => DateOnly(value) == DateOnly(filter),
            ["dateIsNot"] = (value, filter, culture) => DateOnly(value) != DateOnly(filter),
            ["dateBefore"] = (value, filter, culture) => DateCompare(value, filter) < 0,
            ["dateAfter"] = (value, filter, culture) => DateCompare(value, filter) > 0
        };
    }

    /// <summary>Filtra <paramref name="value"/> pelos campos, considerando qualquer um que corresponda.</summary>
    public IReadOnlyList<T> Filter<T>(
        IEnumerable<T>? value,
        IReadOnlyList<string> fields,
        object? filterValue,
        OpFilterMatchMode matchMode,
        CultureInfo? culture = null)
        => Filter(value, fields, filterValue, ToRule(matchMode), culture);

    /// <summary>Filtra <paramref name="value"/> por um modo nomeado (mesmos nomes do upstream).</summary>
    public IReadOnlyList<T> Filter<T>(
        IEnumerable<T>? value,
        IReadOnlyList<string> fields,
        object? filterValue,
        string matchMode,
        CultureInfo? culture = null)
    {
        var result = new List<T>();
        if (value is null || fields is null || fields.Count == 0)
        {
            return result;
        }

        if (!_filters.TryGetValue(matchMode, out var predicate))
        {
            return result;
        }

        var cultureOrDefault = culture ?? CultureInfo.CurrentCulture;

        foreach (var item in value)
        {
            foreach (var field in fields)
            {
                if (predicate(ResolveFieldData(item, field), filterValue, cultureOrDefault))
                {
                    result.Add(item);
                    break;
                }
            }
        }

        return result;
    }

    /// <summary>Adiciona (ou substitui) um modo de comparação customizado.</summary>
    public void Register(string rule, Func<object?, object?, CultureInfo, bool> predicate)
        => _filters[rule] = predicate;

    /// <summary>Adiciona (ou substitui) um modo de comparação customizado.</summary>
    public void Register(OpFilterMatchMode matchMode, Func<object?, object?, CultureInfo, bool> predicate)
        => Register(ToRule(matchMode), predicate);

    /// <summary>Nome do modo (como no upstream) correspondente ao enum.</summary>
    public static string ToRule(OpFilterMatchMode matchMode) => matchMode switch
    {
        OpFilterMatchMode.StartsWith => "startsWith",
        OpFilterMatchMode.Contains => "contains",
        OpFilterMatchMode.NotContains => "notContains",
        OpFilterMatchMode.EndsWith => "endsWith",
        OpFilterMatchMode.Equals => "equals",
        OpFilterMatchMode.NotEquals => "notEquals",
        OpFilterMatchMode.In => "in",
        OpFilterMatchMode.Between => "between",
        OpFilterMatchMode.Lt => "lt",
        OpFilterMatchMode.Lte => "lte",
        OpFilterMatchMode.Gt => "gt",
        OpFilterMatchMode.Gte => "gte",
        OpFilterMatchMode.Is => "is",
        OpFilterMatchMode.IsNot => "isNot",
        OpFilterMatchMode.Before => "before",
        OpFilterMatchMode.After => "after",
        OpFilterMatchMode.DateIs => "dateIs",
        OpFilterMatchMode.DateIsNot => "dateIsNot",
        OpFilterMatchMode.DateBefore => "dateBefore",
        OpFilterMatchMode.DateAfter => "dateAfter",
        OpFilterMatchMode.Custom => "custom",
        _ => "contains"
    };

    // ------------------------------------------------------------ predicados
    private static bool StringStarts(object? value, object? filter, CultureInfo culture)
    {
        if (IsBlank(filter)) return true;
        if (value is null) return false;
        return Normalize(value, culture).StartsWith(Normalize(filter, culture), StringComparison.Ordinal);
    }

    private static bool StringContains(object? value, object? filter, CultureInfo culture)
    {
        if (IsBlank(filter)) return true;
        if (value is null) return false;
        return Normalize(value, culture).Contains(Normalize(filter, culture), StringComparison.Ordinal);
    }

    private static bool StringEnds(object? value, object? filter, CultureInfo culture)
    {
        if (IsBlank(filter)) return true;
        if (value is null) return false;
        return Normalize(value, culture).EndsWith(Normalize(filter, culture), StringComparison.Ordinal);
    }

    private static bool ValueEquals(object? value, object? filter, CultureInfo culture)
    {
        if (IsBlank(filter)) return true;
        if (value is null) return false;
        if (value is DateTime valueDate && filter is DateTime filterDate) return valueDate == filterDate;
        if (Equals(value, filter)) return true;
        return string.Equals(Normalize(value, culture), Normalize(filter, culture), StringComparison.Ordinal);
    }

    private static bool ValueIn(object? value, object? filter, CultureInfo culture)
    {
        if (filter is null) return true;
        if (filter is string || filter is not IEnumerable enumerable) return false;

        var any = false;
        foreach (var candidate in enumerable)
        {
            any = true;
            if (ValueEquals(value, candidate, culture)) return true;
        }

        return !any;
    }

    private static bool ValueBetween(object? value, object? filter, CultureInfo culture)
    {
        var bounds = ToList(filter);
        if (bounds is null || bounds.Count < 2 || bounds[0] is null || bounds[1] is null) return true;
        if (value is null) return false;

        return Compare(value, bounds[0], culture) >= 0 && Compare(value, bounds[1], culture) <= 0;
    }

    // ------------------------------------------------------------ helpers
    private static int Compare(object? value, object? filter, CultureInfo culture)
    {
        if (value is null) return -1;
        if (filter is null) return 1;

        if (value is DateTime valueDate && filter is DateTime filterDate)
        {
            return valueDate.CompareTo(filterDate);
        }

        if (value is IComparable comparable)
        {
            try
            {
                var converted = filter.GetType() == value.GetType()
                    ? filter
                    : Convert.ChangeType(filter, value.GetType(), culture);
                if (converted is IComparable other)
                {
                    return comparable.CompareTo(other);
                }
            }
            catch
            {
                // tipos incompatíveis: cai para a comparação de texto
            }
        }

        return string.CompareOrdinal(Normalize(value, culture), Normalize(filter, culture));
    }

    private static int DateCompare(object? value, object? filter)
    {
        if (value is not DateTime valueDate || filter is not DateTime filterDate) return 0;
        return valueDate.CompareTo(filterDate);
    }

    private static DateTime? DateOnly(object? value)
        => value switch
        {
            DateTime date => date.Date,
            _ => null
        };

    private static List<object?>? ToList(object? filter)
        => filter is IEnumerable enumerable && filter is not string
            ? enumerable.Cast<object?>().ToList()
            : null;

    private static bool IsBlank(object? filter)
        => filter is null || (filter is string text && text.Trim().Length == 0);

    private static string Normalize(object? value, CultureInfo culture)
        => RemoveAccents(value?.ToString() ?? string.Empty).ToLower(culture);

    private static string RemoveAccents(string text)
    {
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

    private static object? ResolveFieldData(object? item, string field)
    {
        if (item is null || string.IsNullOrWhiteSpace(field)) return null;

        object? data = item;
        foreach (var segment in field.Split('.'))
        {
            if (data is null) return null;

            var property = data.GetType().GetProperty(
                segment, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);

            if (property is not null)
            {
                data = property.GetValue(data);
            }
            else if (data is IDictionary dictionary && dictionary.Contains(segment))
            {
                data = dictionary[segment];
            }
            else
            {
                return null;
            }
        }

        return data;
    }
}
