using Microsoft.Playwright;
using Xunit;
using static Microsoft.Playwright.Assertions;

namespace OpBlazorUI.Playwright;

[Collection("showcase")]
public class P0NavTests
{
    private readonly ShowcaseFixture _fixture;

    public P0NavTests(ShowcaseFixture fixture) => _fixture = fixture;

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
    public async Task Tabs_troca_de_aba()
    {
        var errors = new List<string>();
        var page = await NewPageAsync(errors, "/tabs");

        var tabs = page.Locator(".p-tabs").First.Locator(".p-tab");
        await tabs.First.WaitForAsync();
        await tabs.Nth(1).ClickAsync();

        await Expect(page.Locator(".p-tabs").First.Locator(".p-tabpanel")).ToContainTextAsync("Conteúdo da segunda aba");

        Assert.Empty(errors);
        await page.CloseAsync();
    }

    [Fact]
    public async Task Accordion_expande_painel()
    {
        var errors = new List<string>();
        var page = await NewPageAsync(errors, "/accordion");

        var header = page.Locator(".p-accordionheader").Nth(1);
        await header.WaitForAsync();
        await header.ClickAsync();

        await Expect(header).ToHaveAttributeAsync("aria-expanded", "true");

        Assert.Empty(errors);
        await page.CloseAsync();
    }

    [Fact]
    public async Task Steps_seleciona_etapa()
    {
        var errors = new List<string>();
        var page = await NewPageAsync(errors, "/steps");

        var step = page.Locator(".p-steps-item-link").Nth(2);
        await step.WaitForAsync();
        await step.ClickAsync();

        await Expect(page.Locator(".p-steps-item").Nth(2)).ToHaveClassAsync(new System.Text.RegularExpressions.Regex("p-steps-item-active"));

        Assert.Empty(errors);
        await page.CloseAsync();
    }

    [Fact]
    public async Task Menubar_abre_submenu()
    {
        var errors = new List<string>();
        var page = await NewPageAsync(errors, "/menubar");

        var link = page.Locator(".p-menubar-root-list > .p-menubar-item > .p-menubar-item-content > .p-menubar-item-link").First;
        await link.WaitForAsync();
        await link.ClickAsync();

        await Expect(page.Locator(".p-menubar-submenu").First).ToBeVisibleAsync();

        Assert.Empty(errors);
        await page.CloseAsync();
    }

    [Fact]
    public async Task ContextMenu_exibe_ao_clicar()
    {
        var errors = new List<string>();
        var page = await NewPageAsync(errors, "/contextmenu");

        var trigger = page.Locator("button:has-text('Mostrar menu')").First;
        await trigger.WaitForAsync();
        await trigger.ClickAsync();

        await Expect(page.Locator(".p-contextmenu").First).ToBeVisibleAsync();

        Assert.Empty(errors);
        await page.CloseAsync();
    }

    [Fact]
    public async Task Breadcrumb_renderiza_os_itens()
    {
        var errors = new List<string>();
        var page = await NewPageAsync(errors, "/breadcrumb");

        await Expect(page.Locator(".p-breadcrumb-item").First).ToBeVisibleAsync();
        Assert.True(await page.Locator(".p-breadcrumb-item").CountAsync() >= 3);

        Assert.Empty(errors);
        await page.CloseAsync();
    }

    [Fact]
    public async Task Toolbar_renderiza_start_e_end()
    {
        var errors = new List<string>();
        var page = await NewPageAsync(errors, "/toolbar");

        await Expect(page.Locator(".p-toolbar-start").First).ToBeVisibleAsync();
        await Expect(page.Locator(".p-toolbar-end").First).ToBeVisibleAsync();

        Assert.Empty(errors);
        await page.CloseAsync();
    }

    [Fact]
    public async Task PanelMenu_expande_um_painel()
    {
        var errors = new List<string>();
        var page = await NewPageAsync(errors, "/panelmenu");

        var headerLink = page.Locator(".p-panelmenu-header-link").First;
        await headerLink.WaitForAsync();
        await headerLink.ClickAsync();

        await Expect(page.Locator(".p-panelmenu-submenu").First).ToBeVisibleAsync();

        Assert.Empty(errors);
        await page.CloseAsync();
    }
}
