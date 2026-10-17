using System.Text.RegularExpressions;
using Microsoft.Playwright;
using Xunit;
using static Microsoft.Playwright.Assertions;

namespace OpBlazorUI.Playwright;

/// <summary>
/// Recursos de menu usados nas páginas de documentação (Popup, orientação vertical, templates de item e
/// separador, e painéis controlados). Usa a página <c>/_tests/docs-menus</c>.
/// </summary>
[Collection("showcase")]
public class DocsMenusTests
{
    private readonly ShowcaseFixture _fixture;

    public DocsMenusTests(ShowcaseFixture fixture) => _fixture = fixture;

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
        await page.GotoAsync($"{_fixture.BaseUrl}/_tests/docs-menus", new PageGotoOptions { WaitUntil = WaitUntilState.NetworkIdle });
        await page.Locator("#t-title").WaitForAsync();
        return page;
    }

    [Fact]
    public async Task TieredMenu_popup_abre_e_fecha()
    {
        var errors = new List<string>();
        var page = await NewPageAsync(errors);

        Assert.Equal(0, await page.Locator("#t-popup .p-tieredmenu").CountAsync());

        await page.Locator("#t-popup button").ClickAsync();
        await Expect(page.Locator("#t-popup .p-tieredmenu")).ToBeVisibleAsync();

        await page.Locator("#t-popup button").ClickAsync();
        await Expect(page.Locator("#t-popup .p-tieredmenu")).ToHaveCountAsync(0);

        Assert.True(errors.Count == 0, string.Join(" || ", errors));
        await page.CloseAsync();
    }

    [Fact]
    public async Task MegaMenu_renderiza_na_vertical()
    {
        var errors = new List<string>();
        var page = await NewPageAsync(errors);

        await Expect(page.Locator("#t-vertical .p-megamenu")).ToHaveClassAsync(new Regex("p-megamenu-vertical"));
        await Expect(page.Locator("#t-vertical .p-megamenu-submenu-icon")).ToHaveClassAsync(new Regex("pi-angle-right"));

        Assert.True(errors.Count == 0, string.Join(" || ", errors));
        await page.CloseAsync();
    }

    [Fact]
    public async Task ItemTemplate_substitui_o_conteudo_do_item()
    {
        var errors = new List<string>();
        var page = await NewPageAsync(errors);

        await Expect(page.Locator("#t-item-template .t-custom-item")).ToHaveTextAsync("ItemCustom");

        Assert.True(errors.Count == 0, string.Join(" || ", errors));
        await page.CloseAsync();
    }

    [Fact]
    public async Task SeparatorTemplate_substitui_o_separador()
    {
        var errors = new List<string>();
        var page = await NewPageAsync(errors);

        await Expect(page.Locator("#t-separator .t-sep").First).ToHaveTextAsync("/");

        Assert.True(errors.Count == 0, string.Join(" || ", errors));
        await page.CloseAsync();
    }

    [Fact]
    public async Task PanelMenu_controlado_abre_pelo_modelo()
    {
        var errors = new List<string>();
        var page = await NewPageAsync(errors);

        Assert.Equal(0, await page.Locator("#t-controlled .p-panelmenu-submenu").CountAsync());

        await page.Locator("#t-controlled button").ClickAsync();
        await Expect(page.Locator("#t-controlled .p-panelmenu-submenu")).ToBeVisibleAsync();

        Assert.True(errors.Count == 0, string.Join(" || ", errors));
        await page.CloseAsync();
    }

    [Fact]
    public async Task Recursos_renderizam_no_modo_escuro()
    {
        var errors = new List<string>();
        var page = await NewPageAsync(errors);

        await page.EvaluateAsync("document.documentElement.classList.add('app-dark')");

        await Expect(page.Locator("#t-vertical .p-megamenu-vertical")).ToBeVisibleAsync();
        await Expect(page.Locator("#t-item-template .t-custom-item")).ToBeVisibleAsync();
        await Expect(page.Locator("#t-separator .t-sep").First).ToBeVisibleAsync();

        await page.Locator("#t-popup button").ClickAsync();
        await Expect(page.Locator("#t-popup .p-tieredmenu")).ToBeVisibleAsync();

        await page.Locator("#t-controlled button").ClickAsync();
        await Expect(page.Locator("#t-controlled .p-panelmenu-submenu")).ToBeVisibleAsync();

        Assert.True(errors.Count == 0, string.Join(" || ", errors));
        await page.CloseAsync();
    }
}
