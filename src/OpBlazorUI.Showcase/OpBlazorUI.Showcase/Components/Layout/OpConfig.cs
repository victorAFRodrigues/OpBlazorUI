using System.Text;

namespace OpBlazorUI.Showcase.Components.Layout;

public sealed record OpPalette(string Name, string Swatch, IReadOnlyList<string> Ramp);

public static class OpConfig
{
    // 16 cores + "noir" (noir usa a surface como primary)
    private static readonly string[] PrimaryNames =
        ["noir", "emerald", "green", "lime", "orange", "amber", "yellow", "teal",
         "cyan", "sky", "blue", "indigo", "violet", "purple", "fuchsia", "pink", "rose"];

    private static readonly string[][] PrimaryRamps =
    [
        [],
        ["#ecfdf5", "#d1fae5", "#a7f3d0", "#6ee7b7", "#34d399", "#10b981", "#059669", "#047857", "#065f46", "#064e3b", "#022c22"], // emerald
        ["#f0fdf4", "#dcfce7", "#bbf7d0", "#86efac", "#4ade80", "#22c55e", "#16a34a", "#15803d", "#166534", "#14532d", "#052e16"], // green
        ["#f7fee7", "#ecfccb", "#d9f99d", "#bef264", "#a3e635", "#84cc16", "#65a30d", "#4d7c0f", "#3f6212", "#365314", "#1a2e05"], // lime
        ["#fff7ed", "#ffedd5", "#fed7aa", "#fdba74", "#fb923c", "#f97316", "#ea580c", "#c2410c", "#9a3412", "#7c2d12", "#431407"], // orange
        ["#fffbeb", "#fef3c7", "#fde68a", "#fcd34d", "#fbbf24", "#f59e0b", "#d97706", "#b45309", "#92400e", "#78350f", "#451a03"], // amber
        ["#fefce8", "#fef9c3", "#fef08a", "#fde047", "#facc15", "#eab308", "#ca8a04", "#a16207", "#854d0e", "#713f12", "#422006"], // yellow
        ["#f0fdfa", "#ccfbf1", "#99f6e4", "#5eead4", "#2dd4bf", "#14b8a6", "#0d9488", "#0f766e", "#115e59", "#134e4a", "#042f2e"], // teal
        ["#ecfeff", "#cffafe", "#a5f3fc", "#67e8f9", "#22d3ee", "#06b6d4", "#0891b2", "#0e7490", "#155e75", "#164e63", "#083344"], // cyan
        ["#f0f9ff", "#e0f2fe", "#bae6fd", "#7dd3fc", "#38bdf8", "#0ea5e9", "#0284c7", "#0369a1", "#075985", "#0c4a6e", "#082f49"], // sky
        ["#eff6ff", "#dbeafe", "#bfdbfe", "#93c5fd", "#60a5fa", "#3b82f6", "#2563eb", "#1d4ed8", "#1e40af", "#1e3a8a", "#172554"], // blue
        ["#eef2ff", "#e0e7ff", "#c7d2fe", "#a5b4fc", "#818cf8", "#6366f1", "#4f46e5", "#4338ca", "#3730a3", "#312e81", "#1e1b4b"], // indigo
        ["#f5f3ff", "#ede9fe", "#ddd6fe", "#c4b5fd", "#a78bfa", "#8b5cf6", "#7c3aed", "#6d28d9", "#5b21b6", "#4c1d95", "#2e1065"], // violet
        ["#faf5ff", "#f3e8ff", "#e9d5ff", "#d8b4fe", "#c084fc", "#a855f7", "#9333ea", "#7e22ce", "#6b21a8", "#581c87", "#3b0764"], // purple
        ["#fdf4ff", "#fae8ff", "#f5d0fe", "#f0abfc", "#e879f9", "#d946ef", "#c026d3", "#a21caf", "#86198f", "#701a75", "#4a044e"], // fuchsia
        ["#fdf2f8", "#fce7f3", "#fbcfe8", "#f9a8d4", "#f472b6", "#ec4899", "#db2777", "#be185d", "#9d174d", "#831843", "#500724"], // pink
        ["#fff1f2", "#ffe4e6", "#fecdd3", "#fda4af", "#fb7185", "#f43f5e", "#e11d48", "#be123c", "#9f1239", "#881337", "#4c0519"], // rose
    ];

    private static readonly string[] SurfaceNames =
        ["slate", "gray", "zinc", "neutral", "stone", "soho", "viva", "ocean"];

