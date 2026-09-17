namespace OpBlazorUI.Base;

public static class OpSeverityIcon
{
    private static readonly Dictionary<string, string> IconMap = new(StringComparer.OrdinalIgnoreCase)
    {
        ["info"] = "pi pi-info-circle",
        ["success"] = "pi pi-check-circle",
        ["warn"] = "pi pi-exclamation-triangle",
        ["error"] = "pi pi-times-circle",
    };

    public static string? Get(string? severity)
        => severity != null && IconMap.TryGetValue(severity, out var icon) ? icon : null;
}