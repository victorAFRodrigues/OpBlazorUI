using Microsoft.Playwright;
using Xunit;
using static Microsoft.Playwright.Assertions;

namespace OpBlazorUI.Playwright;

[Collection("showcase")]
public class DocsLote2Tests
{
    private readonly ShowcaseFixture _fixture;

    public DocsLote2Tests(ShowcaseFixture fixture) => _fixture = fixture;

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
    public async Task Button_secao_directive()
    {
        var (page, errors) = await OpenAsync("/button");
        await AssertSectionsAsync(page, "import", "basic", "directive", "anchor", "accessibility");
        Assert.Empty(errors);
        await page.CloseAsync();
    }

    [Fact]
    public async Task Slider_secao_filter()
    {
        var (page, errors) = await OpenAsync("/slider");
        await AssertSectionsAsync(page, "import", "basic", "filter", "vertical");

        var filter = Section(page, "filter");
        await Expect(filter.Locator(".p-selectbutton")).ToBeVisibleAsync();
        await Expect(filter.Locator(".p-slider")).ToBeVisibleAsync();
        await Expect(filter.Locator("img")).ToBeVisibleAsync();

        Assert.Empty(errors);
        await page.CloseAsync();
    }

    [Fact]
    public async Task AutoComplete_secoes_novas()
    {
        var (page, errors) = await OpenAsync("/autocomplete");
        // No branch atual as seções existentes ainda não têm Id (o PR de auditoria adiciona);
        // aqui validamos as seções novas, que já nascem com Id.
        await AssertSectionsAsync(page, "basicchips", "advancedchips", "floatlabel", "iftalabel", "filled");

        // Chips avançado: o autocomplete com múltiplos + dropdown renderiza os chips.
        var advanced = Section(page, "advancedchips");
        await Expect(advanced.Locator(".p-autocomplete .p-autocomplete-input-multiple")).ToBeVisibleAsync();
        await Expect(advanced.Locator(".p-autocomplete-dropdown")).ToBeVisibleAsync();

        // FloatLabel e IftaLabel embrulham o autocomplete.
        await Expect(Section(page, "floatlabel").Locator(".p-floatlabel .p-autocomplete").First).ToBeVisibleAsync();
        await Expect(Section(page, "iftalabel").Locator(".p-iftalabel .p-autocomplete").First).ToBeVisibleAsync();

        Assert.Empty(errors);
        await page.CloseAsync();
    }
}
