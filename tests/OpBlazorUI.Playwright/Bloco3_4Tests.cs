using System.Text.RegularExpressions;
using Microsoft.Playwright;
using Xunit;
using static Microsoft.Playwright.Assertions;

namespace OpBlazorUI.Playwright;

/// <summary>
/// Bloco 3.4 (DatePicker). Usa a página <c>/_tests/bloco3-4</c> do Showcase.
/// </summary>
[Collection("showcase")]
public class Bloco3_4Tests
{
    private readonly ShowcaseFixture _fixture;

    public Bloco3_4Tests(ShowcaseFixture fixture) => _fixture = fixture;

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
        await page.GotoAsync($"{_fixture.BaseUrl}/_tests/bloco3-4", new PageGotoOptions { WaitUntil = WaitUntilState.NetworkIdle });
        await page.Locator("#t-title").WaitForAsync();
        return page;
    }

    private static string DayOf(int day) => $"{DateTime.Today:yyyy-MM}-{day:00}";

    [Fact]
    public async Task Range_clear_dispara_o_callback()
    {
        var errors = new List<string>();
        var page = await NewPageAsync(errors);

        await page.Locator($"#t-range-clear .p-datepicker-day[data-date='{DayOf(5)}']").ClickAsync();
        await page.Locator($"#t-range-clear .p-datepicker-day[data-date='{DayOf(10)}']").ClickAsync();
        await Expect(page.Locator("#t-range-value")).ToContainTextAsync("|");

        await page.Locator(".p-datepicker-clear-button").ClickAsync();
        await Expect(page.Locator("#t-range-value")).ToHaveTextAsync("");

        Assert.True(errors.Count == 0, string.Join(" || ", errors));
        await page.CloseAsync();
    }

    [Fact]
    public async Task Digitacao_usa_o_DateFormat()
    {
        var errors = new List<string>();
        var page = await NewPageAsync(errors);

        var input = page.Locator("#t-typing-input");
        await input.FillAsync("15/01/2026");
        await page.Keyboard.PressAsync("Tab");
        await Expect(page.Locator("#t-typing-value")).ToHaveTextAsync("2026-01-15");

        // Entrada parcial não é aceita (mantém o valor anterior).
        await input.FillAsync("15/01");
        await page.Keyboard.PressAsync("Tab");
        await Expect(page.Locator("#t-typing-value")).ToHaveTextAsync("2026-01-15");

        Assert.True(errors.Count == 0, string.Join(" || ", errors));
        await page.CloseAsync();
    }

    [Fact]
    public async Task MinDate_muda_e_reconstroi_a_grade()
    {
        var errors = new List<string>();
        var page = await NewPageAsync(errors);

        var before = page.Locator($"#t-min .p-datepicker-day[data-date='{DayOf(10)}']");
        await Expect(before).ToHaveClassAsync(new Regex("p-disabled"));

        await page.Locator("#t-min-toggle").ClickAsync();
        await Expect(before).Not.ToHaveClassAsync(new Regex("p-disabled"));

        Assert.True(errors.Count == 0, string.Join(" || ", errors));
        await page.CloseAsync();
    }

    [Fact]
    public async Task Reabrir_volta_para_a_view_configurada()
    {
        var errors = new List<string>();
        var page = await NewPageAsync(errors);

        await page.Locator("#t-view-input").ClickAsync();
        await Expect(page.Locator("#t-view .p-datepicker-month-view")).ToBeVisibleAsync();

        // Vai para a view de ano e fecha.
        await page.Locator("#t-view .p-datepicker-select-year").ClickAsync();
        await Expect(page.Locator("#t-view .p-datepicker-year-view")).ToBeVisibleAsync();
        await page.Keyboard.PressAsync("Escape");

        // Reabrir volta para a view configurada (Month).
        await page.Locator("#t-view-input").ClickAsync();
        await Expect(page.Locator("#t-view .p-datepicker-month-view")).ToBeVisibleAsync();

        Assert.True(errors.Count == 0, string.Join(" || ", errors));
        await page.CloseAsync();
    }
}
