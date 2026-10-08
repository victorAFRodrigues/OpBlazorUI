using Microsoft.Playwright;
using Xunit;
using static Microsoft.Playwright.Assertions;

namespace OpBlazorUI.Playwright;

[Collection("showcase")]
public class P0FormTests
{
    private readonly ShowcaseFixture _fixture;

    public P0FormTests(ShowcaseFixture fixture) => _fixture = fixture;

    private async Task<IPage> NewPageAsync(List<string> consoleErrors, string route)
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
        await page.GotoAsync($"{_fixture.BaseUrl}{route}", new PageGotoOptions { WaitUntil = WaitUntilState.NetworkIdle });
        return page;
    }

    [Fact]
    public async Task Textarea_renderiza_e_aceita_digitacao()
    {
        var errors = new List<string>();
        var page = await NewPageAsync(errors, "/textarea");

        var textarea = page.Locator("textarea.p-textarea").First;
        await textarea.WaitForAsync();
        await textarea.FillAsync("Olá Textarea");

        Assert.Equal("Olá Textarea", await textarea.InputValueAsync());
        Assert.Empty(errors);
        await page.CloseAsync();
    }

    [Fact]
    public async Task Fieldset_alternavel_recolhe_o_conteudo()
    {
        var errors = new List<string>();
        var page = await NewPageAsync(errors, "/fieldset");

        var button = page.Locator("button.p-fieldset-toggle-button").First;
        await button.WaitForAsync();
        await Expect(button).ToHaveAttributeAsync("aria-expanded", "true");

        await button.ClickAsync();
        await Expect(button).ToHaveAttributeAsync("aria-expanded", "false");

        Assert.Empty(errors);
        await page.CloseAsync();
    }

    [Fact]
    public async Task InputChips_adiciona_chip_com_enter()
    {
        var errors = new List<string>();
        var page = await NewPageAsync(errors, "/inputchips");

        var input = page.Locator(".p-inputchips-input-item input").First;
        await input.WaitForAsync();
        await input.ClickAsync();
        await input.PressSequentiallyAsync("angular");
        await input.PressAsync("Enter");

        await Expect(page.Locator(".p-inputchips-chip").First).ToBeVisibleAsync();

        Assert.Empty(errors);
        await page.CloseAsync();
    }

    [Fact]
    public async Task Slider_responde_as_setas_do_teclado()
    {
        var errors = new List<string>();
        var page = await NewPageAsync(errors, "/slider");

        var handle = page.Locator(".p-slider .p-slider-handle").First;
        await handle.WaitForAsync();
        var before = int.Parse(await handle.GetAttributeAsync("aria-valuenow") ?? "0");

        await handle.FocusAsync();
        await page.Keyboard.PressAsync("ArrowRight");

        var after = int.Parse(await handle.GetAttributeAsync("aria-valuenow") ?? "0");
        Assert.NotEqual(before, after);

        Assert.Empty(errors);
        await page.CloseAsync();
    }

    [Fact]
    public async Task ColorPicker_inline_exibe_o_painel()
    {
        var errors = new List<string>();
        var page = await NewPageAsync(errors, "/colorpicker");

        var panel = page.Locator(".p-colorpicker-panel-inline").First;
        await panel.WaitForAsync();
        await Expect(panel).ToBeVisibleAsync();

        Assert.Empty(errors);
        await page.CloseAsync();
    }

    [Fact]
    public async Task FileUpload_exibe_o_botao_escolher()
    {
        var errors = new List<string>();
        var page = await NewPageAsync(errors, "/fileupload");

        var choose = page.Locator(".p-fileupload-advanced .op-fileupload-choose").First;
        await choose.WaitForAsync();
        await Expect(choose).ToBeVisibleAsync();

        Assert.Empty(errors);
        await page.CloseAsync();
    }
}
