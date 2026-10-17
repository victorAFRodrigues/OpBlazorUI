using Microsoft.Playwright;
using Xunit;
using static Microsoft.Playwright.Assertions;

namespace OpBlazorUI.Playwright;

/// <summary>
/// Globalização (reordenação). Usa a página <c>/_tests/globalizacao-reordenacao</c>, que fixa
/// a tradução em pt-BR, e verifica os <c>aria-label</c> de OrderList e PickList.
/// </summary>
[Collection("showcase")]
public class GlobalizacaoReordenacaoTests
{
    private readonly ShowcaseFixture _fixture;

    public GlobalizacaoReordenacaoTests(ShowcaseFixture fixture) => _fixture = fixture;

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
        await page.GotoAsync($"{_fixture.BaseUrl}/_tests/globalizacao-reordenacao", new PageGotoOptions { WaitUntil = WaitUntilState.NetworkIdle });
        await page.Locator("#t-title").WaitForAsync();
        return page;
    }

    [Fact]
    public async Task OrderList_usa_a_traducao_pt_br()
    {
        var errors = new List<string>();
        var page = await NewPageAsync(errors);

        var buttons = page.Locator("#t-orderlist .p-orderlist-controls button");
        await Expect(buttons).ToHaveCountAsync(4);
        await Expect(buttons.Nth(0)).ToHaveAttributeAsync("aria-label", "Mover para cima");
        await Expect(buttons.Nth(1)).ToHaveAttributeAsync("aria-label", "Mover para o topo");
        await Expect(buttons.Nth(2)).ToHaveAttributeAsync("aria-label", "Mover para baixo");
        await Expect(buttons.Nth(3)).ToHaveAttributeAsync("aria-label", "Mover para o fim");

        Assert.True(errors.Count == 0, string.Join(" || ", errors));
        await page.CloseAsync();
    }

    [Fact]
    public async Task PickList_usa_a_traducao_pt_br()
    {
        var errors = new List<string>();
        var page = await NewPageAsync(errors);

        // Nos dois painéis (origem e destino), os controles de reordenação repetem os mesmos rótulos.
        await Expect(page.Locator("#t-picklist button[aria-label='Mover para cima']")).ToHaveCountAsync(2);
        await Expect(page.Locator("#t-picklist button[aria-label='Mover para o topo']")).ToHaveCountAsync(2);
        await Expect(page.Locator("#t-picklist button[aria-label='Mover para baixo']")).ToHaveCountAsync(2);
        await Expect(page.Locator("#t-picklist button[aria-label='Mover para o fim']")).ToHaveCountAsync(2);

        // Controles do meio: transferência entre origem e destino.
        await Expect(page.Locator("#t-picklist button[aria-label='Mover para o destino']")).ToHaveCountAsync(1);
        await Expect(page.Locator("#t-picklist button[aria-label='Mover tudo para o destino']")).ToHaveCountAsync(1);
        await Expect(page.Locator("#t-picklist button[aria-label='Mover tudo para a origem']")).ToHaveCountAsync(1);
        await Expect(page.Locator("#t-picklist button[aria-label='Mover para a origem']")).ToHaveCountAsync(1);

        Assert.True(errors.Count == 0, string.Join(" || ", errors));
        await page.CloseAsync();
    }
}
