using Microsoft.Playwright;
using Xunit;
using static Microsoft.Playwright.Assertions;

namespace OpBlazorUI.Playwright;

/// <summary>
/// Globalização (dados e árvore). Usa a página <c>/_tests/globalizacao-dados-arvore</c>, que
/// fixa a tradução em pt-BR, e verifica vazio, aria de filtro e o comparador ordinal.
/// </summary>
[Collection("showcase")]
public class GlobalizacaoDadosArvoreTests
{
    private readonly ShowcaseFixture _fixture;

    public GlobalizacaoDadosArvoreTests(ShowcaseFixture fixture) => _fixture = fixture;

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
        await page.GotoAsync($"{_fixture.BaseUrl}/_tests/globalizacao-dados-arvore", new PageGotoOptions { WaitUntil = WaitUntilState.NetworkIdle });
        await page.Locator("#t-title").WaitForAsync();
        return page;
    }

    [Fact]
    public async Task DataTable_e_DataView_usam_a_traducao_pt_br()
    {
        var errors = new List<string>();
        var page = await NewPageAsync(errors);

        await Expect(page.Locator("#t-datatable .p-datatable-empty-message")).ToHaveTextAsync("Nenhum resultado encontrado");
        await Expect(page.Locator("#t-datatable .p-datatable-filter-column input")).ToHaveAttributeAsync("aria-label", "Filtrar Nome");
        await Expect(page.Locator("#t-dataview .p-dataview-empty-message")).ToHaveTextAsync("Nenhum resultado encontrado");

        Assert.True(errors.Count == 0, string.Join(" || ", errors));
        await page.CloseAsync();
    }

    [Fact]
    public async Task TreeTable_usa_a_traducao_pt_br()
    {
        var errors = new List<string>();
        var page = await NewPageAsync(errors);

        await Expect(page.Locator("#t-treetable .p-treetable-node-toggle-button")).ToHaveAttributeAsync("aria-label", "Linha expandida");
        await Expect(page.Locator("#t-treetable-empty .p-treetable-empty-message")).ToHaveTextAsync("Nenhum resultado encontrado");

        Assert.True(errors.Count == 0, string.Join(" || ", errors));
        await page.CloseAsync();
    }

    [Fact]
    public async Task Tree_filtra_sem_acento_e_usa_a_traducao_pt_br()
    {
        var errors = new List<string>();
        var page = await NewPageAsync(errors);

        var filter = page.Locator("#t-tree .p-tree-filter-input");
        await Expect(filter).ToHaveAttributeAsync("placeholder", "Filtrar");
        await Expect(filter).ToHaveAttributeAsync("aria-label", "Filtrar");

        // "sao" (sem acento e minúsculo) casa com "São Paulo" via comparador ordinal.
        await filter.FillAsync("sao");

        var nodes = page.Locator("#t-tree li[role=treeitem]");
        await Expect(nodes).ToHaveCountAsync(1);
        await Expect(nodes.First).ToHaveAttributeAsync("aria-label", "São Paulo");

        Assert.True(errors.Count == 0, string.Join(" || ", errors));
        await page.CloseAsync();
    }
}
