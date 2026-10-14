using Microsoft.Playwright;
using Xunit;
using static Microsoft.Playwright.Assertions;

namespace OpBlazorUI.Playwright;

/// <summary>
/// Bloco 4.2a2: filtro por coluna, filtro global e modo lazy do <c>OpDataTable</c>.
/// Usa a página <c>/_tests/bloco4-2a2</c> do Showcase.
/// </summary>
[Collection("showcase")]
public class Bloco4_2a2Tests
{
    private readonly ShowcaseFixture _fixture;

    public Bloco4_2a2Tests(ShowcaseFixture fixture) => _fixture = fixture;

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
        await page.GotoAsync($"{_fixture.BaseUrl}/_tests/bloco4-2a2", new PageGotoOptions { WaitUntil = WaitUntilState.NetworkIdle });
        await page.Locator("#t-title").WaitForAsync();
        return page;
    }

    [Fact]
    public async Task Filtro_por_coluna_reduz_as_linhas()
    {
        var errors = new List<string>();
        var page = await NewPageAsync(errors);

        var rows = page.Locator("#t-colfilter .p-datatable-tbody tr");
        await Expect(rows).ToHaveCountAsync(4);

        await page.Locator("#t-colfilter .p-datatable-filter-column input").First.FillAsync("Am");
        await Expect(rows).ToHaveCountAsync(1);
        await Expect(rows.First).ToContainTextAsync("Amanda");

        Assert.True(errors.Count == 0, string.Join(" || ", errors));
        await page.CloseAsync();
    }

    [Fact]
    public async Task Filtro_global_usa_os_campos()
    {
        var errors = new List<string>();
        var page = await NewPageAsync(errors);

        var rows = page.Locator("#t-globalfilter .p-datatable-tbody tr");
        await Expect(rows).ToHaveCountAsync(4);

        await page.Locator("#t-globalfilter > input").FillAsync("Bia");
        await Expect(rows).ToHaveCountAsync(1);
        await Expect(rows.First).ToContainTextAsync("Bia");

        Assert.True(errors.Count == 0, string.Join(" || ", errors));
        await page.CloseAsync();
    }

    [Fact]
    public async Task Modo_lazy_pagina_e_dispara_OnLazyLoad()
    {
        var errors = new List<string>();
        var page = await NewPageAsync(errors);

        await Expect(page.Locator("#t-lazy-log")).ToHaveTextAsync("first=0 rows=5");
        var rows = page.Locator("#t-lazy .p-datatable-tbody tr");
        await Expect(rows).ToHaveCountAsync(5);
        await Expect(rows.First).ToContainTextAsync("Item 1");

        await page.Locator("#t-lazy .p-paginator-next").ClickAsync();
        await Expect(page.Locator("#t-lazy-log")).ToHaveTextAsync("first=5 rows=5");
        await Expect(rows.First).ToContainTextAsync("Item 6");

        Assert.True(errors.Count == 0, string.Join(" || ", errors));
        await page.CloseAsync();
    }
}
