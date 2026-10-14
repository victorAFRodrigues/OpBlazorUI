using Microsoft.Playwright;
using Xunit;
using static Microsoft.Playwright.Assertions;

namespace OpBlazorUI.Playwright;

/// <summary>
/// Bloco 4.1b (teclado de abas e accordion). Usa a página <c>/_tests/bloco4-1b</c>.
/// </summary>
[Collection("showcase")]
public class Bloco4_1bTests
{
    private readonly ShowcaseFixture _fixture;

    public Bloco4_1bTests(ShowcaseFixture fixture) => _fixture = fixture;

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
        await page.GotoAsync($"{_fixture.BaseUrl}/_tests/bloco4-1b", new PageGotoOptions { WaitUntil = WaitUntilState.NetworkIdle });
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
    public async Task Tabs_move_o_foco_e_pula_desabilitado()
    {
        var errors = new List<string>();
        var page = await NewPageAsync(errors);

        await page.Locator("#t-tabs .p-tab").First.FocusAsync();
        await ExpectActiveTextAsync(page, "Um");

        // "Dois" está desabilitado: ArrowRight vai direto para "Tres".
        await page.Keyboard.PressAsync("ArrowRight");
        await ExpectActiveTextAsync(page, "Tres");
        await Expect(page.Locator("#t-tabs-active")).ToHaveTextAsync("2");

        await page.Keyboard.PressAsync("ArrowRight");
        await ExpectActiveTextAsync(page, "Um");
        await Expect(page.Locator("#t-tabs-active")).ToHaveTextAsync("0");

        Assert.True(errors.Count == 0, string.Join(" || ", errors));
        await page.CloseAsync();
    }

    [Fact]
    public async Task TabView_move_o_foco_e_pula_desabilitado()
    {
        var errors = new List<string>();
        var page = await NewPageAsync(errors);

        await page.Locator("#t-tabview .p-tabview-tab-header").First.FocusAsync();
        await ExpectActiveTextAsync(page, "Alfa");

        await page.Keyboard.PressAsync("ArrowRight");
        await ExpectActiveTextAsync(page, "Gama");
        await Expect(page.Locator("#t-tabview-active")).ToHaveTextAsync("2");

        Assert.True(errors.Count == 0, string.Join(" || ", errors));
        await page.CloseAsync();
    }

    [Fact]
    public async Task Accordion_move_o_foco_entre_cabecalhos()
    {
        var errors = new List<string>();
        var page = await NewPageAsync(errors);

        await page.Locator("#t-accordion .p-accordionheader").First.FocusAsync();
        await ExpectActiveTextAsync(page, "A");

        // "B" está desabilitado: ArrowDown vai direto para "C".
        await page.Keyboard.PressAsync("ArrowDown");
        await ExpectActiveTextAsync(page, "C");

        await page.Keyboard.PressAsync("Home");
        await ExpectActiveTextAsync(page, "A");

        // Enter abre o painel focado.
        await page.Keyboard.PressAsync("Enter");
        await Expect(page.Locator("#t-accordion .p-accordionheader").First).ToHaveAttributeAsync("aria-expanded", "true");

        Assert.True(errors.Count == 0, string.Join(" || ", errors));
        await page.CloseAsync();
    }
}
