using Microsoft.Playwright;
using Xunit;
using static Microsoft.Playwright.Assertions;

namespace OpBlazorUI.Playwright;

/// <summary>
/// Bloco 4.1e3/4.1f2 (CascadeSelect, SplitButton, SpeedDial). Usa <c>/_tests/bloco4-1e3f2</c>.
/// </summary>
[Collection("showcase")]
public class Bloco4_1e3f2Tests
{
    private readonly ShowcaseFixture _fixture;

    public Bloco4_1e3f2Tests(ShowcaseFixture fixture) => _fixture = fixture;

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
        await page.GotoAsync($"{_fixture.BaseUrl}/_tests/bloco4-1e3f2", new PageGotoOptions { WaitUntil = WaitUntilState.NetworkIdle });
        await page.Locator("#t-title").WaitForAsync();
        return page;
    }

    [Fact]
    public async Task CascadeSelect_expoe_aria_activedescendant()
    {
        var errors = new List<string>();
        var page = await NewPageAsync(errors);

        var combo = page.Locator("#t-cascade [role=combobox]");
        await combo.FocusAsync();
        await page.Keyboard.PressAsync("ArrowDown");

        await Expect(combo).ToHaveAttributeAsync("aria-activedescendant", "cs_focused");
        await Expect(page.Locator("#t-cascade li#cs_focused")).ToHaveCountAsync(1);

        Assert.True(errors.Count == 0, string.Join(" || ", errors));
        await page.CloseAsync();
    }

    [Fact]
    public async Task SplitButton_navega_por_teclado()
    {
        var errors = new List<string>();
        var page = await NewPageAsync(errors);

        await page.Locator("#t-split .p-splitbutton-dropdown").FocusAsync();
        await page.Keyboard.PressAsync("ArrowDown");
        await Expect(page.Locator("#t-split .p-tieredmenu-root-list")).ToBeVisibleAsync();

        // O menu abre com o foco no primeiro item; seta desce para o próximo.
        await ExpectActiveTextAsync(page, "Salvar");
        await page.Keyboard.PressAsync("ArrowDown");
        await ExpectActiveTextAsync(page, "Excluir");

        Assert.True(errors.Count == 0, string.Join(" || ", errors));
        await page.CloseAsync();
    }

    [Fact]
    public async Task SpeedDial_navega_por_teclado()
    {
        var errors = new List<string>();
        var page = await NewPageAsync(errors);

        await page.Locator("#t-speed .p-speeddial-button").FocusAsync();
        await page.Keyboard.PressAsync("ArrowDown");
        await Expect(page.Locator("#t-speed .p-speeddial-open")).ToHaveCountAsync(1);

        // Abre com o foco na primeira ação; seta desce para a próxima.
        await ExpectActiveActionLabelAsync(page, "Copiar");
        await page.Keyboard.PressAsync("ArrowDown");
        await ExpectActiveActionLabelAsync(page, "Colar");

        Assert.True(errors.Count == 0, string.Join(" || ", errors));
        await page.CloseAsync();
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

    private static async Task ExpectActiveActionLabelAsync(IPage page, string label)
    {
        for (var i = 0; i < 50; i++)
        {
            var active = await page.EvaluateAsync<string>("() => (document.activeElement && document.activeElement.getAttribute('aria-label')) || ''");
            if (active == label)
            {
                return;
            }

            await Task.Delay(100);
        }

        Assert.Equal(label, await page.EvaluateAsync<string>("() => (document.activeElement && document.activeElement.getAttribute('aria-label')) || ''"));
    }
}
