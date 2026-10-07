using Microsoft.Playwright;
using Xunit;
using static Microsoft.Playwright.Assertions;

namespace OpBlazorUI.Playwright;

[Collection("showcase")]
public class DirectivesTests
{
    private readonly ShowcaseFixture _fixture;

    public DirectivesTests(ShowcaseFixture fixture) => _fixture = fixture;

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
        return page;
    }

    [Fact]
    public async Task AutoFocus_foca_o_campo_ao_montar()
    {
        var errors = new List<string>();
        var page = await NewPageAsync(errors);

        await page.GotoAsync($"{_fixture.BaseUrl}/autofocus",
            new PageGotoOptions { WaitUntil = WaitUntilState.NetworkIdle });

        var input = page.Locator("input[placeholder='Focado automaticamente']");
        await input.WaitForAsync();
        await Expect(input).ToBeFocusedAsync();

        Assert.Empty(errors);
        await page.CloseAsync();
    }

    [Fact]
    public async Task Ripple_cria_o_ink_no_clique()
    {
        var errors = new List<string>();
        var page = await NewPageAsync(errors);

        await page.GotoAsync($"{_fixture.BaseUrl}/ripple",
            new PageGotoOptions { WaitUntil = WaitUntilState.NetworkIdle });

        var box = page.Locator(".ripple-box").First;
        await box.WaitForAsync();
        await box.ClickAsync();

        Assert.True(await box.Locator(".p-ink").CountAsync() >= 1, "O ripple deve criar um .p-ink no clique.");

        Assert.Empty(errors);
        await page.CloseAsync();
    }

    [Fact]
    public async Task StyleClass_alterna_a_classe_do_alvo()
    {
        var errors = new List<string>();
        var page = await NewPageAsync(errors);

        await page.GotoAsync($"{_fixture.BaseUrl}/styleclass",
            new PageGotoOptions { WaitUntil = WaitUntilState.NetworkIdle });

        var input = page.Locator("input.p-inputtext").First;
        var toggle = page.Locator("button:has-text('Toggle Display')").First;
        await toggle.WaitForAsync();

        await Expect(input).ToBeHiddenAsync();
        await toggle.ClickAsync();
        await Expect(input).ToBeVisibleAsync();
        await toggle.ClickAsync();
        await Expect(input).ToBeHiddenAsync();

        Assert.Empty(errors);
        await page.CloseAsync();
    }
}
