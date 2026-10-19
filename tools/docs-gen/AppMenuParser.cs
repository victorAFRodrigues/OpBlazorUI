using System.Text.RegularExpressions;

namespace OpBlazorUI.DocsGen;

/// <summary>
/// Lê o menu do Showcase (AppMenu.cs) para descobrir rotas, categorias e status
/// dos itens — a navegação é a fonte de verdade de quais páginas existem.
/// </summary>
internal static class AppMenuParser
{
    private static readonly Regex ItemRegex = new(
        "\\b(Item|GroupItem|PlaceholderItem)\\(\\s*\"([^\"]+)\"\\s*(?:,\\s*\"([^\"]+)\")?\\s*\\)",
        RegexOptions.Compiled);

    private static readonly Regex LabelRegex = new(
        "Label\\s*=\\s*\"([^\"]+)\"",
        RegexOptions.Compiled);

    public static List<MenuItem> Parse(string path)
    {
        var items = new List<MenuItem>();
        string? entry = null;
        string? group = null;
        var pendingEntry = false;
        var pendingGroup = false;

        foreach (var line in File.ReadLines(path))
        {
            if (line.Contains("new AppMenuEntry", StringComparison.Ordinal))
            {
                pendingEntry = true;
                group = null;
            }
            else if (line.Contains("new AppMenuGroup", StringComparison.Ordinal))
            {
                pendingGroup = true;
            }

            if (pendingEntry)
            {
                var match = LabelRegex.Match(line);
                if (match.Success)
                {
                    entry = match.Groups[1].Value;
                    pendingEntry = false;
                }
            }

            if (pendingGroup)
            {
                var match = LabelRegex.Match(line);
                if (match.Success)
                {
                    group = match.Groups[1].Value;
                    pendingGroup = false;
                }
            }

            foreach (Match match in ItemRegex.Matches(line))
            {
                var call = match.Groups[1].Value;
                var label = match.Groups[2].Value;
                var route = match.Groups[3].Success ? match.Groups[3].Value : null;
                var status = call == "PlaceholderItem" ? "placeholder" : route is null ? "coming-soon" : "ported";
                var category = group ?? entry ?? "Outros";
                items.Add(new MenuItem(label, route, category, status));
            }
        }

        var ported = items.Count(i => i.Status != "coming-soon");
        if (ported < 90)
        {
            throw new InvalidOperationException(
                $"AppMenu.cs: só {ported} itens com rota foram encontrados (esperado ~98). O parser do menu precisa de ajuste.");
        }

        return items;
    }
}
