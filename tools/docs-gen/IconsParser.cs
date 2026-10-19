using System.Text.RegularExpressions;

namespace OpBlazorUI.DocsGen;

/// <summary>Lista os glyphs `pi-*` distribuídos com a lib (openng-icons.css).</summary>
internal static class IconsParser
{
    private static readonly Regex IconRegex = new("\\.pi-([a-z0-9-]+)", RegexOptions.Compiled);

    public static IReadOnlyList<string> Load(string cssPath)
    {
        var icons = IconRegex.Matches(File.ReadAllText(cssPath))
            .Select(m => "pi-" + m.Groups[1].Value)
            .Distinct(StringComparer.Ordinal)
            .OrderBy(i => i, StringComparer.Ordinal)
            .ToList();

        if (icons.Count == 0)
        {
            throw new InvalidOperationException($"Nenhum ícone encontrado em {cssPath}.");
        }

        return icons;
    }
}
