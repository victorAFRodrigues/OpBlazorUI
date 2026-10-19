using System.Text.RegularExpressions;

namespace OpBlazorUI.DocsGen;

/// <summary>
/// Normaliza o nome de uma linha de tabela de API. As páginas às vezes agrupam
/// propriedades relacionadas numa linha só ("OnShow / OnHide") e métodos aparecem
/// com assinatura ("ShowAsync(anchor)") — o audit precisa dos nomes individuais.
/// </summary>
internal static class RowNames
{
    private static readonly Regex SignatureRegex = new(@"\s*\([^)]*\)\s*$", RegexOptions.Compiled);

    private static readonly Regex TypePrefixRegex = new(@"^Op[A-Za-z0-9]+\.", RegexOptions.Compiled);

    public static IEnumerable<string> Split(string cell)
    {
        var text = RepoContext.StripHtml(cell);
        foreach (var part in text.Split('/', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            var name = SignatureRegex.Replace(part, "").Trim();
            name = TypePrefixRegex.Replace(name, "");
            var generic = name.IndexOf('<');
            if (generic > 0)
            {
                name = name[..generic].Trim();
            }

            if (name.Length > 0)
            {
                yield return name;
            }
        }
    }
}
