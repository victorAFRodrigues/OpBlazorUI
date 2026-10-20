using Microsoft.Playwright;
using Xunit;
using static Microsoft.Playwright.Assertions;

namespace OpBlazorUI.Playwright;

/// <summary>
/// Configuration: documenta o padrão claro, os três valores de OpDarkMode e a precedência da
/// persistência sem depender do modo do sistema operacional.
/// </summary>
[Collection("showcase")]
public class ConfigurationTests
{
    private readonly ShowcaseFixture _fixture;

    public ConfigurationTests(ShowcaseFixture fixture) => _fixture = fixture;

    [Fact]
    public async Task Configuration_documenta_modo_claro_e_persistencia()
    {
        var errors = new List<string>();
        var page = await _fixture.Browser.NewPageAsync();
        page.SetDefaultTimeout(60000);
        page.Console += (_, msg) =>
        {
            if (msg.Type == "error") errors.Add(msg.Text);
        };
        page.PageError += (_, err) => errors.Add(err);

        await page.GotoAsync($"{_fixture.BaseUrl}/configuration",
            new PageGotoOptions { WaitUntil = WaitUntilState.NetworkIdle });

        await Expect(page.Locator("h1")).ToContainTextAsync("Configuration");
        foreach (var heading in new[] { "Tema padrão e modo escuro", "Salvando as escolhas" })
        {
            await Expect(page.Locator("h2", new() { HasText = heading }).First).ToBeVisibleAsync();
        }

        await Expect(page.GetByText("OpDarkMode.Light").First).ToBeVisibleAsync();
        await Expect(page.GetByText("data-dark=\"light\"").First).ToBeVisibleAsync();
        await Expect(page.GetByText("data-persist=\"false\"").First).ToBeVisibleAsync();
        await Expect(page.GetByText("data-surface=\"slate\"").First).ToBeVisibleAsync();

        Assert.Empty(errors);
        await page.CloseAsync();
    }
}
