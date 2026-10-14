using Microsoft.Playwright;
using Xunit;
using static Microsoft.Playwright.Assertions;

namespace OpBlazorUI.Playwright;

/// <summary>
/// Bloco 4.2a1: templates de coluna, seleção única, paginação controlada e cabeçalho fixo do
/// <c>OpDataTable</c>. Usa a página <c>/_tests/bloco4-2a1</c> do Showcase.
/// </summary>
[Collection("showcase")]
public class Bloco4_2a1Tests
{
    private readonly ShowcaseFixture _fixture;

    public Bloco4_2a1Tests(ShowcaseFixture fixture) => _fixture = fixture;

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
        await page.GotoAsync($"{_fixture.BaseUrl}/_tests/bloco4-2a1", new PageGotoOptions { WaitUntil = WaitUntilState.NetworkIdle });
        await page.Locator("#t-title").WaitForAsync();
        return page;
    }

    [Fact]
    public async Task Templates_renderizam_cabecalho_celula_e_rodape()
    {
        var errors = new List<string>();
        var page = await NewPageAsync(errors);

        await Expect(page.Locator("#t-tmpl .t-hdr")).ToHaveTextAsync("Preço");
        await Expect(page.Locator("#t-tmpl .t-price")).ToHaveCountAsync(3);
        await Expect(page.Locator("#t-tmpl .t-ftr")).ToHaveTextAsync("Total");

        Assert.True(errors.Count == 0, string.Join(" || ", errors));
        await page.CloseAsync();
    }

    [Fact]
    public async Task Selecao_unica_mantem_apenas_uma_linha()
    {
        var errors = new List<string>();
        var page = await NewPageAsync(errors);

        var rows = page.Locator("#t-single .p-datatable-tbody tr");
        await rows.Nth(0).ClickAsync();
        await Expect(page.Locator("#t-single-log")).ToHaveTextAsync("Alface");

        await rows.Nth(2).ClickAsync();
        await Expect(page.Locator("#t-single-log")).ToHaveTextAsync("Banana");
        await Expect(rows.Nth(0)).Not.ToHaveClassAsync(new System.Text.RegularExpressions.Regex("p-datatable-row-selected"));
        await Expect(rows.Nth(2)).ToHaveClassAsync(new System.Text.RegularExpressions.Regex("p-datatable-row-selected"));

        Assert.True(errors.Count == 0, string.Join(" || ", errors));
        await page.CloseAsync();
    }

    [Fact]
    public async Task Paginacao_controlada_atualiza_first()
    {
        var errors = new List<string>();
        var page = await NewPageAsync(errors);

        await Expect(page.Locator("#t-ctrl-log")).ToHaveTextAsync("first=0 rows=5");
        var firstBefore = await page.Locator("#t-ctrl .p-datatable-tbody tr").First.InnerTextAsync();

        await page.Locator("#t-ctrl .p-paginator-next").ClickAsync();
        await Expect(page.Locator("#t-ctrl-log")).ToHaveTextAsync("first=5 rows=5");

        var firstAfter = await page.Locator("#t-ctrl .p-datatable-tbody tr").First.InnerTextAsync();
        Assert.NotEqual(firstBefore, firstAfter);

        Assert.True(errors.Count == 0, string.Join(" || ", errors));
        await page.CloseAsync();
    }

    [Fact]
    public async Task Cabecalho_fixo_aplica_max_height_e_sticky()
    {
        var errors = new List<string>();
        var page = await NewPageAsync(errors);

        var container = page.Locator("#t-sticky .p-datatable-table-container");
        var style = await container.GetAttributeAsync("style");
        Assert.Contains("max-height: 8rem", style);

        var position = await page.Locator("#t-sticky thead").EvaluateAsync<string>(
            "el => getComputedStyle(el).position");
        Assert.Equal("sticky", position);

        Assert.True(errors.Count == 0, string.Join(" || ", errors));
        await page.CloseAsync();
    }
}
