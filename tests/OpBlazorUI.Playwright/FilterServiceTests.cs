using Microsoft.Playwright;
using Xunit;
using static Microsoft.Playwright.Assertions;

namespace OpBlazorUI.Playwright;

[Collection("showcase")]
public class FilterServiceTests
{
    private readonly ShowcaseFixture _fixture;

    public FilterServiceTests(ShowcaseFixture fixture) => _fixture = fixture;

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
    public async Task FilterService_filtra_conforme_o_match_mode()
    {
        var errors = new List<string>();
        var page = await NewPageAsync(errors);

        await page.GotoAsync($"{_fixture.BaseUrl}/filterservice",
            new PageGotoOptions { WaitUntil = WaitUntilState.NetworkIdle });

        var rows = page.Locator("table tbody tr");
        var input = page.Locator("input[placeholder='Filtrar...']");
        var mode = page.Locator(".card select").First;
        await input.WaitForAsync();

        // startsWith (padrão): "an" não inicia nenhum nome/categoria.
        await input.FillAsync("an");
        await Expect(rows).ToHaveCountAsync(0);

        // contains: Banana e Orange.
        await mode.SelectOptionAsync("contains");
        await Expect(rows).ToHaveCountAsync(2);

        // endsWith: "an" não termina nenhum nome/categoria.
        await mode.SelectOptionAsync("endsWith");
        await Expect(rows).ToHaveCountAsync(0);

        // endsWith: "na" termina Banana.
        await input.FillAsync("na");
        await Expect(rows).ToHaveCountAsync(1);

        Assert.Empty(errors);
        await page.CloseAsync();
    }
}
