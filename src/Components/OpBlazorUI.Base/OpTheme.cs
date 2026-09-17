namespace OpBlazorUI.Base;

public static class OpTheme
{
    private const string DefaultTheme = "aura.css";

    private static string _themeUrl = $"_content/OpBlazorUI.Base/themes/{DefaultTheme}";

    public static string ThemeUrl => _themeUrl;

    public static event Action? ThemeChanged;

    public static void SetThemeUrl(string url)
    {
        if (string.Equals(_themeUrl, url, StringComparison.Ordinal)) return;
        _themeUrl = url;
        ThemeChanged?.Invoke();
    }
}
