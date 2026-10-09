using Microsoft.Playwright;
using Xunit;
using static Microsoft.Playwright.Assertions;

namespace OpBlazorUI.Playwright;

/// <summary>
/// Bloco 3.2 (formulários): RadioButton no EditContext, Checkbox readonly, Textarea com value
/// e notificação de seleção múltipla. Usa a página <c>/_tests/bloco3-2</c> do Showcase.
/// </summary>
[Collection("showcase")]
public class Bloco3_2Tests
{
    private readonly ShowcaseFixture _fixture;

    public Bloco3_2Tests(ShowcaseFixture fixture) => _fixture = fixture;

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
        await page.GotoAsync($"{_fixture.BaseUrl}/_tests/bloco3-2", new PageGotoOptions { WaitUntil = WaitUntilState.NetworkIdle });
        await page.Locator("#t-title").WaitForAsync();
        return page;
    }

    [Fact]
    public async Task RadioButton_renderiza_value_e_integra_com_o_modelo()
    {
        var errors = new List<string>();
        var page = await NewPageAsync(errors);

        await Expect(page.Locator("#r-a")).ToHaveAttributeAsync("value", "A");
        await page.Locator("#r-a").ClickAsync();
        await Expect(page.Locator("#t-radio-value")).ToHaveTextAsync("A");

        await page.Locator("#r-b").ClickAsync();
        await Expect(page.Locator("#t-radio-value")).ToHaveTextAsync("B");

        Assert.True(errors.Count == 0, string.Join(" || ", errors));
        await page.CloseAsync();
    }

    [Fact]
    public async Task Checkbox_readonly_nao_altera_pelo_navegador()
    {
        var errors = new List<string>();
        var page = await NewPageAsync(errors);

        var checkbox = page.Locator("#t-checkbox input[type=checkbox]");
        await checkbox.ClickAsync();
        await Task.Delay(150);

        Assert.False(await checkbox.IsCheckedAsync());
        await Expect(page.Locator("#t-checkbox-value")).ToHaveTextAsync("");

        Assert.True(errors.Count == 0, string.Join(" || ", errors));
        await page.CloseAsync();
    }

    [Fact]
    public async Task Textarea_limpa_quando_o_pai_limpa()
    {
        var errors = new List<string>();
        var page = await NewPageAsync(errors);

        var textarea = page.Locator("#t-textarea textarea");
        await textarea.FillAsync("abc");
        await Expect(page.Locator("#t-textarea-value")).ToHaveTextAsync("abc");

        await page.Locator("#t-textarea-clear").ClickAsync();
        await Expect(page.Locator("#t-textarea-value")).ToHaveTextAsync("");
        await Expect(textarea).ToHaveValueAsync("");

        Assert.True(errors.Count == 0, string.Join(" || ", errors));
        await page.CloseAsync();
    }

    [Fact]
    public async Task Listbox_multiplo_notifica_o_EditContext()
    {
        var errors = new List<string>();
        var page = await NewPageAsync(errors);

        var validation = page.Locator("#t-list-edit .validation-message");

        // Submete inválido: o validador roda e a mensagem aparece.
        await page.Locator("#t-list-submit").ClickAsync();
        await Expect(validation).ToHaveTextAsync("Escolha ao menos uma");

        // Ao selecionar, o Listbox notifica o EditContext e a mensagem some sem novo submit.
        await page.Locator("#t-list-edit .p-listbox-option").First.ClickAsync();
        await Expect(validation).ToHaveCountAsync(0);

        Assert.True(errors.Count == 0, string.Join(" || ", errors));
        await page.CloseAsync();
    }
}