    private static readonly string[][] SurfaceRamps =
    [
        ["#ffffff", "#f8fafc", "#f1f5f9", "#e2e8f0", "#cbd5e1", "#94a3b8", "#64748b", "#475569", "#334155", "#1e293b", "#0f172a", "#020617"], // slate
        ["#ffffff", "#f9fafb", "#f3f4f6", "#e5e7eb", "#d1d5db", "#9ca3af", "#6b7280", "#4b5563", "#374151", "#1f2937", "#111827", "#030712"], // gray
        ["#ffffff", "#fafafa", "#f4f4f5", "#e4e4e7", "#d4d4d8", "#a1a1aa", "#71717a", "#52525b", "#3f3f46", "#27272a", "#18181b", "#09090b"], // zinc
        ["#ffffff", "#fafafa", "#f5f5f5", "#e5e5e5", "#d4d4d4", "#a3a3a3", "#737373", "#525252", "#404040", "#262626", "#171717", "#0a0a0a"], // neutral
        ["#ffffff", "#fafaf9", "#f5f5f4", "#e7e5e4", "#d6d3d1", "#a8a29e", "#78716c", "#57534e", "#44403c", "#292524", "#1c1917", "#0c0a09"], // stone
        ["#ffffff", "#ececec", "#dedfdf", "#c4c4c6", "#adaeb0", "#97979b", "#7f8084", "#6a6b70", "#55565b", "#3f4046", "#2c2c34", "#16161d"], // soho
        ["#ffffff", "#f3f3f3", "#e7e7e8", "#cfd0d0", "#b7b8b9", "#9fa1a1", "#87898a", "#6e7173", "#565a5b", "#3e4244", "#262b2c", "#0e1315"], // viva
        ["#ffffff", "#fbfcfc", "#F7F9F8", "#EFF3F2", "#DADEDD", "#B1B7B6", "#828787", "#5F7274", "#415B61", "#29444E", "#183240", "#0c1920"], // ocean
    ];

    public const string DefaultPrimary = "noir";
    public const string DefaultSurface = "slate";

    private static string _primary = DefaultPrimary;
    private static string _surface = DefaultSurface;
    private static bool _rtl;

    public static event Action? ConfigChanged;

    public static string Primary => _primary;
    public static string Surface => _surface;
    public static bool Rtl => _rtl;

    public static IReadOnlyList<OpPalette> PrimaryPalettes { get; } = BuildPrimaryPalettes();
    public static IReadOnlyList<OpPalette> SurfacePalettes { get; } = BuildSurfacePalettes();

    private static IReadOnlyList<OpPalette> BuildPrimaryPalettes()
    {
        var list = new List<OpPalette>();
        for (var i = 0; i < PrimaryNames.Length; i++)
        {
            var name = PrimaryNames[i];
            var swatch = i == 0 ? "var(--text-color)" : PrimaryRamps[i][4]; // 500
            list.Add(new OpPalette(name, swatch, PrimaryRamps[i]));
        }
        return list;
    }

    private static IReadOnlyList<OpPalette> BuildSurfacePalettes()
    {
        var list = new List<OpPalette>();
        for (var i = 0; i < SurfaceNames.Length; i++)
        {
            list.Add(new OpPalette(SurfaceNames[i], SurfaceRamps[i][5], SurfaceRamps[i])); // 500
        }
        return list;
    }

    public static void SetPrimary(string name)
    {
        if (string.Equals(_primary, name, StringComparison.Ordinal)) return;
        _primary = name;
        ConfigChanged?.Invoke();
    }

    public static void SetSurface(string name)
    {
        if (string.Equals(_surface, name, StringComparison.Ordinal)) return;
        _surface = name;
        ConfigChanged?.Invoke();
    }

    public static void SetRtl(bool rtl)
    {
        if (_rtl == rtl) return;
        _rtl = rtl;
        ConfigChanged?.Invoke();
    }

    private static string[] GetSurfaceRamp()
    {
        var i = Array.IndexOf(SurfaceNames, _surface);
        return SurfaceRamps[i < 0 ? 0 : i];
    }

    private static string[] GetPrimaryRamp()
    {
        var i = Array.IndexOf(PrimaryNames, _primary);
        if (i <= 0) return GetSurfaceRamp()[1..]; // noir usa a surface (50..950)
        return PrimaryRamps[i];
    }

    private static readonly int[] PrimaryShades = [50, 100, 200, 300, 400, 500, 600, 700, 800, 900, 950];
    private static readonly int[] SurfaceShades = [0, 50, 100, 200, 300, 400, 500, 600, 700, 800, 900, 950];

    public static string BuildCss()
    {
        var primary = GetPrimaryRamp();
        var surface = GetSurfaceRamp();
        var isNoir = string.Equals(_primary, "noir", StringComparison.Ordinal);

        var sb = new StringBuilder();
        sb.Append(":root{");

        for (var i = 0; i < 11; i++)
            sb.Append($"--p-primary-{PrimaryShades[i]}:{primary[i]};");

        sb.Append($"--p-primary-color:{(isNoir ? surface[11] : primary[5])};");
        sb.Append("--p-primary-contrast-color:#ffffff;");
        sb.Append($"--p-primary-hover-color:{(isNoir ? surface[9] : primary[6])};");
        sb.Append($"--p-primary-active-color:{(isNoir ? surface[8] : primary[7])};");

        for (var i = 0; i < 12; i++)
            sb.Append($"--p-surface-{SurfaceShades[i]}:{surface[i]};");

        sb.Append('}');

        sb.Append(".app-dark{");
        sb.Append($"--p-primary-color:{(isNoir ? surface[1] : primary[3])};");
        sb.Append($"--p-primary-contrast-color:{(isNoir ? surface[11] : surface[10])};");
        sb.Append($"--p-primary-hover-color:{(isNoir ? surface[3] : primary[2])};");
        sb.Append($"--p-primary-active-color:{(isNoir ? surface[4] : primary[1])};");
        sb.Append('}');

        return sb.ToString();
    }
}
