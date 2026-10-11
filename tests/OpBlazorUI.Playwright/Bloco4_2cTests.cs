using Microsoft.Playwright;
using Xunit;
using static Microsoft.Playwright.Assertions;

namespace OpBlazorUI.Playwright;

/// <summary>
/// Bloco 4.2c: SelectionLimit/SelectedItemsLabel (MultiSelect), Delay (AutoComplete),
/// Linear (Stepper) e Closable (Tabs). Usa a página <c>/_tests/bloco4-2c</c>.
/// </summary>
[Collection("showcase")]
public class Bloco4_2cTests
{
    private readonly ShowcaseFixture _fixture;

    public Bloco4_2cTests(ShowcaseFixture fixture) => _fixture = fixture;

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
        await page.GotoAsync($"{_fixture.BaseUrl}/_tests/bloco4-2c", new PageGotoOptions { WaitUntil = WaitUntilState.NetworkIdle });
        await page.Locator("#t-title").WaitForAsync();
        return page;
    }

    [Fact]
    public async Task MultiSelect_respeita_SelectionLimit_e_label_customizado()
    {
        var errors = new List<string>();
        var page = await NewPageAsync(errors);

        await page.Locator("#t-ms .p-multiselect").ClickAsync();
        var options = page.Locator("#t-ms .p-multiselect-option");
        await Expect(options.First).ToBeVisibleAsync();

        await options.Nth(0).ClickAsync();
        await options.Nth(1).ClickAsync();
        await options.Nth(2).ClickAsync();

        await Expect(page.Locator("#t-ms-log")).ToHaveTextAsync("2");
        await Expect(page.Locator("#t-ms .p-multiselect-label")).ToContainTextAsync("escolhidos");

        Assert.True(errors.Count == 0, string.Join(" || ", errors));
        await page.CloseAsync();
    }

    [Fact]
    public async Task AutoComplete_com_Delay_consulta_e_exibe()
    {
        var errors = new List<string>();
        var page = await NewPageAsync(errors);

        await page.Locator("#t-ac input[type=text]").FillAsync("Cidade 01");
        await Expect(page.Locator("#t-ac .p-autocomplete-option").First).ToBeVisibleAsync();

        Assert.True(errors.Count == 0, string.Join(" || ", errors));
        await page.CloseAsync();
    }

    [Fact]
    public async Task Stepper_linear_nao_pula_etapas()
    {
        var errors = new List<string>();
        var page = await NewPageAsync(errors);

        var steps = page.Locator("#t-step .p-step");
        await steps.Nth(2).ClickAsync();
        await Expect(page.Locator("#t-step-log")).ToHaveTextAsync("0");

        await steps.Nth(1).ClickAsync();
        await Expect(page.Locator("#t-step-log")).ToHaveTextAsync("1");

        Assert.True(errors.Count == 0, string.Join(" || ", errors));
        await page.CloseAsync();
    }

    [Fact]
    public async Task TabPanel_closable_dispara_OnClose()
    {
        var errors = new List<string>();
        var page = await NewPageAsync(errors);

        await Expect(page.Locator("#t-tabs-log")).ToHaveTextAsync("A,B,C");

        await page.Locator("#t-tabs .p-tab-close-icon").First.ClickAsync();
        await Expect(page.Locator("#t-tabs-log")).ToHaveTextAsync("B,C");

        Assert.True(errors.Count == 0, string.Join(" || ", errors));
        await page.CloseAsync();
    }

    [Fact]
    public async Task PickList_move_item_com_duplo_clique()
    {
        var errors = new List<string>();
        var page = await NewPageAsync(errors);

        await Expect(page.Locator("#t-pick-log")).ToHaveTextAsync("SP,RJ,PE|");

        var sourceOption = page.Locator("#t-pick .p-listbox").First.Locator(".p-listbox-option").First;
        await sourceOption.DblClickAsync();

        await Expect(page.Locator("#t-pick-log")).ToHaveTextAsync("RJ,PE|SP");

        Assert.True(errors.Count == 0, string.Join(" || ", errors));
        await page.CloseAsync();
    }
}
