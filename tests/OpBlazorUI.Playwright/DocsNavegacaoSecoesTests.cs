using Microsoft.Playwright;
using Xunit;
using static Microsoft.Playwright.Assertions;

namespace OpBlazorUI.Playwright;

/// <summary>
/// Seções das páginas de documentação de navegação (Accordion, Tabs, Steps e Breadcrumb):
/// ids, ordem das seções e demos interativos.
/// </summary>
[Collection("showcase")]
public class DocsNavegacaoSecoesTests
{
    private readonly ShowcaseFixture _fixture;

    public DocsNavegacaoSecoesTests(ShowcaseFixture fixture) => _fixture = fixture;

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
    public async Task Accordion_secoes_novas()
    {
        var (page, errors) = await OpenAsync("/accordion");
        await AssertSectionsAsync(page, "import", "basic", "dynamic", "multiple", "disabled", "controlled", "template", "accessibility");

        var controlled = Section(page, "controlled").Locator(".p-accordion").First;
        await Expect(controlled.Locator(".p-accordionpanel").Nth(0)).ToHaveClassAsync(new System.Text.RegularExpressions.Regex("p-accordionpanel-active"));
        await Section(page, "controlled").Locator("button:has-text('2')").First.ClickAsync();
        await Expect(controlled.Locator(".p-accordionpanel").Nth(1)).ToHaveClassAsync(new System.Text.RegularExpressions.Regex("p-accordionpanel-active"));

        // O painel desabilitado não abre: o cabeçalho fica indisponível para interação.
        var disabled = Section(page, "disabled").Locator(".p-accordion").First;
        await Expect(disabled.Locator(".p-accordionheader").Nth(3)).ToHaveAttributeAsync("aria-disabled", "true");
        await Expect(disabled.Locator(".p-accordionpanel").Nth(3)).Not.ToHaveClassAsync(new System.Text.RegularExpressions.Regex("p-accordionpanel-active"));

        Assert.Empty(errors);
        await page.CloseAsync();
    }

    [Fact]
    public async Task Breadcrumb_secoes_novas()
    {
        var (page, errors) = await OpenAsync("/breadcrumb");
        await AssertSectionsAsync(page, "import", "basic", "home", "icons", "template", "router", "accessibility");
        Assert.Empty(errors);
        await page.CloseAsync();
    }

    [Fact]
    public async Task Tabs_secoes_novas()
    {
        var (page, errors) = await OpenAsync("/tabs");
        await AssertSectionsAsync(page, "import", "basic", "dynamic", "controlled", "lazy", "disabled", "template", "tabmenu", "accessibility");

        // Dinâmico: a lista de abas renderiza normalmente.
        await Expect(Section(page, "dynamic").Locator(".p-tablist").First).ToBeVisibleAsync();

        // TabMenu: componente dedicado de navegação por abas.
        await Expect(Section(page, "tabmenu").Locator(".p-tabmenu").First).ToBeVisibleAsync();

        Assert.Empty(errors);
        await page.CloseAsync();
    }

    [Fact]
    public async Task Steps_secoes_novas()
    {
        var (page, errors) = await OpenAsync("/steps");
        await AssertSectionsAsync(page, "import", "basic", "controlled", "interactive", "routing", "accessibility");
        await Expect(Section(page, "interactive").Locator(".p-steps").First).ToBeVisibleAsync();
        Assert.Empty(errors);
        await page.CloseAsync();
    }
}
