using Microsoft.Playwright;
using Xunit;
using static Microsoft.Playwright.Assertions;

namespace OpBlazorUI.Playwright;

[Collection("showcase")]
public class P1DadosTests
{
    private readonly ShowcaseFixture _fixture;

    public P1DadosTests(ShowcaseFixture fixture) => _fixture = fixture;

    private async Task<IPage> NewPageAsync(List<string> consoleErrors, string route)
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
        await page.GotoAsync($"{_fixture.BaseUrl}{route}", new PageGotoOptions { WaitUntil = WaitUntilState.NetworkIdle });
        return page;
    }

    [Fact]
    public async Task Tree_expande_e_seleciona()
    {
        var errors = new List<string>();
        var page = await NewPageAsync(errors, "/tree");

        var firstTree = page.Locator(".p-tree").First;
        await firstTree.WaitForAsync();
        await firstTree.Locator(".p-tree-node-toggle-button").First.ClickAsync();
        await Expect(firstTree.Locator(".p-tree-node-children").First).ToBeVisibleAsync();

        await firstTree.Locator(".p-tree-node-content").First.ClickAsync();
        await Expect(firstTree.Locator(".p-tree-node-selected").First).ToBeVisibleAsync();

        Assert.Empty(errors);
        await page.CloseAsync();
    }

    [Fact]
    public async Task TreeTable_expande_linha()
    {
        var errors = new List<string>();
        var page = await NewPageAsync(errors, "/treetable");

        var table = page.Locator(".p-treetable").First;
        await table.WaitForAsync();
        var rows = table.Locator(".p-treetable-tbody .p-treetable-row");
        var before = await rows.CountAsync();

        await table.Locator(".p-treetable-node-toggle-button").First.ClickAsync();
        await Task.Delay(200);

        Assert.NotEqual(before, await rows.CountAsync());

        Assert.Empty(errors);
        await page.CloseAsync();
    }

    [Fact]
    public async Task DataView_renderiza_conteudo()
    {
        var errors = new List<string>();
        var page = await NewPageAsync(errors, "/dataview");

        await Expect(page.Locator(".p-dataview").First).ToBeVisibleAsync();
        await Expect(page.Locator(".p-dataview-content").First).ToBeVisibleAsync();

        Assert.Empty(errors);
        await page.CloseAsync();
    }

    [Fact]
    public async Task OrderList_move_item_selecionado()
    {
        var errors = new List<string>();
        var page = await NewPageAsync(errors, "/orderlist");

        var root = page.Locator(".p-orderlist").First;
        await root.WaitForAsync();
        await Expect(root.Locator(".p-orderlist-controls")).ToBeVisibleAsync();

        var firstOption = root.Locator(".p-listbox-option").Nth(1);
        var label = (await firstOption.InnerTextAsync()).Trim();
        await firstOption.ClickAsync();
        await Task.Delay(250);

        await root.Locator(".p-orderlist-controls button").First.ClickAsync();
        await Task.Delay(250);

        var firstLabelAfter = (await root.Locator(".p-listbox-option").First.InnerTextAsync()).Trim();
        Assert.True(firstLabelAfter == label, "O item selecionado deveria ir para o topo.");

        Assert.Empty(errors);
        await page.CloseAsync();
    }

    [Fact]
    public async Task PickList_transfere_item()
    {
        var errors = new List<string>();
        var page = await NewPageAsync(errors, "/picklist");

        var root = page.Locator(".p-picklist").First;
        await root.WaitForAsync();
        Assert.True(await root.Locator(".p-picklist-list-container").CountAsync() >= 2);

        var sourceCountBefore = await root.Locator(".p-picklist-list-container").First.Locator(".p-listbox-option").CountAsync();
        await root.Locator(".p-picklist-list-container").First.Locator(".p-listbox-option").First.ClickAsync();
        // Grupo de controles de transferência fica entre as duas listas (nth(1)).
        await root.Locator(".p-picklist-controls").Nth(1).Locator("button").First.ClickAsync();
        await Task.Delay(150);

        Assert.True(await root.Locator(".p-picklist-list-container").First.Locator(".p-listbox-option").CountAsync() < sourceCountBefore);

        Assert.Empty(errors);
        await page.CloseAsync();
    }

    [Fact]
    public async Task ConfirmPopup_abre_ancorado()
    {
        var errors = new List<string>();
        var page = await NewPageAsync(errors, "/confirmpopup");

        var trigger = page.Locator("button:has-text('Salvar')").First;
        await trigger.WaitForAsync();
        await trigger.ClickAsync();

        await Expect(page.Locator(".p-confirmpopup").First).ToBeVisibleAsync();

        Assert.Empty(errors);
        await page.CloseAsync();
    }

    [Fact]
    public async Task InlineMessage_renderiza_severidades()
    {
        var errors = new List<string>();
        var page = await NewPageAsync(errors, "/inlinemessage");

        await Expect(page.Locator(".p-inlinemessage").First).ToBeVisibleAsync();
        await Expect(page.Locator(".p-inlinemessage-info").First).ToBeVisibleAsync();
        await Expect(page.Locator(".p-inlinemessage-error").First).ToBeVisibleAsync();

        Assert.Empty(errors);
        await page.CloseAsync();
    }

    [Fact]
    public async Task MeterGroup_renderiza_medidores_e_rotulos()
    {
        var errors = new List<string>();
        var page = await NewPageAsync(errors, "/metergroup");

        await Expect(page.Locator(".p-metergroup").First).ToBeVisibleAsync();
        Assert.True(await page.Locator(".p-metergroup-meter").CountAsync() >= 1);
        Assert.True(await page.Locator(".p-metergroup-label").CountAsync() >= 1);

        Assert.Empty(errors);
        await page.CloseAsync();
    }

    [Fact]
    public async Task Timeline_renderiza_eventos()
    {
        var errors = new List<string>();
        var page = await NewPageAsync(errors, "/timeline");

        await Expect(page.Locator(".p-timeline-event").First).ToBeVisibleAsync();
        Assert.True(await page.Locator(".p-timeline-event").CountAsync() >= 2);

        Assert.Empty(errors);
        await page.CloseAsync();
    }
}
