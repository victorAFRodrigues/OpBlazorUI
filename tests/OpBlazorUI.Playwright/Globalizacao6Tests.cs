using Microsoft.Playwright;
using Xunit;
using static Microsoft.Playwright.Assertions;

namespace OpBlazorUI.Playwright;

/// <summary>
/// Lote 6 da globalização (mídia). Usa a página <c>/_tests/globalizacao-6</c>, que fixa a tradução
/// em pt-BR, e verifica os <c>aria-label</c> de Image e Galleria.
/// </summary>
[Collection("showcase")]
public class Globalizacao6Tests
{
    private readonly ShowcaseFixture _fixture;

    public Globalizacao6Tests(ShowcaseFixture fixture) => _fixture = fixture;

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
        await page.GotoAsync($"{_fixture.BaseUrl}/_tests/globalizacao-6", new PageGotoOptions { WaitUntil = WaitUntilState.NetworkIdle });
        await page.Locator("#t-title").WaitForAsync();
        return page;
    }

    [Fact]
    public async Task Image_usa_a_traducao_pt_br()
    {
        var errors = new List<string>();
        var page = await NewPageAsync(errors);

        var preview = page.Locator("#t-image .p-image-preview-mask");
        await Expect(preview).ToHaveAttributeAsync("aria-label", "Ampliar imagem");
        await preview.ClickAsync();

        await Expect(page.Locator("#t-image .p-image-rotate-right-button")).ToHaveAttributeAsync("aria-label", "Girar para a direita");
        await Expect(page.Locator("#t-image .p-image-rotate-left-button")).ToHaveAttributeAsync("aria-label", "Girar para a esquerda");
        await Expect(page.Locator("#t-image .p-image-zoom-out-button")).ToHaveAttributeAsync("aria-label", "Reduzir");
        await Expect(page.Locator("#t-image .p-image-zoom-in-button")).ToHaveAttributeAsync("aria-label", "Ampliar");
        await Expect(page.Locator("#t-image .p-image-close-button")).ToHaveAttributeAsync("aria-label", "Fechar");

        Assert.True(errors.Count == 0, string.Join(" || ", errors));
        await page.CloseAsync();
    }

    [Fact]
    public async Task Galleria_usa_a_traducao_pt_br()
    {
        var errors = new List<string>();
        var page = await NewPageAsync(errors);

        await Expect(page.Locator("#t-galleria .p-galleria-prev-button")).ToHaveAttributeAsync("aria-label", "Primeira página");
        await Expect(page.Locator("#t-galleria .p-galleria-next-button")).ToHaveAttributeAsync("aria-label", "Próxima página");
        await Expect(page.Locator("#t-galleria .p-galleria-thumbnail-prev-button")).ToHaveAttributeAsync("aria-label", "Miniaturas anteriores");
        await Expect(page.Locator("#t-galleria .p-galleria-thumbnail-next-button")).ToHaveAttributeAsync("aria-label", "Próximas miniaturas");

        Assert.True(errors.Count == 0, string.Join(" || ", errors));
        await page.CloseAsync();
    }
}
