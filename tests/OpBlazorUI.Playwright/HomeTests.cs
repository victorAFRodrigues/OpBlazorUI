using Microsoft.Playwright;
using Xunit;
using static Microsoft.Playwright.Assertions;

namespace OpBlazorUI.Playwright;

/// <summary>
/// Home: os primeiros passos apontam para os guias de instalação e configuração, e não para
/// testar um componente isolado.
/// </summary>
[Collection("showcase")]
public class HomeTests
{
    private readonly ShowcaseFixture _fixture;

    public HomeTests(ShowcaseFixture fixture) => _fixture = fixture;

    [Fact]
    public async Task Home_aponta_para_instalacao_e_configuracao()
    {
        var page = await _fixture.Browser.NewPageAsync();
        page.SetDefaultTimeout(60000);
        await page.GotoAsync($"{_fixture.BaseUrl}/", new PageGotoOptions { WaitUntil = WaitUntilState.NetworkIdle });

        await Expect(page.Locator("h1")).ToContainTextAsync("OpBlazorUI");
        await Expect(page.Locator("a[href='installation']").First).ToBeVisibleAsync();
        await Expect(page.Locator("a[href='configuration']").First).ToBeVisibleAsync();
        await Expect(page.Locator("a[href='installation']").First).ToContainTextAsync("Instalação");
        await Expect(page.Locator("a[href='configuration']").First).ToContainTextAsync("Configuração");

        var content = await page.Locator(".doc-section").InnerTextAsync();
        Assert.DoesNotContain("Comece pelo Button", content);

        await page.CloseAsync();
    }
}
