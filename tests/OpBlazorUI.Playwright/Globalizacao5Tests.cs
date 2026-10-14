using Microsoft.Playwright;
using Xunit;
using static Microsoft.Playwright.Assertions;

namespace OpBlazorUI.Playwright;

/// <summary>
/// Lote 5 da globalização (nav/abas/paginação). Usa a página <c>/_tests/globalizacao-5</c>, que
/// fixa a tradução em pt-BR, e verifica os <c>aria-label</c> de Tabs, TabView, Paginator e Carousel.
/// </summary>
[Collection("showcase")]
public class Globalizacao5Tests
{
    private readonly ShowcaseFixture _fixture;

    public Globalizacao5Tests(ShowcaseFixture fixture) => _fixture = fixture;

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
        await page.GotoAsync($"{_fixture.BaseUrl}/_tests/globalizacao-5", new PageGotoOptions { WaitUntil = WaitUntilState.NetworkIdle });
        await page.Locator("#t-title").WaitForAsync();
        return page;
    }

    [Fact]
    public async Task Tabs_e_TabView_usam_a_traducao_pt_br()
    {
        var errors = new List<string>();
        var page = await NewPageAsync(errors);

        await Expect(page.Locator("#t-tabs .p-tablist-prev-button")).ToHaveAttributeAsync("aria-label", "Anterior");
        await Expect(page.Locator("#t-tabs .p-tablist-next-button")).ToHaveAttributeAsync("aria-label", "Próximo");
        await Expect(page.Locator("#t-tabs .p-tab-close-icon")).ToHaveAttributeAsync("aria-label", "Fechar");

        await Expect(page.Locator("#t-tabview .p-tabview-prev-button")).ToHaveAttributeAsync("aria-label", "Anterior");
        await Expect(page.Locator("#t-tabview .p-tabview-next-button")).ToHaveAttributeAsync("aria-label", "Próximo");

        Assert.True(errors.Count == 0, string.Join(" || ", errors));
        await page.CloseAsync();
    }

    [Fact]
    public async Task Paginator_usa_a_traducao_pt_br()
    {
        var errors = new List<string>();
        var page = await NewPageAsync(errors);

        await Expect(page.Locator("#t-paginator .p-paginator-first")).ToHaveAttributeAsync("aria-label", "Primeira página");
        await Expect(page.Locator("#t-paginator .p-paginator-prev")).ToHaveAttributeAsync("aria-label", "Página anterior");
        await Expect(page.Locator("#t-paginator .p-paginator-next")).ToHaveAttributeAsync("aria-label", "Próxima página");
        await Expect(page.Locator("#t-paginator .p-paginator-last")).ToHaveAttributeAsync("aria-label", "Última página");
        await Expect(page.Locator("#t-paginator .p-paginator-pages button").First).ToHaveAttributeAsync("aria-label", "Página 1");
        await Expect(page.Locator("#t-paginator .p-paginator-jtp-input input")).ToHaveAttributeAsync("aria-label", "Ir para a página (campo)");
        await Expect(page.Locator("#t-paginator .p-paginator-rpp-dropdown")).ToHaveAttributeAsync("aria-label", "Itens por página");

        Assert.True(errors.Count == 0, string.Join(" || ", errors));
        await page.CloseAsync();
    }

    [Fact]
    public async Task Carousel_usa_a_traducao_pt_br()
    {
        var errors = new List<string>();
        var page = await NewPageAsync(errors);

        await Expect(page.Locator("#t-carousel .p-carousel-prev-button")).ToHaveAttributeAsync("aria-label", "Primeira página");
        await Expect(page.Locator("#t-carousel .p-carousel-next-button")).ToHaveAttributeAsync("aria-label", "Próxima página");
        await Expect(page.Locator("#t-carousel .p-carousel-indicator-button").First).ToHaveAttributeAsync("aria-label", "Página 1");

        Assert.True(errors.Count == 0, string.Join(" || ", errors));
        await page.CloseAsync();
    }
}
