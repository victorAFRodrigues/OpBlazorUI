using Microsoft.Playwright;
using Xunit;
using static Microsoft.Playwright.Assertions;

namespace OpBlazorUI.Playwright;

/// <summary>
/// Bloco 4.4: Chart (OpBlazorUI.Charts sobre Blazor-ApexCharts). Usa a página
/// <c>/_tests/bloco4-4</c> do Showcase.
/// </summary>
[Collection("showcase")]
public class Bloco4_4Tests
{
    private readonly ShowcaseFixture _fixture;

    public Bloco4_4Tests(ShowcaseFixture fixture) => _fixture = fixture;

    private async Task<IPage> NewPageAsync(List<string> consoleErrors)
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
        await page.GotoAsync($"{_fixture.BaseUrl}/_tests/bloco4-4", new PageGotoOptions { WaitUntil = WaitUntilState.NetworkIdle });
        await page.Locator("#t-title").WaitForAsync();
        return page;
    }

    [Fact]
    public async Task Renderiza_o_grafico_apos_a_hidratacao()
    {
        var errors = new List<string>();
        var page = await NewPageAsync(errors);

        await Expect(page.Locator("#t-chart .apexcharts-canvas")).ToBeVisibleAsync();
        await Expect(page.Locator("#t-chart .apexcharts-svg")).ToHaveCountAsync(1);

        Assert.True(errors.Count == 0, string.Join(" || ", errors));
        await page.CloseAsync();
    }
}
