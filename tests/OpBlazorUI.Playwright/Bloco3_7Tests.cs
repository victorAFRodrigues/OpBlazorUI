using Microsoft.Playwright;
using Xunit;
using static Microsoft.Playwright.Assertions;

namespace OpBlazorUI.Playwright;

/// <summary>
/// Bloco 3.7 (menus). Usa a página <c>/_tests/bloco3-7</c>.
/// </summary>
[Collection("showcase")]
public class Bloco3_7Tests
{
    private readonly ShowcaseFixture _fixture;

    public Bloco3_7Tests(ShowcaseFixture fixture) => _fixture = fixture;

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
        await page.GotoAsync($"{_fixture.BaseUrl}/_tests/bloco3-7", new PageGotoOptions { WaitUntil = WaitUntilState.NetworkIdle });
        await page.Locator("#t-title").WaitForAsync();
        return page;
    }

    private static ILocator Item(IPage page, string scope, string text) =>
        page.Locator($"{scope} .p-menu-item, {scope} .p-tieredmenu-item, {scope} .p-panelmenu-item")
            .Filter(new LocatorFilterOptions { HasText = text });

    [Fact]
    public async Task Menu_item_disabled_nao_navega_e_Target_e_respeitado()
    {
        var errors = new List<string>();
        var page = await NewPageAsync(errors);

        var off = page.Locator("#t-menu .p-menu-item").Filter(new LocatorFilterOptions { HasText = "Off" }).Locator("a.p-menu-item-link");
        Assert.Null(await off.GetAttributeAsync("href"));

        var link = page.Locator("#t-menu .p-menu-item").Filter(new LocatorFilterOptions { HasText = "Link" }).Locator("a.p-menu-item-link");
        Assert.Equal("_blank", await link.GetAttributeAsync("target"));

        Assert.True(errors.Count == 0, string.Join(" || ", errors));
        await page.CloseAsync();
    }

    [Fact]
    public async Task Menu_executa_Command()
    {
        var errors = new List<string>();
        var page = await NewPageAsync(errors);

        await page.Locator("#t-menu .p-menu-item").Filter(new LocatorFilterOptions { HasText = "Exec" }).ClickAsync();
        await Expect(page.Locator("#t-menu-cmd")).ToHaveTextAsync("1");

        Assert.True(errors.Count == 0, string.Join(" || ", errors));
        await page.CloseAsync();
    }

    [Fact]
    public async Task TieredMenu_fecha_irmao_e_folha_fecha_tudo()
    {
        var errors = new List<string>();
        var page = await NewPageAsync(errors);

        var submenus = page.Locator("#t-tiered .p-tieredmenu-submenu");
        await Expect(submenus).ToHaveCountAsync(0);

        await page.Locator("#t-tiered .p-tieredmenu-item-link").Filter(new LocatorFilterOptions { HasText = "A" }).ClickAsync();
        await Expect(submenus).ToHaveCountAsync(1);
        await Expect(submenus.First).ToContainTextAsync("A1");

        // Abrir o irmão fecha o submenu anterior.
        await page.Locator("#t-tiered .p-tieredmenu-item-link").Filter(new LocatorFilterOptions { HasText = "B" }).ClickAsync();
        await Expect(submenus).ToHaveCountAsync(1);
        await Expect(submenus.First).ToContainTextAsync("B1");
        await Expect(page.Locator("#t-tiered")).Not.ToContainTextAsync("A1");

        // Clicar numa folha fecha os submenus.
        await page.Locator("#t-tiered .p-tieredmenu-item-link").Filter(new LocatorFilterOptions { HasText = "B1" }).ClickAsync();
        await Expect(submenus).ToHaveCountAsync(0);

        Assert.True(errors.Count == 0, string.Join(" || ", errors));
        await page.CloseAsync();
    }

    [Fact]
    public async Task PanelMenu_com_tres_niveis()
    {
        var errors = new List<string>();
        var page = await NewPageAsync(errors);

        await page.Locator("#t-panelmenu .p-panelmenu-header-link").Filter(new LocatorFilterOptions { HasText = "P1" }).ClickAsync();
        await Expect(page.Locator("#t-panelmenu")).ToContainTextAsync("P2");

        await page.Locator("#t-panelmenu .p-panelmenu-header-link").Filter(new LocatorFilterOptions { HasText = "P2" }).ClickAsync();
        await Expect(page.Locator("#t-panelmenu")).ToContainTextAsync("Leaf3");

        Assert.True(errors.Count == 0, string.Join(" || ", errors));
        await page.CloseAsync();
    }

    [Fact]
    public async Task ContextMenu_abre_nas_coordenadas_do_mouse()
    {
        var errors = new List<string>();
        var page = await NewPageAsync(errors);

        var box = await page.Locator("#t-context-target").BoundingBoxAsync();
        Assert.NotNull(box);

        await page.Mouse.ClickAsync(box!.X + 10, box.Y + 10, new MouseClickOptions { Button = MouseButton.Right });
        var menu = page.Locator("#t-context .p-contextmenu");
        await Expect(menu).ToBeVisibleAsync();
        var firstLeft = await menu.EvaluateAsync<string>("el => el.style.left");

        await page.Mouse.ClickAsync(box.X + 180, box.Y + 100, new MouseClickOptions { Button = MouseButton.Right });
        await Expect(menu).ToBeVisibleAsync();
        var secondLeft = await menu.EvaluateAsync<string>("el => el.style.left");

        Assert.NotEqual(firstLeft, secondLeft);

        Assert.True(errors.Count == 0, string.Join(" || ", errors));
        await page.CloseAsync();
    }

    [Fact]
    public async Task Submenu_inverte_perto_da_borda()
    {
        var errors = new List<string>();
        var page = await NewPageAsync(errors);
        await page.SetViewportSizeAsync(500, 700);

        await page.Locator("#t-flip .p-tieredmenu-item-link").Filter(new LocatorFilterOptions { HasText = "Root" }).ClickAsync();
        await page.Locator("#t-flip .p-tieredmenu-item-link").Filter(new LocatorFilterOptions { HasText = "Nivel2" }).ClickAsync();

        // Perto da borda direita o submenu abre para a esquerda.
        await Expect(page.Locator("#t-flip [data-op-submenu='1']")).ToHaveAttributeAsync("data-op-submenu-placement", "left");
        // E o nível seguinte continua abrindo ao lado (não abaixo).
        await Expect(page.Locator("#t-flip [data-op-submenu='2']"))
            .ToHaveAttributeAsync("data-op-submenu-placement", new System.Text.RegularExpressions.Regex("^(left|right)$"));

        Assert.True(errors.Count == 0, string.Join(" || ", errors));
        await page.CloseAsync();
    }
}
