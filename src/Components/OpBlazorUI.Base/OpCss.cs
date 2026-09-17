namespace OpBlazorUI.Base;

public static class OpCss
{
    public static string BuildClass(params string?[] classes)
        => string.Join(' ', classes.Where(c => !string.IsNullOrWhiteSpace(c)));
}