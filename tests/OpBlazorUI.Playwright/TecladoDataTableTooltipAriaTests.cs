using Microsoft.Playwright;
using Xunit;
using static Microsoft.Playwright.Assertions;

namespace OpBlazorUI.Playwright;

/// <summary>
/// Teclado e ARIA avulsos: ordenação do DataTable, Escape do Tooltip, aria-checked
/// mixed do Checkbox e role spinbutton do InputNumber. Usa a página
/// <c>/_tests/teclado-data-table-tooltip-aria</c>.
/// </summary>
[Collection("showcase")]
public class TecladoDataTableTooltipAriaTests
{
    private readonly ShowcaseFixture _fixture;

    public TecladoDataTableTooltipAriaTests(ShowcaseFixture fixture) => _fixture = fixture;

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
        await page.GotoAsync($"{_fixture.BaseUrl}/_tests/teclado-data-table-tooltip-aria", new PageGotoOptions { WaitUntil = WaitUntilState.NetworkIdle });
        await page.Locator("#t-title").WaitForAsync();
        return page;
    }

    [Fact]
    public async Task DataTable_ordena_por_teclado()
    {
        var errors = new List<string>();
        var page = await NewPageAsync(errors);

        var header = page.Locator("#t-dt th").First;
        await Expect(header).ToHaveAttributeAsync("aria-sort", "none");
        await header.FocusAsync();

        await page.Keyboard.PressAsync("Enter");
        await Expect(header).ToHaveAttributeAsync("aria-sort", "ascending");

        await page.Keyboard.PressAsync("Enter");
        await Expect(header).ToHaveAttributeAsync("aria-sort", "descending");

        Assert.True(errors.Count == 0, string.Join(" || ", errors));
        await page.CloseAsync();
    }

    [Fact]
    public async Task Tooltip_fecha_com_Escape()
    {
        var errors = new List<string>();
        var page = await NewPageAsync(errors);

        await page.Locator("#t-tt-target").FocusAsync();
        await Expect(page.Locator("#t-tt [role=tooltip]")).ToBeVisibleAsync();

        await page.Keyboard.PressAsync("Escape");
        await Expect(page.Locator("#t-tt [role=tooltip]")).ToHaveCountAsync(0);

        Assert.True(errors.Count == 0, string.Join(" || ", errors));
        await page.CloseAsync();
    }

    [Fact]
    public async Task Checkbox_indeterminado_expoe_aria_checked_mixed()
    {
        var errors = new List<string>();
        var page = await NewPageAsync(errors);

        await Expect(page.Locator("#t-cb input[type=checkbox]")).ToHaveAttributeAsync("aria-checked", "mixed");

        Assert.True(errors.Count == 0, string.Join(" || ", errors));
        await page.CloseAsync();
    }

    [Fact]
    public async Task InputNumber_expoe_role_spinbutton()
    {
        var errors = new List<string>();
        var page = await NewPageAsync(errors);

        var input = page.Locator("#t-in input");
        await Expect(input).ToHaveAttributeAsync("role", "spinbutton");
        await Expect(input).ToHaveAttributeAsync("aria-valuemin", "0");
        await Expect(input).ToHaveAttributeAsync("aria-valuemax", "100");

        Assert.True(errors.Count == 0, string.Join(" || ", errors));
        await page.CloseAsync();
    }
}
