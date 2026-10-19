using Microsoft.Playwright;
using Xunit;
using static Microsoft.Playwright.Assertions;

namespace OpBlazorUI.Playwright;

[Collection("showcase")]
public class LlmsTxtPageTests
{
    private readonly ShowcaseFixture _fixture;

    public LlmsTxtPageTests(ShowcaseFixture fixture) => _fixture = fixture;

    [Fact]
    public async Task Pagina_llms_txt_renderiza_sem_erros()
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

        await page.GotoAsync($"{_fixture.BaseUrl}/llms-txt", new PageGotoOptions { WaitUntil = WaitUntilState.NetworkIdle });
        await Expect(page.Locator("h1")).ToContainTextAsync("LLMs.txt");
        await Expect(page.Locator("h2", new() { HasText = "Arquivos publicados" })).ToBeVisibleAsync();
        await Expect(page.Locator("h2", new() { HasText = "Limitações" })).ToBeVisibleAsync();

        Assert.True(errors.Count == 0, string.Join(" || ", errors));
        await page.CloseAsync();
    }
}
