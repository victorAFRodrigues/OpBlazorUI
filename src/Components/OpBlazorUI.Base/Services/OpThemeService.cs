namespace OpBlazorUI.Base.Services;

/// <summary>Configuração inicial do tema, definida em <c>AddOpBlazorUI(o =&gt; ...)</c>.</summary>
public sealed class OpThemeOptions
{
    /// <summary><c>aura</c>, <c>lara</c> ou <c>nora</c>.</summary>
    public string Preset { get; set; } = OpThemeService.DefaultPreset;

    /// <summary>Primária inicial; <c>null</c> usa a do preset (emerald no Aura).</summary>
    public string? Primary { get; set; } = OpThemeService.DefaultPrimary;

    /// <summary>Superfície inicial; <c>null</c> usa a do preset (slate no claro, zinc no escuro).</summary>
    public string? Surface { get; set; }

    public bool Rtl { get; set; }

    /// <summary>
    /// Salva as escolhas do usuário no <c>localStorage</c> (chave <c>op-theme</c>) e as restaura no
    /// próximo acesso. Para aplicar antes do Blazor iniciar (sem flash), inclua o script
    /// <c>_content/OpBlazorUI.Base/op-theme.js</c> no &lt;head&gt;.
    /// </summary>
    public bool PersistTheme { get; set; }
}

/// <summary>
/// Estado do tema por usuário (scoped: cada circuito do Blazor Server tem o seu). Preset, paletas,
/// modo escuro e RTL são aplicados no documento pelo <c>&lt;OpBlazorUiSetup /&gt;</c>.
/// </summary>
public sealed class OpThemeService
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

    public static IReadOnlyList<string> Surfaces { get; } =
        ["slate", "gray", "zinc", "neutral", "stone", "soho", "viva", "ocean"];

    public const string PalettesUrl = "_content/OpBlazorUI.Base/themes/palettes.css";

    public OpThemeService(OpThemeOptions options)
    {
        Persist = options.PersistTheme;
        Preset = Validate(options.Preset, Presets, nameof(options.Preset))!;
        Primary = Validate(options.Primary, Primaries, nameof(options.Primary));
        Surface = Validate(options.Surface, Surfaces, nameof(options.Surface));
        Rtl = options.Rtl;
    }

    /// <summary>Disparado a cada mudança (inclusive ao restaurar o estado salvo).</summary>
    public event Action? Changed;

    public bool Persist { get; }

    public string Preset { get; private set; }

    public string? Primary { get; private set; }

    public string? Surface { get; private set; }

    public bool DarkMode { get; private set; }

    public bool Rtl { get; private set; }

    public string ThemeUrl => $"_content/OpBlazorUI.Base/themes/{Preset}.css";

    public void SetPreset(string preset) => Update(() => Preset = Validate(preset, Presets, nameof(preset))!);

    public void SetPrimary(string? primary) => Update(() => Primary = Validate(primary, Primaries, nameof(primary)));

    public void SetSurface(string? surface) => Update(() => Surface = Validate(surface, Surfaces, nameof(surface)));

    public void SetDarkMode(bool dark) => Update(() => DarkMode = dark);

    public void ToggleDarkMode() => SetDarkMode(!DarkMode);

    public void SetRtl(bool rtl) => Update(() => Rtl = rtl);

    /// <summary>Restaura um estado (salvo ou lido do documento). Valores inválidos são ignorados.</summary>
    internal void Restore(string? preset, string? primary, string? surface, bool? dark, bool? rtl, bool hasPalette)
    {
        if (preset is not null && Presets.Contains(preset)) Preset = preset;
        if (hasPalette)
        {
            if (primary is null || Primaries.Contains(primary)) Primary = primary;
            if (surface is null || Surfaces.Contains(surface)) Surface = surface;
        }

        if (dark is not null) DarkMode = dark.Value;
        if (rtl is not null) Rtl = rtl.Value;
        Changed?.Invoke();
    }

    private void Update(Action apply)
    {
        var before = (Preset, Primary, Surface, DarkMode, Rtl);
        apply();
        if (before != (Preset, Primary, Surface, DarkMode, Rtl)) Changed?.Invoke();
    }

    private static string? Validate(string? value, IReadOnlyList<string> allowed, string name)
    {
        if (value is not null && !allowed.Contains(value))
            throw new ArgumentException($"Valor desconhecido: {value}. Use: {string.Join(", ", allowed)}.", name);
        return value;
    }
}
