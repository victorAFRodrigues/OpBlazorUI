using Microsoft.Playwright;
using Xunit;
using static Microsoft.Playwright.Assertions;

namespace OpBlazorUI.Playwright;

/// <summary>
/// Bloco 3.3 (entrada de texto e números). Usa a página <c>/_tests/bloco3-3</c> do Showcase.
/// </summary>
[Collection("showcase")]
public class Bloco3_3Tests
{
    private readonly ShowcaseFixture _fixture;

    public Bloco3_3Tests(ShowcaseFixture fixture) => _fixture = fixture;

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
        await page.GotoAsync($"{_fixture.BaseUrl}/_tests/bloco3-3", new PageGotoOptions { WaitUntil = WaitUntilState.NetworkIdle });
        await page.Locator("#t-title").WaitForAsync();
        return page;
    }

    [Fact]
    public async Task ToggleButton_usa_nbsp_e_nao_o_texto_literal()
    {
        var errors = new List<string>();
        var page = await NewPageAsync(errors);

        var label = await page.Locator("#t-toggle .p-togglebutton-label").InnerTextAsync();
        Assert.DoesNotContain("nbsp", label);
        Assert.Equal("", label.Trim());

        Assert.True(errors.Count == 0, string.Join(" || ", errors));
        await page.CloseAsync();
    }

    [Fact]
    public async Task Rating_muda_por_teclado()
    {
        var errors = new List<string>();
        var page = await NewPageAsync(errors);

        await page.Locator("#t-rating input[value='1']").FocusAsync();
        await page.Keyboard.PressAsync("ArrowRight");

        await Expect(page.Locator("#t-rating-value")).ToHaveTextAsync("2");

        Assert.True(errors.Count == 0, string.Join(" || ", errors));
        await page.CloseAsync();
    }

    [Fact]
    public async Task InputText_KeyFilter_int_aceita_negativo_e_remove_invalido()
    {
        var errors = new List<string>();
        var page = await NewPageAsync(errors);

        var input = page.Locator("#t-ki");
        await input.FillAsync("a1");
        await Expect(input).ToHaveValueAsync("1");

        await input.FillAsync("-5");
        await Expect(input).ToHaveValueAsync("-5");

        Assert.True(errors.Count == 0, string.Join(" || ", errors));
        await page.CloseAsync();
    }

    [Fact]
    public async Task InputText_mask_aplica()
    {
        var errors = new List<string>();
        var page = await NewPageAsync(errors);

        var input = page.Locator("#t-mask input");
        await input.FillAsync("1234");
        await Expect(input).ToHaveValueAsync("12/34");
        await Expect(page.Locator("#t-mask-value")).ToHaveTextAsync("12/34");

        Assert.True(errors.Count == 0, string.Join(" || ", errors));
        await page.CloseAsync();
    }

    [Fact]
    public async Task InputChips_virgula_nao_duplica()
    {
        var errors = new List<string>();
        var page = await NewPageAsync(errors);

        var input = page.Locator("#t-chips-input");
        await input.FillAsync("a,");
        await input.FillAsync("b,");

        await Expect(page.Locator("#t-chips-value")).ToHaveTextAsync("a|b");

        Assert.True(errors.Count == 0, string.Join(" || ", errors));
        await page.CloseAsync();
    }

    [Fact]
    public async Task InputChips_respeita_SeparatorKeys()
    {
        var errors = new List<string>();
        var page = await NewPageAsync(errors);

        var input = page.Locator("#t-chips-sep-input");
        await input.FillAsync("x,");
        await Expect(page.Locator("#t-chips-sep-value")).ToHaveTextAsync("");

        await input.FillAsync("x;");
        await Expect(page.Locator("#t-chips-sep-value")).ToHaveTextAsync("x");

        Assert.True(errors.Count == 0, string.Join(" || ", errors));
        await page.CloseAsync();
    }

    [Fact]
    public async Task InputChips_readonly_nao_permite_remover()
    {
        var errors = new List<string>();
        var page = await NewPageAsync(errors);

        await Expect(page.Locator("#t-chips-ro .p-chip-remove-icon")).ToHaveCountAsync(0);
        await Expect(page.Locator("#t-chips-ro .p-chip-label")).ToHaveTextAsync("fixo");

        await page.Locator("#t-chips-ro .p-chip").ClickAsync();
        await Task.Delay(150);
        await Expect(page.Locator("#t-chips-ro .p-chip-label")).ToHaveTextAsync("fixo");

        Assert.True(errors.Count == 0, string.Join(" || ", errors));
        await page.CloseAsync();
    }

    [Fact]
    public async Task InputNumber_currency_parse_e_MaxFractionDigits()
    {
        var errors = new List<string>();
        var page = await NewPageAsync(errors);

        var input = page.Locator("#t-money input");
        await input.FillAsync("R$ 10.5");
        await page.Keyboard.PressAsync("Tab");
        await Expect(page.Locator("#t-money-value")).ToHaveTextAsync("10.5");

        await input.FillAsync("1.239");
        await page.Keyboard.PressAsync("Tab");
        await Expect(page.Locator("#t-money-value")).ToHaveTextAsync("1.24");

        Assert.True(errors.Count == 0, string.Join(" || ", errors));
        await page.CloseAsync();
    }
}
