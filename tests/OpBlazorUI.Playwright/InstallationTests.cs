using Microsoft.Playwright;
using Xunit;
using static Microsoft.Playwright.Assertions;

namespace OpBlazorUI.Playwright;

/// <summary>
/// Instalação: exemplos por modelo de hospedagem e tipografia padrão da lib.
/// </summary>
[Collection("showcase")]
public class InstallationTests
{
    private readonly ShowcaseFixture _fixture;

    public InstallationTests(ShowcaseFixture fixture) => _fixture = fixture;

    [Fact]
    public async Task Installation_exemplos_e_fonte_inter()
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

        await page.GotoAsync($"{_fixture.BaseUrl}/installation",
            new PageGotoOptions { WaitUntil = WaitUntilState.NetworkIdle });

        await Expect(page.Locator("h1")).ToContainTextAsync("Installation");

        foreach (var heading in new[]
                 {
                     "Tipografia",
                     "Escolha o modelo",
                     "Tema inicial e setup",
                     "Blazor Web App: Server",
                     "Blazor Web App: WebAssembly",
                     "Blazor Web App: Auto",
                     "WebAssembly standalone",
                     "MAUI Blazor Hybrid",
                     "SSR estático",
                     "Referência: modos de renderização"
                 })
        {
            // O h2 inclui o texto do link âncora (#) além do rótulo.
            await Expect(page.Locator("h2", new() { HasText = heading }).First)
                .ToBeVisibleAsync();
        }

        await Expect(page.Locator("h2#tipografia")).ToBeVisibleAsync();
        await Expect(page.Locator("code", new() { HasText = "--op-font-family" }).First)
            .ToBeVisibleAsync();

        var fontCss = await page.APIRequest.GetAsync(
            $"{_fixture.BaseUrl}/_content/OpBlazorUI/fonts/inter/inter.css");
        Assert.True(fontCss.Ok, $"CSS da fonte retornou {fontCss.Status}.");
        Assert.Contains("InterVariable.woff2", await fontCss.TextAsync());

        var family = await page.EvaluateAsync<string>(
            "() => getComputedStyle(document.documentElement).fontFamily");
        Assert.Contains("Inter var", family);

        await page.EvaluateAsync("() => document.fonts.ready");
        var interLoaded = await page.EvaluateAsync<bool>(
            "() => Array.from(document.fonts).some(f => f.family.includes('Inter') && f.status === 'loaded')");
        Assert.True(interLoaded, "A fonte Inter não foi carregada pelo navegador.");

        Assert.Empty(errors);
        await page.CloseAsync();
    }
}
