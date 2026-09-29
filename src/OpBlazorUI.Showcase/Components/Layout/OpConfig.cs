using System.Collections.Generic;
using System.Linq;
using OpBlazorUI.Base.Services;

namespace OpBlazorUI.Showcase.Components.Layout;

public sealed record OpPalette(string Name, string Swatch);

/// <summary>Amostras de cor do configurador. O estado do tema fica no <see cref="OpThemeService"/>.</summary>
public static class OpConfig
{
    // Amostra = tom 500 da paleta; noir usa a cor do texto (preto no claro, branco no escuro).
    private static readonly Dictionary<string, string> PrimarySwatches = new()
    {
        ["noir"] = "var(--text-color)",
        ["emerald"] = "#10b981", ["green"] = "#22c55e", ["lime"] = "#84cc16", ["orange"] = "#f97316",
        ["amber"] = "#f59e0b", ["yellow"] = "#eab308", ["teal"] = "#14b8a6", ["cyan"] = "#06b6d4",
        ["sky"] = "#0ea5e9", ["blue"] = "#3b82f6", ["indigo"] = "#6366f1", ["violet"] = "#8b5cf6",
        ["purple"] = "#a855f7", ["fuchsia"] = "#d946ef", ["pink"] = "#ec4899", ["rose"] = "#f43f5e",
    };

    private static readonly Dictionary<string, string> SurfaceSwatches = new()
    {
        ["slate"] = "#64748b", ["gray"] = "#6b7280", ["zinc"] = "#71717a", ["neutral"] = "#737373",
        ["stone"] = "#78716c", ["soho"] = "#77787d", ["viva"] = "#6e7173", ["ocean"] = "#5f7274",
    };

    public const string DefaultSurface = "slate";

    public static IReadOnlyList<OpPalette> PrimaryPalettes { get; } =
        OpThemeService.Primaries.Select(p => new OpPalette(p, PrimarySwatches[p])).ToList();

    public static IReadOnlyList<OpPalette> SurfacePalettes { get; } =
        OpThemeService.Surfaces.Select(s => new OpPalette(s, SurfaceSwatches[s])).ToList();
}
