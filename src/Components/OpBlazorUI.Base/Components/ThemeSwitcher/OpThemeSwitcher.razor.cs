using Microsoft.AspNetCore.Components;
using OpBlazorUI.Base.Services;

namespace OpBlazorUI.Base.Components.ThemeSwitcher;

public sealed record OpThemeOption(string Name, string Hex);

/// <summary>
/// Seletor de cor primária e botão de modo escuro. Lê e grava o <see cref="OpThemeService"/>, então
/// fica sincronizado com qualquer outro controle de tema da aplicação.
/// </summary>
public partial class OpThemeSwitcher : ComponentBase, IDisposable
{
    [Parameter] public string? StyleClass { get; set; }

    [Parameter(CaptureUnmatchedValues = true)]
    public Dictionary<string, object>? AdditionalAttributes { get; set; }

    [Parameter] public EventCallback<string> OnThemeChanged { get; set; }

    [Parameter] public EventCallback<bool> OnDarkModeChanged { get; set; }

    [Inject] private OpThemeService Theme { get; set; } = default!;

    private bool _darkMode => Theme.DarkMode;

    /// <summary>Primárias exibidas (a amostra usa o tom 500; noir usa preto).</summary>
    public static readonly IReadOnlyList<OpThemeOption> Themes = new List<OpThemeOption>
    {
        new("noir", "#0a0a0a"),
        new("emerald", "#10b981"),
        new("green", "#22c55e"),
        new("lime", "#84cc16"),
        new("orange", "#f97316"),
        new("amber", "#f59e0b"),
        new("yellow", "#eab308"),
        new("teal", "#14b8a6"),
        new("cyan", "#06b6d4"),
        new("sky", "#0ea5e9"),
        new("blue", "#3b82f6"),
        new("indigo", "#6366f1"),
        new("violet", "#8b5cf6"),
        new("purple", "#a855f7"),
        new("fuchsia", "#d946ef"),
        new("pink", "#ec4899"),
        new("rose", "#f43f5e")
    };

    protected override void OnInitialized() => Theme.Changed += OnServiceChanged;

    private void OnServiceChanged() => InvokeAsync(StateHasChanged);

    private async Task SetTheme(OpThemeOption theme)
    {
        Theme.SetPrimary(theme.Name);
        await OnThemeChanged.InvokeAsync(theme.Name);
    }

    private async Task ToggleDarkMode()
    {
        Theme.ToggleDarkMode();
        await OnDarkModeChanged.InvokeAsync(Theme.DarkMode);
    }

    public void Dispose() => Theme.Changed -= OnServiceChanged;
}
