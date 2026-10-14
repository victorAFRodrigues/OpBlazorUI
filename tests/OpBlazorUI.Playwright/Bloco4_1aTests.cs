using Microsoft.Playwright;
using Xunit;
using static Microsoft.Playwright.Assertions;

namespace OpBlazorUI.Playwright;

/// <summary>
/// Bloco 4.1a (teclado dos menus). Usa a página <c>/_tests/bloco4-1a</c>.
/// </summary>
[Collection("showcase")]
public class Bloco4_1aTests
{
    private readonly ShowcaseFixture _fixture;

    public Bloco4_1aTests(ShowcaseFixture fixture) => _fixture = fixture;

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
        await page.GotoAsync($"{_fixture.BaseUrl}/_tests/bloco4-1a", new PageGotoOptions { WaitUntil = WaitUntilState.NetworkIdle });
        await page.Locator("#t-title").WaitForAsync();
        return page;
    }

    private static async Task ExpectActiveTextAsync(IPage page, string text)
    {
        for (var i = 0; i < 50; i++)
        {
            var active = await page.EvaluateAsync<string>("() => (document.activeElement && document.activeElement.textContent || '').trim()");
            if (active == text)
            {
                return;
            }

            await Task.Delay(100);
        }

        var final = await page.EvaluateAsync<string>("() => (document.activeElement && document.activeElement.textContent || '').trim()");
        Assert.Equal(text, final);
    }

    [Fact]
    public async Task Menubar_navega_abre_e_fecha_por_teclado()
    {
        var errors = new List<string>();
        var page = await NewPageAsync(errors);

        await page.Locator("#t-menubar a[role=menuitem]").First.FocusAsync();
        await ExpectActiveTextAsync(page, "Arquivo");

        await page.Keyboard.PressAsync("ArrowRight");
        await ExpectActiveTextAsync(page, "Editar");

        await page.Keyboard.PressAsync("ArrowLeft");
        await ExpectActiveTextAsync(page, "Arquivo");

        // Enter abre o submenu e move o foco para o primeiro filho.
        await page.Keyboard.PressAsync("Enter");
        await Expect(page.Locator("#t-menubar .p-menubar-submenu")).ToBeVisibleAsync();
        await ExpectActiveTextAsync(page, "Novo");

        // Escape fecha os submenus.
        await page.Keyboard.PressAsync("Escape");
        await Expect(page.Locator("#t-menubar .p-menubar-submenu")).ToHaveCountAsync(0);

        Assert.True(errors.Count == 0, string.Join(" || ", errors));
        await page.CloseAsync();
    }

    [Fact]
    public async Task TieredMenu_navega_abre_e_seleciona_por_teclado()
    {
        var errors = new List<string>();
        var page = await NewPageAsync(errors);

        await page.Locator("#t-tiered a[role=menuitem]").First.FocusAsync();
        await ExpectActiveTextAsync(page, "A");

        await page.Keyboard.PressAsync("ArrowDown");
        await ExpectActiveTextAsync(page, "B");

        // Volta ao início (wrap).
        await page.Keyboard.PressAsync("ArrowDown");
        await ExpectActiveTextAsync(page, "A");

        // Abre A e foca o primeiro filho.
        await page.Keyboard.PressAsync("ArrowRight");
        await ExpectActiveTextAsync(page, "A1");

        await page.Keyboard.PressAsync("ArrowDown");
        await ExpectActiveTextAsync(page, "A2");

        // Fecha o submenu e volta ao pai (sem selecionar o filho).
        await page.Keyboard.PressAsync("ArrowLeft");
        await ExpectActiveTextAsync(page, "A");
        await Expect(page.Locator("#t-tiered .p-tieredmenu-submenu")).ToHaveCountAsync(0);

        // Abre B e seleciona a folha.
        await page.Keyboard.PressAsync("ArrowDown");
        await page.Keyboard.PressAsync("ArrowRight");
        await ExpectActiveTextAsync(page, "B1");
        await page.Keyboard.PressAsync("Enter");

        await Expect(page.Locator("#t-tiered-log")).ToHaveTextAsync("B1");
        await Expect(page.Locator("#t-tiered .p-tieredmenu-submenu")).ToHaveCountAsync(0);

        Assert.True(errors.Count == 0, string.Join(" || ", errors));
        await page.CloseAsync();
    }

    [Fact]
    public async Task Menubar_Home_e_End()
    {
        var errors = new List<string>();
        var page = await NewPageAsync(errors);

        await page.Locator("#t-menubar a[role=menuitem]").First.FocusAsync();
        await page.Keyboard.PressAsync("End");
        await ExpectActiveTextAsync(page, "Editar");

        await page.Keyboard.PressAsync("Home");
        await ExpectActiveTextAsync(page, "Arquivo");

        Assert.True(errors.Count == 0, string.Join(" || ", errors));
        await page.CloseAsync();
    }
}
