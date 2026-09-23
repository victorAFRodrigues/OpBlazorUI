using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace OpBlazorUI.Base.Components.ThemeSwitcher;

public sealed record OpThemeOption(string Name, string? Hex, string File);

public partial class OpThemeSwitcher : ComponentBase, IAsyncDisposable
{
    private const string JsModule = "./_content/OpBlazorUI.Base/optimus.interop.js";
    private Lazy<Task<IJSObjectReference>>? _module;
    private bool _disposed;
    private bool _darkMode;
    private bool _darkModeInitialized;

    [Parameter] public string? StyleClass { get; set; }

    [Parameter] public EventCallback<string> OnThemeChanged { get; set; }

    [Parameter] public EventCallback<bool> OnDarkModeChanged { get; set; }

    [Inject] private IJSRuntime Js { get; set; } = default!;

    public static readonly IReadOnlyList<OpThemeOption> Themes = new List<OpThemeOption>
    {
        new("noir", "#0a0a0a", "aura.css"),
        new("lara", "#8b5cf6", "lara.css"),
        new("emerald", "#10b981", "aura-emerald.css"),
        new("green", "#22c55e", "aura-green.css"),
        new("lime", "#84cc16", "aura-lime.css"),
        new("orange", "#f97316", "aura-orange.css"),
        new("amber", "#f59e0b", "aura-amber.css"),
        new("yellow", "#eab308", "aura-yellow.css"),
        new("teal", "#14b8a6", "aura-teal.css"),
        new("cyan", "#06b6d4", "aura-cyan.css"),
        new("sky", "#0ea5e9", "aura-sky.css"),
        new("blue", "#3b82f6", "aura-blue.css"),
        new("indigo", "#6366f1", "aura-indigo.css"),
        new("violet", "#8b5cf6", "aura-violet.css"),
        new("purple", "#a855f7", "aura-purple.css"),
        new("fuchsia", "#d946ef", "aura-fuchsia.css"),
        new("pink", "#ec4899", "aura-pink.css"),
        new("rose", "#f43f5e", "aura-rose.css")
    };

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender && !_darkModeInitialized)
        {
            _darkModeInitialized = true;
            var module = await GetModuleAsync();
            if (module is not null)
            {
                _darkMode = await module.InvokeAsync<bool>("getDarkMode");
                StateHasChanged();
            }
        }
    }

    private async Task SetTheme(OpThemeOption theme)
    {
        OpTheme.SetThemeUrl($"_content/OpBlazorUI.Base/themes/{theme.File}");
        await OnThemeChanged.InvokeAsync(theme.Name);
    }

    private async Task ToggleDarkMode()
    {
        _darkMode = !_darkMode;
        var module = await GetModuleAsync();
        if (module is not null)
        {
            await module.InvokeVoidAsync("setDarkMode", _darkMode);
        }

        await OnDarkModeChanged.InvokeAsync(_darkMode);
        StateHasChanged();
    }

    private async Task<IJSObjectReference?> GetModuleAsync()
    {
        if (_module is null)
        {
            _module = new Lazy<Task<IJSObjectReference>>(() =>
                Js.InvokeAsync<IJSObjectReference>("import", JsModule).AsTask());
        }

        try
        {
            return await _module.Value;
        }
        catch
        {
            return null;
        }
    }

    public async ValueTask DisposeAsync()
    {
        if (_disposed) return;
        _disposed = true;
        if (_module is { IsValueCreated: true })
        {
            try
            {
                var module = await _module.Value;
                await module.DisposeAsync();
            }
            catch
            {
                // ignore
            }
        }
    }
}