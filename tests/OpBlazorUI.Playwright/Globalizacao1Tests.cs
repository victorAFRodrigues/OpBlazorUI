using Microsoft.Playwright;
using Xunit;
using static Microsoft.Playwright.Assertions;

namespace OpBlazorUI.Playwright;

/// <summary>
/// Lote 1 da globalização (ações/aria). Usa a página <c>/_tests/globalizacao-1</c>, que fixa a
/// tradução em pt-BR, e verifica que rótulos e <c>aria-label</c> vêm da tradução global.
/// </summary>
[Collection("showcase")]
public class Globalizacao1Tests
{
    private readonly ShowcaseFixture _fixture;

    public Globalizacao1Tests(ShowcaseFixture fixture) => _fixture = fixture;

    private async Task<IPage> NewPageAsync(List<string> consoleErrors)
    {
        var page = await _fixture.Browser.NewPageAsync();
        page.SetDefaultTimeout(60000);
        page.Console += (_, msg) =>
        {
            if (msg.Type == "error")
            {
                consoleErrors.Add(msg.Text);
            }
        };
        await page.GotoAsync($"{_fixture.BaseUrl}/_tests/globalizacao-1", new PageGotoOptions { WaitUntil = WaitUntilState.NetworkIdle });
        await page.Locator("#t-title").WaitForAsync();
        return page;
    }

    [Fact]
    public async Task Acoes_e_aria_usam_a_traducao_pt_br()
    {
        var errors = new List<string>();
        var page = await NewPageAsync(errors);

        await Expect(page.Locator("#t-message .p-message-close-button")).ToHaveAttributeAsync("aria-label", "Fechar");
        await Expect(page.Locator("#t-chip .p-chip-remove-icon")).ToHaveAttributeAsync("aria-label", "Remover");
        await Expect(page.Locator("#t-inputchips .p-chip-remove-icon").First).ToHaveAttributeAsync("aria-label", "Remover");
        await Expect(page.Locator("#t-inplace .p-inplace-content button")).ToHaveAttributeAsync("aria-label", "Fechar");

        await Expect(page.Locator("#t-fileupload .p-button-label")).ToHaveTextAsync("Escolher");
        await Expect(page.Locator("#t-fileupload")).ToContainTextAsync("Nenhum arquivo escolhido");

        await Expect(page.Locator("#t-dialog .p-dialog-maximize-button")).ToHaveAttributeAsync("aria-label", "Maximizar");
        await Expect(page.Locator("#t-dialog .p-dialog-close-button")).ToHaveAttributeAsync("aria-label", "Fechar");

        Assert.True(errors.Count == 0, string.Join(" || ", errors));
        await page.CloseAsync();
    }

    [Fact]
    public async Task ConfirmDialog_usa_a_traducao_pt_br()
    {
        var errors = new List<string>();
        var page = await NewPageAsync(errors);

        await page.Locator("#t-confirm-open").ClickAsync();
        await Expect(page.Locator(".p-confirmdialog-reject-button")).ToContainTextAsync("Não");
        await Expect(page.Locator(".p-confirmdialog-accept-button")).ToContainTextAsync("Sim");

        Assert.True(errors.Count == 0, string.Join(" || ", errors));
        await page.CloseAsync();
    }
}
