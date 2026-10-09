using Microsoft.Playwright;
using Xunit;
using static Microsoft.Playwright.Assertions;

namespace OpBlazorUI.Playwright;

/// <summary>
/// Bloco 3.5 (árvores): derivação de folha, filtro com hierarquia, propagação de checkbox,
/// paginação por raiz e "selecionar tudo". Usa a página <c>/_tests/bloco3-5</c>.
/// </summary>
[Collection("showcase")]
public class Bloco3_5Tests
{
    private readonly ShowcaseFixture _fixture;

    public Bloco3_5Tests(ShowcaseFixture fixture) => _fixture = fixture;

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
        await page.GotoAsync($"{_fixture.BaseUrl}/_tests/bloco3-5", new PageGotoOptions { WaitUntil = WaitUntilState.NetworkIdle });
        await page.Locator("#t-title").WaitForAsync();
        return page;
    }

    private static ILocator Node(IPage page, string scope, string text) =>
        page.Locator($"{scope} li.p-tree-node").Filter(new LocatorFilterOptions { HasText = text });

    [Fact]
    public async Task Leaf_deriva_dos_filhos()
    {
        var errors = new List<string>();
        var page = await NewPageAsync(errors);

        // Nó sem filhos e sem Leaf explícito é folha (o toggle fica oculto, sem lazy).
        var leaf = Node(page, "#t-leaf", "SemFilho");
        await Expect(leaf).ToHaveClassAsync(new System.Text.RegularExpressions.Regex("p-tree-node-leaf"));
        await Expect(leaf.Locator(".p-tree-node-toggle-button")).ToBeHiddenAsync();
        await Expect(leaf.Locator(".pi-spinner")).ToHaveCountAsync(0);

        // Nó com Leaf=false (lazy) é expansível e mostra o spinner ao expandir.
        var lazy = Node(page, "#t-leaf", "Lazy");
        await Expect(lazy).Not.ToHaveClassAsync(new System.Text.RegularExpressions.Regex("p-tree-node-leaf"));
        await lazy.Locator(".p-tree-node-toggle-button").ClickAsync();
        await Expect(lazy.Locator(".pi-spinner")).ToHaveCountAsync(1);
        await Expect(page.Locator("#t-leaf-log")).ToHaveTextAsync("expandiu Lazy");

        Assert.True(errors.Count == 0, string.Join(" || ", errors));
        await page.CloseAsync();
    }

    [Fact]
    public async Task Filtro_ignora_caixa_e_recalcula_o_modo()
    {
        var errors = new List<string>();
        var page = await NewPageAsync(errors);

        // "doc" (minúsculo) casa com "Documentos" — FilterBy case-insensitive.
        await page.Locator("#t-tree-filter .p-tree-filter-input").FillAsync("doc");
        var labels = page.Locator("#t-tree-filter .p-tree-node-label");
        await Expect(labels).ToHaveCountAsync(3); // Documentos + Contratos + Imagem (lenient)
        await Expect(page.Locator("#t-tree-filter")).ToContainTextAsync("Contratos");

        // Trocar para strict em runtime recolhe os filhos que não casam.
        await page.Locator("#t-filter-mode").ClickAsync();
        await Expect(labels).ToHaveCountAsync(1);
        await Expect(labels).ToHaveTextAsync("Documentos");

        Assert.True(errors.Count == 0, string.Join(" || ", errors));
        await page.CloseAsync();
    }

    [Fact]
    public async Task Filtro_por_campo_customizado()
    {
        var errors = new List<string>();
        var page = await NewPageAsync(errors);

        // Com FilterBy=label, "zebra" não casa com nada.
        await page.Locator("#t-tree-filter .p-tree-filter-input").FillAsync("zebra");
        await Expect(page.Locator("#t-tree-filter .p-tree-node-label")).ToHaveCountAsync(0);

        // Passando FilterBy para "data", o nó Alpha (Data="Zebra") aparece.
        await page.Locator("#t-filter-by").ClickAsync();
        await Expect(page.Locator("#t-tree-filter .p-tree-node-label")).ToHaveTextAsync("Alpha");

        Assert.True(errors.Count == 0, string.Join(" || ", errors));
        await page.CloseAsync();
    }

    [Fact]
    public async Task Checkbox_com_filtro_propaga_para_o_ancestral()
    {
        var errors = new List<string>();
        var page = await NewPageAsync(errors);

        await page.Locator("#t-tree-checkbox .p-tree-filter-input").FillAsync("contra");
        var child = page.Locator("#t-tree-checkbox .p-tree-node-content").Filter(new LocatorFilterOptions { HasText = "Contratos" });
        await child.ClickAsync();

        // Apenas "Contratos" está visível no filtro, então o ancestral é marcado.
        await Expect(page.Locator("#t-check-keys")).ToHaveTextAsync("C1,P");

        Assert.True(errors.Count == 0, string.Join(" || ", errors));
        await page.CloseAsync();
    }

    [Fact]
    public async Task TreeSelect_filtro_preserva_hierarquia()
    {
        var errors = new List<string>();
        var page = await NewPageAsync(errors);

        await page.Locator("#t-treeselect-filter .p-treeselect-dropdown").ClickAsync();
        await page.Locator("#t-treeselect-filter .p-treeselect-filter").FillAsync("doc");

        var labels = page.Locator("#t-treeselect-filter .p-treeselect-overlay .p-tree-node-label");
        await Expect(labels).ToHaveCountAsync(3); // Documentos + Contrato + Imagem
        await Expect(page.Locator("#t-treeselect-filter .p-treeselect-overlay")).ToContainTextAsync("Documentos");
        await Expect(page.Locator("#t-treeselect-filter .p-treeselect-overlay")).Not.ToContainTextAsync("Foto");

        Assert.True(errors.Count == 0, string.Join(" || ", errors));
        await page.CloseAsync();
    }

    [Fact]
    public async Task TreeSelect_remover_chip_propaga()
    {
        var errors = new List<string>();
        var page = await NewPageAsync(errors);

        await page.Locator("#t-treeselect-chip .p-treeselect-dropdown").ClickAsync();
        await page.Locator("#t-treeselect-chip .p-tree-node-content").Filter(new LocatorFilterOptions { HasText = "Pai" }).ClickAsync();
        await Expect(page.Locator("#t-tschip-count")).ToHaveTextAsync("3");

        // Remover o chip do pai desmarca também os descendentes.
        await page.Locator("#t-treeselect-chip .p-chip").Filter(new LocatorFilterOptions { HasText = "Pai" })
            .Locator(".p-chip-remove-icon").ClickAsync();
        await Expect(page.Locator("#t-tschip-count")).ToHaveTextAsync("0");

        Assert.True(errors.Count == 0, string.Join(" || ", errors));
        await page.CloseAsync();
    }

    [Fact]
    public async Task TreeTable_pagina_pelos_nos_raiz()
    {
        var errors = new List<string>();
        var page = await NewPageAsync(errors);

        var body = page.Locator("#t-tt-page .p-treetable-tbody");
        await Expect(body).ToContainTextAsync("Raiz 1");
        await Expect(body).ToContainTextAsync("Filho 1.1");
        await Expect(body).Not.ToContainTextAsync("Raiz 2");

        await page.Locator("#t-tt-page .p-paginator-next").ClickAsync();
        await Expect(body).ToContainTextAsync("Raiz 2");
        await Expect(body).ToContainTextAsync("Filho 2.1");

        Assert.True(errors.Count == 0, string.Join(" || ", errors));
        await page.CloseAsync();
    }

    [Fact]
    public async Task TreeTable_select_all_ignora_nao_selecionaveis()
    {
        var errors = new List<string>();
        var page = await NewPageAsync(errors);

        // Seleção inicial: Pai + Filho A. Filho B não é selecionável e não deve impedir o "todos".
        var header = page.Locator("#t-tt-select thead input[type=checkbox]");
        await Expect(header).ToBeCheckedAsync();

        Assert.True(errors.Count == 0, string.Join(" || ", errors));
        await page.CloseAsync();
    }
}
