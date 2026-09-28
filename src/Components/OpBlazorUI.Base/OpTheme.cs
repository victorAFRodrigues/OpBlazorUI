namespace OpBlazorUI.Base;

/// <summary>
/// Tema ativo: um preset (arquivo <c>themes/{preset}.css</c>) mais as paletas primária e de
/// superfície, aplicadas por atributo no <c>&lt;html&gt;</c> (<c>data-op-primary</c> /
/// <c>data-op-surface</c>) a partir de <c>themes/palettes.css</c>. O <c>&lt;OpBlazorUiSetup /&gt;</c>
/// aplica as mudanças.
/// </summary>
public static class OpTheme
{
    public const string DefaultPreset = "aura";
    public const string DefaultPrimary = "noir";

    /// <summary>Presets disponíveis (gerados por <c>tools/theme-gen</c>).</summary>
    public static IReadOnlyList<string> Presets { get; } = ["aura", "lara", "nora"];

    /// <summary>Primárias: <c>noir</c> (segue a superfície) e as 16 cores do Tailwind.</summary>
    public static IReadOnlyList<string> Primaries { get; } =
    [
        "noir", "emerald", "green", "lime", "orange", "amber", "yellow", "teal", "cyan",
        "sky", "blue", "indigo", "violet", "purple", "fuchsia", "pink", "rose"
    ];

    /// <summary>Superfícies. <c>null</c> usa a do preset (slate no claro, zinc no escuro).</summary>
    public static IReadOnlyList<string> Surfaces { get; } =
        ["slate", "gray", "zinc", "neutral", "stone", "soho", "viva", "ocean"];

    private static string _preset = DefaultPreset;
    private static string? _primary = DefaultPrimary;
    private static string? _surface;

    public static event Action? ThemeChanged;

    public static string Preset => _preset;

    /// <summary>Primária ativa; <c>null</c> usa a do preset (emerald no Aura).</summary>
    public static string? Primary => _primary;

    public static string? Surface => _surface;

    public static string ThemeUrl => $"_content/OpBlazorUI.Base/themes/{_preset}.css";

    public const string PalettesUrl = "_content/OpBlazorUI.Base/themes/palettes.css";

    public static void SetPreset(string preset)
    {
        if (!Presets.Contains(preset)) throw new ArgumentException($"Preset desconhecido: {preset}", nameof(preset));
        if (string.Equals(_preset, preset, StringComparison.Ordinal)) return;
        _preset = preset;
        ThemeChanged?.Invoke();
    }

    public static void SetPrimary(string? primary)
    {
        if (primary is not null && !Primaries.Contains(primary))
            throw new ArgumentException($"Primária desconhecida: {primary}", nameof(primary));
        if (string.Equals(_primary, primary, StringComparison.Ordinal)) return;
        _primary = primary;
        ThemeChanged?.Invoke();
    }

    public static void SetSurface(string? surface)
    {
        if (surface is not null && !Surfaces.Contains(surface))
            throw new ArgumentException($"Superfície desconhecida: {surface}", nameof(surface));
        if (string.Equals(_surface, surface, StringComparison.Ordinal)) return;
        _surface = surface;
        ThemeChanged?.Invoke();
    }
}
