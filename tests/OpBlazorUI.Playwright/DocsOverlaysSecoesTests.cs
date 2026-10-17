using Microsoft.Playwright;
using Xunit;
using static Microsoft.Playwright.Assertions;

namespace OpBlazorUI.Playwright;

/// <summary>
/// Seções das páginas de documentação de overlays (DynamicDialog e ConfirmPopup):
/// ids, ordem das seções e demos interativos.
/// </summary>
[Collection("showcase")]
public class DocsOverlaysSecoesTests
{
    private readonly ShowcaseFixture _fixture;

    public DocsOverlaysSecoesTests(ShowcaseFixture fixture) => _fixture = fixture;

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
    public async Task DynamicDialog_secoes_novas()
    {
        var (page, errors) = await OpenAsync("/dynamicdialog");
        await AssertSectionsAsync(page, "import", "usage", "open", "customization", "passingdata", "close", "example");

        // Exemplo: abrir o diálogo de seleção de produto.
        await Section(page, "example").Locator("button").First.ClickAsync();
        await Expect(page.Locator(".p-dialog").First).ToBeVisibleAsync();

        Assert.Empty(errors);
        await page.CloseAsync();
    }

    [Fact]
    public async Task ConfirmPopup_secoes_novas()
    {
        var (page, errors) = await OpenAsync("/confirmpopup");
        await AssertSectionsAsync(page, "import", "basic", "custom", "template", "headless", "events", "accessibility");

        // Template: abre o popup ancorado ao botão.
        await Section(page, "template").Locator("button").First.ClickAsync();
        await Expect(page.Locator(".p-confirmpopup").First).ToBeVisibleAsync();

        Assert.Empty(errors);
        await page.CloseAsync();
    }
}
