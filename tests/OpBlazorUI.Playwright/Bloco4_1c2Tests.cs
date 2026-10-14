using Microsoft.Playwright;
using Xunit;
using static Microsoft.Playwright.Assertions;

namespace OpBlazorUI.Playwright;

/// <summary>
/// Bloco 4.1c2 (teclado de TreeSelect e TreeTable). Usa a página <c>/_tests/bloco4-1c2</c>.
/// </summary>
[Collection("showcase")]
public class Bloco4_1c2Tests
{
    private readonly ShowcaseFixture _fixture;

    public Bloco4_1c2Tests(ShowcaseFixture fixture) => _fixture = fixture;

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
        await page.GotoAsync($"{_fixture.BaseUrl}/_tests/bloco4-1c2", new PageGotoOptions { WaitUntil = WaitUntilState.NetworkIdle });
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
    public async Task TreeSelect_navega_e_seleciona_por_teclado()
    {
        var errors = new List<string>();
        var page = await NewPageAsync(errors);

        await page.Locator("#t-ts .p-treeselect-dropdown").ClickAsync();
        await page.Locator("#t-ts li[role=treeitem]").First.WaitForAsync();

        await page.Locator("#t-ts li[role=treeitem]").First.FocusAsync();
        await ExpectActiveLabelAsync(page, "Pai");

        // Abre (expande) e foca o primeiro filho.
        await page.Keyboard.PressAsync("ArrowRight");
        await ExpectActiveLabelAsync(page, "Filho1");

        await page.Keyboard.PressAsync("ArrowDown");
        await ExpectActiveLabelAsync(page, "Filho2");

        await page.Keyboard.PressAsync("Enter");
        await Expect(page.Locator("#t-ts-value")).ToHaveTextAsync("f2");

        Assert.True(errors.Count == 0, string.Join(" || ", errors));
        await page.CloseAsync();
    }

    [Fact]
    public async Task TreeTable_navega_expande_e_seleciona_por_teclado()
    {
        var errors = new List<string>();
        var page = await NewPageAsync(errors);

        await page.Locator("#t-tt tr.p-treetable-row").First.FocusAsync();
        var first = page.Locator("#t-tt tr.p-treetable-row").First;
        await Expect(first).ToContainTextAsync("Pai");

        // Expande e foca o primeiro filho.
        await page.Keyboard.PressAsync("ArrowRight");
        var child = page.Locator("#t-tt tr.p-treetable-row").Nth(1);
        await Expect(child).ToContainTextAsync("Filho1");

        await page.Keyboard.PressAsync("ArrowDown");
        await page.Keyboard.PressAsync("Enter");
        await Expect(page.Locator("#t-tt-log")).ToHaveTextAsync("Filho2");

        Assert.True(errors.Count == 0, string.Join(" || ", errors));
        await page.CloseAsync();
    }
}
