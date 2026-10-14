using Microsoft.Playwright;
using Xunit;
using static Microsoft.Playwright.Assertions;

namespace OpBlazorUI.Playwright;

/// <summary>
/// Bloco 3.1 (seleção): OptionValue (item G), navegação com grupos, etc.
/// Usa a página <c>/_tests/bloco3-1</c> do Showcase.
/// </summary>
[Collection("showcase")]
public class Bloco3_1Tests
{
    private readonly ShowcaseFixture _fixture;

    public Bloco3_1Tests(ShowcaseFixture fixture) => _fixture = fixture;

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
        await page.GotoAsync($"{_fixture.BaseUrl}/_tests/bloco3-1", new PageGotoOptions { WaitUntil = WaitUntilState.NetworkIdle });
        await page.Locator("#t-title").WaitForAsync();
        return page;
    }

    private static ILocator Option(IPage page, string scope, string cls, string text)
        => page.Locator($"{scope} .{cls}").Filter(new LocatorFilterOptions { HasText = text }).First;

    [Fact]
    public async Task Select_grava_o_OptionValue()
    {
        var errors = new List<string>();
        var page = await NewPageAsync(errors);

        await page.Locator("#t-select .p-select-label").ClickAsync();
        await Option(page, "#t-select", "p-select-option", "São Paulo").ClickAsync();

        await Expect(page.Locator("#t-select-value")).ToHaveTextAsync("SP");
        await Expect(page.Locator("#t-select .p-select-label")).ToHaveTextAsync("São Paulo");

        Assert.Empty(errors);
        await page.CloseAsync();
    }

    [Fact]
    public async Task Select_agrupado_por_teclado_grava_o_OptionValue()
    {
        var errors = new List<string>();
        var page = await NewPageAsync(errors);

        var label = page.Locator("#t-select-group .p-select-label");
        await label.ClickAsync();
        await page.Keyboard.PressAsync("ArrowDown");
        await page.Keyboard.PressAsync("Enter");

        // A navegação percorre os filhos achatados; o primeiro é São Paulo (SP).
        await Expect(page.Locator("#t-select-group-value")).ToHaveTextAsync("SP");

        Assert.Empty(errors);
        await page.CloseAsync();
    }

    [Fact]
    public async Task MultiSelect_grava_o_OptionValue()
    {
        var errors = new List<string>();
        var page = await NewPageAsync(errors);

        await page.Locator("#t-multiselect .p-multiselect-label-container").ClickAsync();
        await Option(page, "#t-multiselect", "p-multiselect-option", "Rio de Janeiro").ClickAsync();
        await Option(page, "#t-multiselect", "p-multiselect-option", "Curitiba").ClickAsync();

        await Expect(page.Locator("#t-multiselect-value")).ToHaveTextAsync("RJ,PR");

        Assert.Empty(errors);
        await page.CloseAsync();
    }

    [Fact]
    public async Task Listbox_grava_o_OptionValue()
    {
        var errors = new List<string>();
        var page = await NewPageAsync(errors);

        await Option(page, "#t-listbox", "p-listbox-option", "Belo Horizonte").ClickAsync();
        await Option(page, "#t-listbox", "p-listbox-option", "Curitiba").ClickAsync();

        await Expect(page.Locator("#t-listbox-value")).ToHaveTextAsync("MG,PR");

        Assert.Empty(errors);
        await page.CloseAsync();
    }

    [Fact]
    public async Task AutoComplete_grava_o_OptionValue()
    {
        var errors = new List<string>();
        var page = await NewPageAsync(errors);

        await page.Locator("#t-autocomplete input[type=text]").FillAsync("São");
        await Option(page, "#t-autocomplete", "p-autocomplete-option", "São Paulo").ClickAsync();

        await Expect(page.Locator("#t-autocomplete-value")).ToHaveTextAsync("SP");

        Assert.Empty(errors);
        await page.CloseAsync();
    }

    [Fact]
    public async Task SelectButton_grava_o_OptionValue()
    {
        var errors = new List<string>();
        var page = await NewPageAsync(errors);

        await Option(page, "#t-selectbutton", "p-togglebutton", "Curitiba").ClickAsync();

        await Expect(page.Locator("#t-selectbutton-value")).ToHaveTextAsync("PR");

        Assert.Empty(errors);
        await page.CloseAsync();
    }
}
