using Microsoft.Playwright;
using Xunit;
using static Microsoft.Playwright.Assertions;

namespace OpBlazorUI.Playwright;

/// <summary>
/// Bloco 4.1e2 (teclado de MultiSelect e AutoComplete). Usa a página <c>/_tests/bloco4-1e2</c>.
/// </summary>
[Collection("showcase")]
public class Bloco4_1e2Tests
{
    private readonly ShowcaseFixture _fixture;

    public Bloco4_1e2Tests(ShowcaseFixture fixture) => _fixture = fixture;

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
        await page.GotoAsync($"{_fixture.BaseUrl}/_tests/bloco4-1e2", new PageGotoOptions { WaitUntil = WaitUntilState.NetworkIdle });
        await page.Locator("#t-title").WaitForAsync();
        return page;
    }

    [Fact]
    public async Task MultiSelect_Space_alterna_a_opcao_focada()
    {
        var errors = new List<string>();
        var page = await NewPageAsync(errors);

        await page.Locator("#t-ms [role=combobox]").First.FocusAsync();

        // ArrowDown abre e foca a primeira opção.
        await page.Keyboard.PressAsync("ArrowDown");
        await Expect(page.Locator("#t-ms .p-multiselect-overlay")).ToBeVisibleAsync();

        // Space alterna a opção focada.
        await page.Keyboard.PressAsync(" ");
        await Expect(page.Locator("#t-ms-value")).ToContainTextAsync("C0");
        await Expect(page.Locator("#t-ms .p-multiselect-option-selected")).ToHaveCountAsync(1);

        Assert.True(errors.Count == 0, string.Join(" || ", errors));
        await page.CloseAsync();
    }

    [Fact]
    public async Task AutoComplete_move_o_highlight_com_setas()
    {
        var errors = new List<string>();
        var page = await NewPageAsync(errors);

        var input = page.Locator("#t-ac input[role=combobox]");
        await input.FocusAsync();
        await input.FillAsync("Cidade");
        await Expect(page.Locator("#t-ac .p-autocomplete-option").First).ToBeVisibleAsync();

        await page.Keyboard.PressAsync("ArrowDown");
        await Expect(input).ToHaveAttributeAsync("aria-activedescendant", "ac_0");

        await page.Keyboard.PressAsync("ArrowDown");
        await Expect(input).ToHaveAttributeAsync("aria-activedescendant", "ac_1");

        Assert.True(errors.Count == 0, string.Join(" || ", errors));
        await page.CloseAsync();
    }
}
