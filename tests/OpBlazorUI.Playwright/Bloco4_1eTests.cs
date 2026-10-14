using Microsoft.Playwright;
using Xunit;
using static Microsoft.Playwright.Assertions;

namespace OpBlazorUI.Playwright;

/// <summary>
/// Bloco 4.1e (teclado da seleção). Usa a página <c>/_tests/bloco4-1e</c>.
/// </summary>
[Collection("showcase")]
public class Bloco4_1eTests
{
    private readonly ShowcaseFixture _fixture;

    public Bloco4_1eTests(ShowcaseFixture fixture) => _fixture = fixture;

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
        await page.GotoAsync($"{_fixture.BaseUrl}/_tests/bloco4-1e", new PageGotoOptions { WaitUntil = WaitUntilState.NetworkIdle });
        await page.Locator("#t-title").WaitForAsync();
        return page;
    }

    [Fact]
    public async Task Select_Space_seleciona_a_opcao_focada()
    {
        var errors = new List<string>();
        var page = await NewPageAsync(errors);

        await page.Locator("#t-select [role=combobox]").First.FocusAsync();

        // ArrowDown abre e foca a primeira opção.
        await page.Keyboard.PressAsync("ArrowDown");
        await Expect(page.Locator("#t-select .p-select-overlay")).ToBeVisibleAsync();

        // Space seleciona a opção focada.
        await page.Keyboard.PressAsync(" ");

        await Expect(page.Locator("#t-select-value")).ToHaveTextAsync("C01");

        Assert.True(errors.Count == 0, string.Join(" || ", errors));
        await page.CloseAsync();
    }

    [Fact]
    public async Task Listbox_aria_activedescendant_acompanha_o_foco()
    {
        var errors = new List<string>();
        var page = await NewPageAsync(errors);

        var list = page.Locator("#t-listbox ul[role=listbox]");
        await list.FocusAsync();

        await page.Keyboard.PressAsync("ArrowDown");
        await Expect(list).ToHaveAttributeAsync("aria-activedescendant", "lb_0");

        await page.Keyboard.PressAsync("ArrowDown");
        await Expect(list).ToHaveAttributeAsync("aria-activedescendant", "lb_1");

        Assert.True(errors.Count == 0, string.Join(" || ", errors));
        await page.CloseAsync();
    }
}
