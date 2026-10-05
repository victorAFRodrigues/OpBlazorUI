using Microsoft.Playwright;
using Xunit;
using static Microsoft.Playwright.Assertions;

namespace OpBlazorUI.Playwright;

[Collection("showcase")]
public class FocusTests
{
    private readonly ShowcaseFixture _fixture;

    public FocusTests(ShowcaseFixture fixture) => _fixture = fixture;

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
        return page;
    }

    private static Task<bool> FocusIsInsideAsync(IPage page, string selector) =>
        page.EvaluateAsync<bool>(
            "sel => { const root = document.querySelector(sel); return !!root && root.contains(document.activeElement); }",
            selector);

    [Fact]
    public async Task ConfirmDialog_prende_foco_foca_accept_e_restaura_o_gatilho()
    {
        var errors = new List<string>();
        var page = await NewPageAsync(errors);

        await page.GotoAsync($"{_fixture.BaseUrl}/confirmdialog",
            new PageGotoOptions { WaitUntil = WaitUntilState.NetworkIdle });

        var trigger = page.Locator("button:has-text('Salvar')").First;
        await trigger.WaitForAsync();
        await trigger.ClickAsync();

        var dialog = page.Locator(".p-confirmdialog").First;
        await dialog.WaitForAsync();

        // DefaultFocus="accept": o botão de aceitar recebe o foco inicial.
        await Expect(page.Locator("[data-pc-focus='accept']").First).ToBeFocusedAsync();

        // Tab permanece preso dentro do diálogo (e sai do accept).
        await page.Keyboard.PressAsync("Tab");
        await Expect(page.Locator("[data-pc-focus='accept']").First).Not.ToBeFocusedAsync();
        Assert.True(await FocusIsInsideAsync(page, ".p-confirmdialog"), "Tab deve manter o foco dentro do diálogo.");

        // Shift+Tab volta ao início e continua preso.
        await page.Keyboard.PressAsync("Shift+Tab");
        Assert.True(await FocusIsInsideAsync(page, ".p-confirmdialog"), "Shift+Tab deve manter o foco dentro do diálogo.");

        // Escape fecha e devolve o foco ao gatilho.
        await page.Keyboard.PressAsync("Escape");
        await Expect(dialog).ToBeHiddenAsync();
        await Expect(trigger).ToBeFocusedAsync();

        Assert.Empty(errors);
        await page.CloseAsync();
    }

    [Fact]
    public async Task Dialog_modal_prende_o_foco_e_restaura_o_gatilho()
    {
        var errors = new List<string>();
        var page = await NewPageAsync(errors);

        await page.GotoAsync($"{_fixture.BaseUrl}/dialog",
            new PageGotoOptions { WaitUntil = WaitUntilState.NetworkIdle });

        var trigger = page.Locator("button:has-text('Mostrar')").First;
        await trigger.WaitForAsync();
        await trigger.ClickAsync();

        var dialog = page.Locator(".p-dialog").First;
        await dialog.WaitForAsync();

        // FocusOnShow: o foco vai para a raiz do diálogo (tabindex=-1).
        Assert.True(await FocusIsInsideAsync(page, ".p-dialog"), "O foco deve iniciar dentro do diálogo.");

        // Tab continua preso dentro do diálogo.
        await page.Keyboard.PressAsync("Tab");
        Assert.True(await FocusIsInsideAsync(page, ".p-dialog"), "Tab deve manter o foco dentro do diálogo.");

        // Escape fecha e devolve o foco ao gatilho.
        await page.Keyboard.PressAsync("Escape");
        await Expect(dialog).ToBeHiddenAsync();
        await Expect(trigger).ToBeFocusedAsync();

        Assert.Empty(errors);
        await page.CloseAsync();
    }
}
