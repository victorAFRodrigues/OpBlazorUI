using System.Text.RegularExpressions;
using Microsoft.Playwright;
using Xunit;
using static Microsoft.Playwright.Assertions;

namespace OpBlazorUI.Playwright;

/// <summary>
/// Globalização (ThemeSwitcher). Usa a página <c>/_tests/globalizacao-theme-switcher</c>, que
/// fixa a tradução em pt-BR, e verifica os rótulos do <c>OpThemeSwitcher</c>.
/// </summary>
[Collection("showcase")]
public class GlobalizacaoThemeSwitcherTests
{
    private readonly ShowcaseFixture _fixture;

    public GlobalizacaoThemeSwitcherTests(ShowcaseFixture fixture) => _fixture = fixture;

    [Fact]
    public async Task ThemeSwitcher_usa_a_traducao_pt_br()
    {
        var errors = new List<string>();
        var page = await _fixture.Browser.NewPageAsync();
        page.SetDefaultTimeout(60000);
        page.Console += (_, msg) =>
        {
            if (msg.Type == "error")
            {
                errors.Add(msg.Text);
            }
        };
        await page.GotoAsync($"{_fixture.BaseUrl}/_tests/globalizacao-theme-switcher", new PageGotoOptions { WaitUntil = WaitUntilState.NetworkIdle });
        await page.Locator("#t-title").WaitForAsync();

        // O botão de modo alterna entre os dois rótulos conforme o tema atual.
        var switcher = page.Locator("#t-themeswitcher .op-theme-switcher");
        var modeLabel = await switcher.Locator("button").First.GetAttributeAsync("aria-label");
        Assert.True(modeLabel is "Modo claro" or "Modo escuro", $"aria-label inesperado: '{modeLabel}'");

        // Cada cor de tema tem nome acessível "Tema <nome>".
        await Expect(switcher.Locator("button[aria-label='Tema noir']")).ToHaveCountAsync(1);
        await Expect(switcher.Locator("button[aria-label='Tema emerald']")).ToHaveCountAsync(1);
        await Expect(switcher.Locator("button[aria-label='Tema rose']")).ToHaveCountAsync(1);

        Assert.True(errors.Count == 0, string.Join(" || ", errors));
        await page.CloseAsync();
    }
}
