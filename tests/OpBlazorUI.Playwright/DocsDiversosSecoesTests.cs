using Microsoft.Playwright;
using Xunit;
using static Microsoft.Playwright.Assertions;

namespace OpBlazorUI.Playwright;

/// <summary>
/// Seções diversas das páginas de documentação (Ripple, Toolbar, Button, Galleria e Tree):
/// ids, ordem das seções e demos interativos.
/// </summary>
[Collection("showcase")]
public class DocsDiversosSecoesTests
{
    private readonly ShowcaseFixture _fixture;

    public DocsDiversosSecoesTests(ShowcaseFixture fixture) => _fixture = fixture;

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
    public async Task Ripple_secao_default()
    {
        var (page, errors) = await OpenAsync("/ripple");
        await AssertSectionsAsync(page, "import", "default", "custom", "accessibility");
        Assert.Empty(errors);
        await page.CloseAsync();
    }

    [Fact]
    public async Task Toolbar_secao_custom_com_avatar()
    {
        var (page, errors) = await OpenAsync("/toolbar");
        await AssertSectionsAsync(page, "import", "basic", "custom", "content", "accessibility");
        await Expect(Section(page, "custom").Locator(".p-toolbar .p-avatar").First).ToBeVisibleAsync();
        Assert.Empty(errors);
        await page.CloseAsync();
    }

    [Fact]
    public async Task Button_secao_directive()
    {
        var (page, errors) = await OpenAsync("/button");
        await AssertSectionsAsync(page, "import", "basic", "directive", "anchor", "accessibility");
        Assert.Empty(errors);
        await page.CloseAsync();
    }

    [Fact]
    public async Task Galleria_secoes_novas()
    {
        var (page, errors) = await OpenAsync("/galleria");
        await AssertSectionsAsync(page,
            "import", "basic", "controlled",
            "indicator", "clickevent", "hoverevent", "position", "template",
            "thumbnail", "responsive",
            "fullscreen", "withthumbnails", "withtouthumbnails", "customcontent",
            "navigator", "itemwiththumbnails", "itemwithtouthumbnails", "hover", "withindicators",
            "autoplay", "caption", "advanced", "accessibility");

        // Indicadores: a lista de indicadores é renderizada.
        await Expect(Section(page, "clickevent").Locator(".p-galleria-indicator").First).ToBeVisibleAsync();

        // Navegadores: botões de anterior/próximo sobre o item.
        await Expect(Section(page, "itemwiththumbnails").Locator(".p-galleria-nav-button").First).ToBeVisibleAsync();

        Assert.Empty(errors);
        await page.CloseAsync();
    }

    [Fact]
    public async Task Tree_secoes_nao_suportadas()
    {
        var (page, errors) = await OpenAsync("/tree");
        await AssertSectionsAsync(page,
            "import", "basic", "controlled", "single", "multiple", "checkbox", "event", "lazy", "template", "filter",
            "preload", "lazyvirtualscroll", "dragdrop-single", "dragdrop-multiple", "contextmenu", "accessibility");
        Assert.Empty(errors);
        await page.CloseAsync();
    }
}
