using Microsoft.Playwright;
using Xunit;
using static Microsoft.Playwright.Assertions;

namespace OpBlazorUI.Playwright;

/// <summary>
/// Bloco 4.1b2 (Steps, TabMenu, PanelMenu, MegaMenu). Usa a página <c>/_tests/bloco4-1b2</c>.
/// </summary>
[Collection("showcase")]
public class Bloco4_1b2Tests
{
    private readonly ShowcaseFixture _fixture;

    public Bloco4_1b2Tests(ShowcaseFixture fixture) => _fixture = fixture;

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
        await page.GotoAsync($"{_fixture.BaseUrl}/_tests/bloco4-1b2", new PageGotoOptions { WaitUntil = WaitUntilState.NetworkIdle });
        await page.Locator("#t-title").WaitForAsync();
        return page;
    }

    private static async Task ExpectActiveTextAsync(IPage page, string text)
    {
        for (var i = 0; i < 50; i++)
        {
            var active = await page.EvaluateAsync<string>("() => ((document.activeElement && document.activeElement.textContent) || '').replace(/\\s+/g, '')");
            if (active == text)
            {
                return;
            }

            await Task.Delay(100);
        }

        Assert.Equal(text, await page.EvaluateAsync<string>("() => ((document.activeElement && document.activeElement.textContent) || '').replace(/\\s+/g, '')"));
    }

    [Fact]
    public async Task Steps_move_o_foco_e_pula_desabilitado()
    {
        var errors = new List<string>();
        var page = await NewPageAsync(errors);

        await page.Locator("#t-steps .p-steps-item-link").First.FocusAsync();
        await ExpectActiveTextAsync(page, "1Um");

        await page.Keyboard.PressAsync("ArrowRight");
        await ExpectActiveTextAsync(page, "3Tres");
        await Expect(page.Locator("#t-steps-active")).ToHaveTextAsync("2");

        Assert.True(errors.Count == 0, string.Join(" || ", errors));
        await page.CloseAsync();
    }

    [Fact]
    public async Task TabMenu_move_o_foco()
    {
        var errors = new List<string>();
        var page = await NewPageAsync(errors);

        await page.Locator("#t-tabmenu .p-tabmenu-item-link").First.FocusAsync();
        await ExpectActiveTextAsync(page, "Alfa");

        await page.Keyboard.PressAsync("ArrowRight");
        await ExpectActiveTextAsync(page, "Beta");

        await page.Keyboard.PressAsync("End");
        await ExpectActiveTextAsync(page, "Gama");

        Assert.True(errors.Count == 0, string.Join(" || ", errors));
        await page.CloseAsync();
    }

    [Fact]
    public async Task PanelMenu_expande_e_seleciona_por_teclado()
    {
        var errors = new List<string>();
        var page = await NewPageAsync(errors);

        await page.Locator("#t-panelmenu .p-panelmenu-header-link").First.FocusAsync();
        await ExpectActiveTextAsync(page, "P1");

        // ArrowRight expande e foca o primeiro filho.
        await page.Keyboard.PressAsync("ArrowRight");
        await ExpectActiveTextAsync(page, "Item1");

        await page.Keyboard.PressAsync("Enter");
        await Expect(page.Locator("#t-panel-log")).ToHaveTextAsync("Item1");

        Assert.True(errors.Count == 0, string.Join(" || ", errors));
        await page.CloseAsync();
    }

    [Fact]
    public async Task MegaMenu_abre_e_seleciona_por_teclado()
    {
        var errors = new List<string>();
        var page = await NewPageAsync(errors);

        await page.Locator("#t-megamenu .p-megamenu-item-link").First.FocusAsync();
        await ExpectActiveTextAsync(page, "A");

        // ArrowRight abre e foca o primeiro item do submenu.
        await page.Keyboard.PressAsync("ArrowRight");
        await ExpectActiveTextAsync(page, "A1");

        await page.Keyboard.PressAsync("Enter");
        await Expect(page.Locator("#t-mega-log")).ToHaveTextAsync("A1");

        Assert.True(errors.Count == 0, string.Join(" || ", errors));
        await page.CloseAsync();
    }
}
