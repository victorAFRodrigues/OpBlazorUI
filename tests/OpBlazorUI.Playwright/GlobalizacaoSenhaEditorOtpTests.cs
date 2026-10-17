using Microsoft.Playwright;
using Xunit;
using static Microsoft.Playwright.Assertions;

namespace OpBlazorUI.Playwright;

/// <summary>
/// Globalização (senha, editor e OTP). Usa a página
/// <c>/_tests/globalizacao-senha-editor-otp</c>, que fixa a tradução em pt-BR, e verifica o
/// medidor de senha, a toolbar do editor e os campos do OTP.
/// </summary>
[Collection("showcase")]
public class GlobalizacaoSenhaEditorOtpTests
{
    private readonly ShowcaseFixture _fixture;

    public GlobalizacaoSenhaEditorOtpTests(ShowcaseFixture fixture) => _fixture = fixture;

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
        await page.GotoAsync($"{_fixture.BaseUrl}/_tests/globalizacao-senha-editor-otp", new PageGotoOptions { WaitUntil = WaitUntilState.NetworkIdle });
        await page.Locator("#t-title").WaitForAsync();
        return page;
    }

    [Fact]
    public async Task Password_usa_a_traducao_pt_br()
    {
        var errors = new List<string>();
        var page = await NewPageAsync(errors);

        var input = page.Locator("#t-password input");
        await input.FocusAsync();

        // Valor "abc" não casa com os regex de força → nível fraco.
        var meterText = page.Locator("#t-password .p-password-meter + div");
        await Expect(meterText).ToHaveTextAsync("Fraca");

        // Sem valor, o medidor mostra o prompt.
        await input.FillAsync("");
        await Expect(meterText).ToHaveTextAsync("Digite uma senha");

        Assert.True(errors.Count == 0, string.Join(" || ", errors));
        await page.CloseAsync();
    }

    [Fact]
    public async Task Editor_usa_a_traducao_pt_br()
    {
        var errors = new List<string>();
        var page = await NewPageAsync(errors);

        await Expect(page.Locator("#t-editor .ql-bold")).ToHaveAttributeAsync("aria-label", "Negrito");
        await Expect(page.Locator("#t-editor .ql-italic")).ToHaveAttributeAsync("aria-label", "Itálico");
        await Expect(page.Locator("#t-editor .ql-underline")).ToHaveAttributeAsync("aria-label", "Sublinhado");
        await Expect(page.Locator("#t-editor select.ql-color")).ToHaveAttributeAsync("aria-label", "Cor do texto");
        await Expect(page.Locator("#t-editor select.ql-background")).ToHaveAttributeAsync("aria-label", "Cor de fundo");
        await Expect(page.Locator("#t-editor .ql-list[value=ordered]")).ToHaveAttributeAsync("aria-label", "Lista ordenada");
        await Expect(page.Locator("#t-editor .ql-list[value=bullet]")).ToHaveAttributeAsync("aria-label", "Lista não ordenada");
        await Expect(page.Locator("#t-editor select.ql-align")).ToHaveAttributeAsync("aria-label", "Alinhamento");
        await Expect(page.Locator("#t-editor .ql-link")).ToHaveAttributeAsync("aria-label", "Inserir link");
        await Expect(page.Locator("#t-editor .ql-code-block")).ToHaveAttributeAsync("aria-label", "Bloco de código");
        await Expect(page.Locator("#t-editor .ql-clean")).ToHaveAttributeAsync("aria-label", "Limpar formatação");

        // O Quill troca o <select class="ql-header"> por um picker; os rótulos vêm das nossas <option>.
        await Expect(page.Locator("#t-editor .ql-header .ql-picker-item[data-value='1']")).ToHaveAttributeAsync("data-label", "Título");
        await Expect(page.Locator("#t-editor .ql-header .ql-picker-item[data-value='2']")).ToHaveAttributeAsync("data-label", "Subtítulo");

        Assert.True(errors.Count == 0, string.Join(" || ", errors));
        await page.CloseAsync();
    }

    [Fact]
    public async Task InputOtp_usa_a_traducao_pt_br()
    {
        var errors = new List<string>();
        var page = await NewPageAsync(errors);

        var inputs = page.Locator("#t-inputotp input");
        await Expect(inputs).ToHaveCountAsync(4);
        await Expect(inputs.Nth(0)).ToHaveAttributeAsync("aria-label", "Caractere 1");
        await Expect(inputs.Nth(3)).ToHaveAttributeAsync("aria-label", "Caractere 4");

        Assert.True(errors.Count == 0, string.Join(" || ", errors));
        await page.CloseAsync();
    }
}
