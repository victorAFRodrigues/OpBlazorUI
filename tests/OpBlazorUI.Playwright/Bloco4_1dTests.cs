using Microsoft.Playwright;
using Xunit;
using static Microsoft.Playwright.Assertions;

namespace OpBlazorUI.Playwright;

/// <summary>
/// Bloco 4.1d (teclado do DatePicker). Usa a página <c>/_tests/bloco4-1d</c>.
/// </summary>
[Collection("showcase")]
public class Bloco4_1dTests
{
    private readonly ShowcaseFixture _fixture;

    public Bloco4_1dTests(ShowcaseFixture fixture) => _fixture = fixture;

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
        await page.GotoAsync($"{_fixture.BaseUrl}/_tests/bloco4-1d", new PageGotoOptions { WaitUntil = WaitUntilState.NetworkIdle });
        await page.Locator("#t-title").WaitForAsync();
        return page;
    }

    private static async Task ExpectActiveDateAsync(IPage page, string date)
    {
        const string js = "() => (document.activeElement && document.activeElement.getAttribute('data-date')) || ''";
        for (var i = 0; i < 50; i++)
        {
            var active = await page.EvaluateAsync<string>(js);
            if (active == date)
            {
                return;
            }

            await Task.Delay(100);
        }

        Assert.Equal(date, await page.EvaluateAsync<string>(js));
    }

    [Fact]
    public async Task DatePicker_navega_com_setas_e_paginas()
    {
        var errors = new List<string>();
        var page = await NewPageAsync(errors);

        await page.Locator("#t-dp-input").ClickAsync();
        await Expect(page.Locator("#t-dp .p-datepicker-day-view")).ToBeVisibleAsync();

        var day = page.Locator("#t-dp .p-datepicker-day[data-date='2026-01-15']");
        await Expect(day).ToHaveAttributeAsync("tabindex", "0");
        await day.FocusAsync();
        await ExpectActiveDateAsync(page, "2026-01-15");

        await page.Keyboard.PressAsync("ArrowRight");
        await ExpectActiveDateAsync(page, "2026-01-16");

        await page.Keyboard.PressAsync("ArrowDown");
        await ExpectActiveDateAsync(page, "2026-01-23");

        // PageDown vai para o mês seguinte mantendo o foco no mesmo dia.
        await page.Keyboard.PressAsync("PageDown");
        await ExpectActiveDateAsync(page, "2026-02-23");

        // Enter seleciona o dia focado.
        await page.Keyboard.PressAsync("Enter");
        await Expect(page.Locator("#t-dp-value")).ToHaveTextAsync("2026-02-23");

        Assert.True(errors.Count == 0, string.Join(" || ", errors));
        await page.CloseAsync();
    }
}
