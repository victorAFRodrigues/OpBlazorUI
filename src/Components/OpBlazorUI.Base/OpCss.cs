using System.Globalization;

namespace OpBlazorUI.Base;

public static class OpCss
{
    /// <summary>
    /// Formata um número para CSS ou atributos ARIA: sempre com ponto decimal (cultura
    /// invariante) e até 4 casas. Interpolar um <c>double</c> direto usa a cultura atual e,
    /// em pt-BR, gera <c>37,5%</c>, que o navegador descarta.
    /// </summary>
    public static string Num(double value) => value.ToString("0.####", CultureInfo.InvariantCulture);

    public static string BuildClass(params string?[] classes)
    {
        var seen = new HashSet<string>(StringComparer.Ordinal);
        var result = new List<string>();

        foreach (var entry in classes)
        {
            if (string.IsNullOrWhiteSpace(entry))
            {
                continue;
            }

            foreach (var token in entry.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
            {
                if (seen.Add(token))
                {
                    result.Add(token);
                }
            }
        }

        return string.Join(' ', result);
    }
}