namespace OpBlazorUI.Base;

public static class OpCss
{
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