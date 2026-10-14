using Microsoft.Playwright;
using Xunit;
using static Microsoft.Playwright.Assertions;

namespace OpBlazorUI.Playwright;

/// <summary>
/// Bloco 4.3: DynamicDialog (OpDialogService + OpDynamicDialogHost). Usa a página
/// <c>/_tests/bloco4-3</c> do Showcase.
/// </summary>
[Collection("showcase")]
public class Bloco4_3Tests
{
    private readonly ShowcaseFixture _fixture;

    public Bloco4_3Tests(ShowcaseFixture fixture) => _fixture = fixture;

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
        await page.GotoAsync($"{_fixture.BaseUrl}/_tests/bloco4-3", new PageGotoOptions { WaitUntil = WaitUntilState.NetworkIdle });
        await page.Locator("#t-title").WaitForAsync();
        return page;
    }

    [Fact]
    public async Task Abre_dialog_dinamico_com_header_e_inputs()
    {
        var errors = new List<string>();
        var page = await NewPageAsync(errors);

        await page.Locator("#t-open-dyn").ClickAsync();

        await Expect(page.Locator(".p-dialog-title")).ToHaveTextAsync("Dinâmico");
        await Expect(page.Locator("#t-dyn-content")).ToHaveTextAsync("Olá dinâmico");

        Assert.True(errors.Count == 0, string.Join(" || ", errors));
        await page.CloseAsync();
    }

    [Fact]
    public async Task Close_retorna_resultado_ao_chamador()
    {
        var errors = new List<string>();
        var page = await NewPageAsync(errors);

        await page.Locator("#t-open-dyn").ClickAsync();
        await Expect(page.Locator(".p-dialog")).ToBeVisibleAsync();

        await page.Locator("#t-dyn-ok").ClickAsync();
        await Expect(page.Locator(".p-dialog")).ToHaveCountAsync(0);
        await Expect(page.Locator("#t-dyn-log")).ToHaveTextAsync("ok");

        Assert.True(errors.Count == 0, string.Join(" || ", errors));
        await page.CloseAsync();
    }

    [Fact]
    public async Task Fechar_pelo_X_retorna_null()
    {
        var errors = new List<string>();
        var page = await NewPageAsync(errors);

        await page.Locator("#t-open-dyn").ClickAsync();
        await Expect(page.Locator(".p-dialog")).ToBeVisibleAsync();

        await page.Locator(".p-dialog-close-button").ClickAsync();
        await Expect(page.Locator(".p-dialog")).ToHaveCountAsync(0);
        await Expect(page.Locator("#t-dyn-log")).ToHaveTextAsync("null");

        Assert.True(errors.Count == 0, string.Join(" || ", errors));
        await page.CloseAsync();
    }
}
