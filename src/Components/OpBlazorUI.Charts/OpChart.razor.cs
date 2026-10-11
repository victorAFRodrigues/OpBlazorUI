using ApexCharts;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace OpBlazorUI.Charts;

/// <summary>
/// Wrapper fino sobre o <c>ApexChart</c> com duas diferenças: exibe um placeholder no prerender
/// estático (o ApexCharts só funciona de forma interativa) e aplica cores/fonte do tema quando
/// nenhuma cor é informada em <see cref="Options"/>.
/// </summary>
public partial class OpChart<TItem> : ComponentBase, IAsyncDisposable where TItem : class
{
    private const string ModulePath = "./_content/OpBlazorUI.Charts/op-charts.interop.js";

    private IJSObjectReference? _module;
    private bool _interactive;
    private ApexChartOptions<TItem>? _options;

    [Inject] private IJSRuntime Js { get; set; } = default!;

    [Parameter] public ApexChartOptions<TItem>? Options { get; set; }

    /// <summary>Altura do gráfico (número em px ou string CSS).</summary>
    [Parameter] public object? Height { get; set; }

    /// <summary>Largura do gráfico (número em px ou string CSS).</summary>
    [Parameter] public object? Width { get; set; }

    [Parameter] public string? Title { get; set; }

    /// <summary>Aplica as cores/fonte do tema quando <see cref="Options"/> não define cores.</summary>
    [Parameter] public bool UseThemeColors { get; set; } = true;

    [Parameter] public string? Style { get; set; }
    [Parameter] public string? StyleClass { get; set; }

    /// <summary>Conteúdo exibido no prerender estático ou quando o JS ainda não está disponível.</summary>
    [Parameter] public RenderFragment? Placeholder { get; set; }

    /// <summary>Séries (<c>ApexPointSeries&lt;TItem&gt;</c> e afins).</summary>
    [Parameter] public RenderFragment? ChildContent { get; set; }

    [Parameter(CaptureUnmatchedValues = true)]
    public Dictionary<string, object>? AdditionalAttributes { get; set; }

    private ApexChartOptions<TItem> EffectiveOptions => _options ??= Options ?? new ApexChartOptions<TItem>();

    private string PlaceholderStyle
    {
        get
        {
            var height = Height switch
            {
                null => "16rem",
                int px => $"{px}px",
                _ => Height.ToString()
            };

            return $"height: {height}; display: flex; align-items: center; justify-content: center; " +
                   "color: var(--p-text-muted-color, #9ca3af); font-size: 2rem;";
        }
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (!firstRender || _interactive)
        {
            return;
        }

        if (UseThemeColors)
        {
            await ApplyThemeAsync();
        }

        _interactive = true;
        StateHasChanged();
    }

    private async Task ApplyThemeAsync()
    {
        var options = EffectiveOptions;
        if (options.Colors is { Count: > 0 })
        {
            return;
        }

        try
        {
            _module ??= await Js.InvokeAsync<IJSObjectReference>("import", ModulePath);
            var theme = await _module.InvokeAsync<OpChartTheme>("getChartTheme");
            if (theme.Colors.Length > 0)
            {
                options.Colors = theme.Colors.ToList();
            }

            options.Chart ??= new Chart();
            options.Chart.ForeColor ??= theme.ForeColor;
            options.Chart.FontFamily ??= theme.FontFamily;
        }
        catch
        {
            // Sem JS (prerender/SSR): segue com as cores padrão do ApexCharts.
        }
    }

    public async ValueTask DisposeAsync()
    {
        if (_module is not null)
        {
            try
            {
                await _module.DisposeAsync();
            }
            catch
            {
                // circuito encerrado
            }
        }
    }

    private sealed class OpChartTheme
    {
        public string[] Colors { get; set; } = Array.Empty<string>();
        public string? ForeColor { get; set; }
        public string? FontFamily { get; set; }
    }
}
