using Microsoft.Playwright;
using Xunit;
using static Microsoft.Playwright.Assertions;

namespace OpBlazorUI.Playwright;

/// <summary>
/// Seções das páginas de documentação dos menus (ContextMenu, PanelMenu, MegaMenu, TieredMenu
/// e Menubar): ids, ordem das seções e demos interativos.
/// </summary>
[Collection("showcase")]
public class DocsMenusSecoesTests
{
    private readonly ShowcaseFixture _fixture;

    public DocsMenusSecoesTests(ShowcaseFixture fixture) => _fixture = fixture;

    private async Task<(IPage page, List<string> errors)> OpenAsync(string route)
    {
        var errors = new List<string>();
        var page = await _fixture.Browser.NewPageAsync();
        page.SetDefaultTimeout(60000);
        page.Console += (_, msg) =>
        {
            if (msg.Type == "error")
            {
                errors.Add(msg.Text);
            }
        };
        page.PageError += (_, err) => errors.Add(err);

        await page.GotoAsync($"{_fixture.BaseUrl}{route}",
            new PageGotoOptions { WaitUntil = WaitUntilState.NetworkIdle });
        return (page, errors);
    }

    private static ILocator Section(IPage page, string id) =>
        page.Locator($"section.doc-section:has(h2#{id})");

    private static async Task AssertSectionsAsync(IPage page, params string[] ids)
    {
        foreach (var id in ids)
        {
            await Expect(Section(page, id)).ToBeVisibleAsync();
        }
    }

    [Fact]
    public async Task ContextMenu_secoes_novas()
    {
        var (page, errors) = await OpenAsync("/contextmenu");
        await AssertSectionsAsync(page, "import", "basic", "document", "template", "command", "router", "table", "accessibility");
        Assert.Empty(errors);
        await page.CloseAsync();
    }

    [Fact]
    public async Task PanelMenu_secoes_novas()
    {
        var (page, errors) = await OpenAsync("/panelmenu");
        await AssertSectionsAsync(page, "import", "basic", "multiple", "controlled", "template", "command", "router", "accessibility");

        // Controlado: o botão expande todos os painéis.
        var controlled = Section(page, "controlled").Locator(".p-panelmenu").First;
        await Section(page, "controlled").Locator("button").First.ClickAsync();
        await Expect(controlled.Locator(".p-panelmenu-header-active").First).ToBeVisibleAsync();

        Assert.Empty(errors);
        await page.CloseAsync();
    }

    [Fact]
    public async Task MegaMenu_secoes_novas()
    {
        var (page, errors) = await OpenAsync("/megamenu");
        await AssertSectionsAsync(page, "import", "basic", "vertical", "template", "command", "router", "accessibility");
        await Expect(page.Locator(".p-megamenu-vertical").First).ToBeVisibleAsync();
        Assert.Empty(errors);
        await page.CloseAsync();
    }

    [Fact]
    public async Task TieredMenu_secoes_novas()
    {
        var (page, errors) = await OpenAsync("/tieredmenu");
        await AssertSectionsAsync(page, "import", "basic", "popup", "template", "command", "router", "accessibility");

        // Popup: o botão "Alternar" abre o overlay do tieredmenu.
        await Section(page, "popup").Locator("button").First.ClickAsync();
        await Expect(page.Locator(".p-tieredmenu-overlay").First).ToBeVisibleAsync();

        Assert.Empty(errors);
        await page.CloseAsync();
    }

    [Fact]
    public async Task Menubar_secoes_novas()
    {
        var (page, errors) = await OpenAsync("/menubar");
        await AssertSectionsAsync(page, "import", "basic", "icons", "template", "command", "router", "accessibility");
        Assert.Empty(errors);
        await page.CloseAsync();
    }
}
