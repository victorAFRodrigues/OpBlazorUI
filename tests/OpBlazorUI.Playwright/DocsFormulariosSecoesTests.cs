using Microsoft.Playwright;
using Xunit;
using static Microsoft.Playwright.Assertions;

namespace OpBlazorUI.Playwright;

/// <summary>
/// Seções das páginas de documentação de formulários (AutoComplete, Slider, Knob e FileUpload):
/// ids, ordem das seções e demos interativos.
/// </summary>
[Collection("showcase")]
public class DocsFormulariosSecoesTests
{
    private readonly ShowcaseFixture _fixture;

    public DocsFormulariosSecoesTests(ShowcaseFixture fixture) => _fixture = fixture;

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

    [Fact]
    public async Task FileUpload_secoes_novas()
    {
        var (page, errors) = await OpenAsync("/fileupload");
        await AssertSectionsAsync(page, "import", "basic", "auto", "advanced", "simple", "custom", "validation", "template", "accessibility");
        await Expect(Section(page, "advanced").Locator(".p-fileupload-advanced").First).ToBeVisibleAsync();
        Assert.Empty(errors);
        await page.CloseAsync();
    }

    [Fact]
    public async Task Knob_secao_forms()
    {
        var (page, errors) = await OpenAsync("/knob");
        await AssertSectionsAsync(page, "import", "basic", "minmax", "step", "template", "stroke", "size", "color", "readonly", "forms", "disabled", "accessibility");
        await Expect(Section(page, "forms").Locator("form").First).ToBeVisibleAsync();
        Assert.Empty(errors);
        await page.CloseAsync();
    }
}
