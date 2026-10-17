using Microsoft.Playwright;
using Xunit;
using static Microsoft.Playwright.Assertions;

namespace OpBlazorUI.Playwright;

/// <summary>
/// Bloco 4.4: Chart (OpBlazorUI.Charts sobre Blazor-ApexCharts). Usa a página
/// <c>/_tests/bloco4-4</c> do Showcase e a página de documentação <c>/chart</c>.
/// </summary>
[Collection("showcase")]
public class Bloco4_4Tests
{
    private readonly ShowcaseFixture _fixture;

    public Bloco4_4Tests(ShowcaseFixture fixture) => _fixture = fixture;

    private async Task<IPage> NewPageAsync(List<string> consoleErrors, string route = "/_tests/bloco4-4", bool waitForTestTitle = true)
    {
        var page = await _fixture.Browser.NewPageAsync();
        page.SetDefaultTimeout(60000);
        page.Console += (_, msg) =>
        {
            if (msg.Type == "error")
            {
                consoleErrors.Add(msg.Text);
            }
        };
        await page.GotoAsync($"{_fixture.BaseUrl}{route}", new PageGotoOptions { WaitUntil = WaitUntilState.NetworkIdle });
        if (waitForTestTitle)
        {
            await page.Locator("#t-title").WaitForAsync();
        }

        return page;
    }

    /// <summary>Normaliza uma cor CSS para <c>rgb(r, g, b)</c> (descarta o alpha) via navegador.</summary>
    private static Task<string> NormalizeColorAsync(IPage page, string css)
        => page.EvaluateAsync<string>(
            @"c => {
                if (!c) return '';
                const el = document.createElement('span');
                el.style.color = c;
                document.body.appendChild(el);
                const rgb = getComputedStyle(el).color;
                el.remove();
                const m = rgb.match(/rgba?\((\d+),\s*(\d+),\s*(\d+)/);
                return m ? `rgb(${m[1]}, ${m[2]}, ${m[3]})` : rgb;
            }", css);

    private static Task<string?> FirstSeriesFillAsync(IPage page, string sectionSelector)
        => page.EvaluateAsync<string?>(
            "sel => document.querySelector(sel + ' .apexcharts-series [fill]')?.getAttribute('fill')",
            sectionSelector);

    [Fact]
    public async Task Renderiza_o_grafico_apos_a_hidratacao()
    {
        var errors = new List<string>();
        var page = await NewPageAsync(errors);

        await Expect(page.Locator("#t-chart .apexcharts-canvas")).ToBeVisibleAsync();
        await Expect(page.Locator("#t-chart .apexcharts-svg")).ToHaveCountAsync(1);
        await Expect(page.Locator(".op-chart-placeholder")).ToHaveCountAsync(0);

        Assert.True(errors.Count == 0, string.Join(" || ", errors));
        await page.CloseAsync();
    }

    [Fact]
    public async Task Cores_do_tema_sao_aplicadas_na_serie()
    {
        var errors = new List<string>();
        var page = await NewPageAsync(errors);

        await Expect(page.Locator("#t-chart .apexcharts-canvas")).ToBeVisibleAsync();

        // Mesma leitura do op-charts.interop.js: token do tema, com fallback.
        var primaryRaw = await page.EvaluateAsync<string>(
            "getComputedStyle(document.documentElement).getPropertyValue('--p-primary-color').trim()");
        var expected = await NormalizeColorAsync(page, string.IsNullOrWhiteSpace(primaryRaw) ? "#6366f1" : primaryRaw);

        var actualRaw = await FirstSeriesFillAsync(page, "#t-chart");
        Assert.False(string.IsNullOrEmpty(actualRaw), "#t-chart sem fill na primeira série");
        var actual = await NormalizeColorAsync(page, actualRaw!);

        Assert.True(expected == actual,
            $"primeira série: esperado '{expected}' (--p-primary-color '{primaryRaw}'), obtido '{actual}' (raw '{actualRaw}')");

        Assert.True(errors.Count == 0, string.Join(" || ", errors));
        await page.CloseAsync();
    }

    [Fact]
    public async Task UseThemeColors_false_mantem_a_paleta_padrao()
    {
        var errors = new List<string>();
        var page = await NewPageAsync(errors);

        await Expect(page.Locator("#t-no-theme .apexcharts-canvas")).ToBeVisibleAsync();

        // Primeira cor da paleta padrão do ApexCharts.
        var expected = await NormalizeColorAsync(page, "#008FFB");
        var actualRaw = await FirstSeriesFillAsync(page, "#t-no-theme");
        Assert.False(string.IsNullOrEmpty(actualRaw), "#t-no-theme sem fill na primeira série");
        var actual = await NormalizeColorAsync(page, actualRaw!);

        Assert.True(expected == actual,
            $"primeira série: esperado '{expected}' (padrão do ApexCharts), obtido '{actual}' (raw '{actualRaw}')");

        Assert.True(errors.Count == 0, string.Join(" || ", errors));
        await page.CloseAsync();
    }

    [Fact]
    public async Task Atributos_extras_sao_repassados_ao_conteiner()
    {
        var errors = new List<string>();
        var page = await NewPageAsync(errors);

        var container = page.Locator("#t-attrs .op-chart[data-testid='op-chart-attrs']");
        await Expect(container).ToBeVisibleAsync();
        await Expect(container).ToHaveAttributeAsync("aria-label", "Gráfico com atributos");

        Assert.True(errors.Count == 0, string.Join(" || ", errors));
        await page.CloseAsync();
    }

    [Fact]
    public async Task Pagina_de_docs_renderiza_os_demos()
    {
        var errors = new List<string>();
        var page = await NewPageAsync(errors, "/chart", waitForTestTitle: false);

        await Expect(page.Locator(".op-chart").First).ToBeVisibleAsync();
        await Expect(page.Locator(".apexcharts-canvas").First).ToBeVisibleAsync();
        await Expect(page.Locator(".op-chart-placeholder")).ToHaveCountAsync(0);

        Assert.True(errors.Count == 0, string.Join(" || ", errors));
        await page.CloseAsync();
    }
}
