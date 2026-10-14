using Microsoft.Playwright;
using Xunit;
using static Microsoft.Playwright.Assertions;

namespace OpBlazorUI.Playwright;

/// <summary>
/// Lote 8 da globalização (números/formulários). Usa a página <c>/_tests/globalizacao-8</c>, que
/// fixa a tradução em pt-BR, e verifica separadores e símbolo do <c>OpInputNumber</c> sem
/// <c>CultureInfo</c>.
/// </summary>
[Collection("showcase")]
public class Globalizacao8Tests
{
    private readonly ShowcaseFixture _fixture;

    public Globalizacao8Tests(ShowcaseFixture fixture) => _fixture = fixture;

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
        await page.GotoAsync($"{_fixture.BaseUrl}/_tests/globalizacao-8", new PageGotoOptions { WaitUntil = WaitUntilState.NetworkIdle });
        await page.Locator("#t-title").WaitForAsync();
        return page;
    }

    [Fact]
    public async Task InputNumber_usa_os_separadores_da_traducao()
    {
        var errors = new List<string>();
        var page = await NewPageAsync(errors);

        var input = page.Locator("#t-inputnumber input");
        await input.FillAsync("1.234,56");
        await page.Keyboard.PressAsync("Tab");

        await Expect(page.Locator("#t-number-value")).ToHaveTextAsync("1234.56");
        // No blur o campo é reformatado com os separadores da tradução (pt: "." milhar, "," decimal).
        await Expect(input).ToHaveValueAsync("1.234,56");

        Assert.True(errors.Count == 0, string.Join(" || ", errors));
        await page.CloseAsync();
    }

    [Fact]
    public async Task InputNumber_moeda_usa_o_simbolo_da_traducao()
    {
        var errors = new List<string>();
        var page = await NewPageAsync(errors);

        var input = page.Locator("#t-currency input");
        await input.FillAsync("12,5");
        await page.Keyboard.PressAsync("Tab");

        // Sem Currency, o símbolo vem de OpTranslation.CurrencySymbol ("R$" em pt-BR) — antes vinha
        // da cultura atual. O separador decimal também é o da tradução.
        await Expect(input).ToHaveValueAsync("R$12,50");

        Assert.True(errors.Count == 0, string.Join(" || ", errors));
        await page.CloseAsync();
    }
}
