using Microsoft.Playwright;
using Xunit;

namespace OpBlazorUI.Playwright;

/// <summary>
/// Modo escuro: claro por padrão mesmo com o SO escuro, escolha explícita persistida e o
/// contrato de data-dark/data-persist do op-theme.js.
/// </summary>
[Collection("showcase")]
public class ThemeModeTests
{
    private readonly ShowcaseFixture _fixture;

    public ThemeModeTests(ShowcaseFixture fixture) => _fixture = fixture;

    [Fact]
    public async Task Showcase_comeca_claro_e_persiste_a_escolha_explicita()
    {
        var errors = new List<string>();
        var page = await _fixture.Browser.NewPageAsync();
        page.SetDefaultTimeout(60000);
        page.Console += (_, msg) =>
        {
            if (msg.Type == "error") errors.Add(msg.Text);
        };
        page.PageError += (_, err) => errors.Add(err);

        await page.EmulateMediaAsync(new() { ColorScheme = ColorScheme.Dark });
        await page.GotoAsync($"{_fixture.BaseUrl}/", new PageGotoOptions { WaitUntil = WaitUntilState.NetworkIdle });
        await page.Locator("h1").WaitForAsync();

        Assert.False(await IsDarkAsync(page));
        await page.Locator(".topbar-item[title='Modo escuro']").ClickAsync();
        await page.WaitForFunctionAsync("() => document.documentElement.classList.contains('app-dark')");

        var storedDark = await page.EvaluateAsync<string>("() => localStorage.getItem('op-theme')");
        Assert.Contains("\"darkMode\":\"dark\"", storedDark);

        await page.ReloadAsync(new PageReloadOptions { WaitUntil = WaitUntilState.NetworkIdle });
        await page.Locator("h1").WaitForAsync();
        Assert.True(await IsDarkAsync(page));

        await page.Locator(".topbar-item[title='Modo claro']").ClickAsync();
        await page.WaitForFunctionAsync("() => !document.documentElement.classList.contains('app-dark')");

        var storedLight = await page.EvaluateAsync<string>("() => localStorage.getItem('op-theme')");
        Assert.Contains("\"darkMode\":\"light\"", storedLight);

        await page.ReloadAsync(new PageReloadOptions { WaitUntil = WaitUntilState.NetworkIdle });
        await page.Locator("h1").WaitForAsync();
        Assert.False(await IsDarkAsync(page));

        Assert.Empty(errors);
        await page.CloseAsync();
    }

    [Fact]
    public async Task Script_respeita_system_e_data_persist_false()
    {
        var page = await _fixture.Browser.NewPageAsync();
        page.SetDefaultTimeout(60000);
        await page.GotoAsync($"{_fixture.BaseUrl}/", new PageGotoOptions { WaitUntil = WaitUntilState.NetworkIdle });
        await page.EvaluateAsync(
            "() => localStorage.setItem('op-theme', JSON.stringify({ darkMode: 'dark' }))");

        await page.EmulateMediaAsync(new() { ColorScheme = ColorScheme.Dark });
        await SetThemeDocumentAsync(page, "data-dark=\"system\" data-persist=\"true\"");
        Assert.True(await IsDarkAsync(page));

        await SetThemeDocumentAsync(page, "data-dark=\"light\" data-persist=\"true\"");
        Assert.True(await IsDarkAsync(page), "Sem data-persist=false, o estado salvo deve vencer.");

        await SetThemeDocumentAsync(page, "data-dark=\"light\" data-persist=\"false\"");
        Assert.False(await IsDarkAsync(page), "data-persist=false deve ignorar o escuro salvo.");

        await SetThemeDocumentAsync(page, "data-dark=\"system\" data-persist=\"false\"");
        Assert.True(await IsDarkAsync(page));

        await page.CloseAsync();
    }

    private static Task<bool> IsDarkAsync(IPage page) =>
        page.EvaluateAsync<bool>("() => document.documentElement.classList.contains('app-dark')");

    private async Task SetThemeDocumentAsync(IPage page, string attributes)
    {
        // Mantém a origem do Showcase para o script resolver _content/... e o localStorage.
        await page.SetContentAsync(
            $"""
            <!DOCTYPE html>
            <html>
            <head>
                <base href="{_fixture.BaseUrl}/" />
                <script src="{_fixture.BaseUrl}/_content/OpBlazorUI/op-theme.js" {attributes}></script>
            </head>
            <body></body>
            </html>
            """,
            new PageSetContentOptions { WaitUntil = WaitUntilState.Load });
    }
}
