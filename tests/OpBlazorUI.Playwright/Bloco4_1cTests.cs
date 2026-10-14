using Microsoft.Playwright;
using Xunit;
using static Microsoft.Playwright.Assertions;

namespace OpBlazorUI.Playwright;

/// <summary>
/// Bloco 4.1c (teclado das árvores). Usa a página <c>/_tests/bloco4-1c</c>.
/// </summary>
[Collection("showcase")]
public class Bloco4_1cTests
{
    private readonly ShowcaseFixture _fixture;

    public Bloco4_1cTests(ShowcaseFixture fixture) => _fixture = fixture;

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
        await page.GotoAsync($"{_fixture.BaseUrl}/_tests/bloco4-1c", new PageGotoOptions { WaitUntil = WaitUntilState.NetworkIdle });
        await page.Locator("#t-title").WaitForAsync();
        return page;
    }

    private static async Task ExpectActiveLabelAsync(IPage page, string label)
    {
        const string js = "() => { const a = document.activeElement; if (!a) return ''; const l = a.querySelector && a.querySelector('.p-tree-node-label'); return ((l ? l.textContent : a.textContent) || '').trim(); }";
        for (var i = 0; i < 50; i++)
        {
            var active = await page.EvaluateAsync<string>(js);
            if (active == label)
            {
                return;
            }

            await Task.Delay(100);
        }

        Assert.Equal(label, await page.EvaluateAsync<string>(js));
    }

    [Fact]
    public async Task Tree_navega_expande_e_seleciona_por_teclado()
    {
        var errors = new List<string>();
        var page = await NewPageAsync(errors);

        await page.Locator("#t-tree li[role=treeitem]").First.FocusAsync();
        await ExpectActiveLabelAsync(page, "Pai");

        await page.Keyboard.PressAsync("ArrowDown");
        await ExpectActiveLabelAsync(page, "Raiz2");

        await page.Keyboard.PressAsync("ArrowUp");
        await ExpectActiveLabelAsync(page, "Pai");

        // Expande e foca o primeiro filho.
        await page.Keyboard.PressAsync("ArrowRight");
        await ExpectActiveLabelAsync(page, "Filho1");

        await page.Keyboard.PressAsync("ArrowDown");
        await ExpectActiveLabelAsync(page, "Filho2");

        // Em folha, ArrowLeft volta ao pai.
        await page.Keyboard.PressAsync("ArrowLeft");
        await ExpectActiveLabelAsync(page, "Pai");

        // No pai expandido, ArrowLeft recolhe.
        await page.Keyboard.PressAsync("ArrowLeft");
        await page.Keyboard.PressAsync("ArrowDown");
        await ExpectActiveLabelAsync(page, "Raiz2");

        Assert.True(errors.Count == 0, string.Join(" || ", errors));
        await page.CloseAsync();
    }

    [Fact]
    public async Task Tree_Enter_seleciona_o_no()
    {
        var errors = new List<string>();
        var page = await NewPageAsync(errors);

        await page.Locator("#t-tree li[role=treeitem]").First.FocusAsync();
        await ExpectActiveLabelAsync(page, "Pai");

        await page.Keyboard.PressAsync("ArrowDown");
        await ExpectActiveLabelAsync(page, "Raiz2");
        await page.Keyboard.PressAsync("Enter");

        await Expect(page.Locator("#t-tree-log")).ToHaveTextAsync("Raiz2");

        Assert.True(errors.Count == 0, string.Join(" || ", errors));
        await page.CloseAsync();
    }
}
