using System.Text.RegularExpressions;
using Microsoft.Playwright;
using Xunit;
using static Microsoft.Playwright.Assertions;

namespace OpBlazorUI.Playwright;

/// <summary>
/// OpTreeTable em nível de lib: ordenação (single/múltipla/removível), filtro por coluna
/// e global (lenient/strict), gridlines/tamanho, skeleton, lazy, menu de contexto,
/// reordenação, cabeçalho agrupado, congelamento, paginador e estilo condicional. Usa a
/// página <c>/_tests/tree-table-lib</c> do Showcase.
/// </summary>
[Collection("showcase")]
public class TreeTableLibTests
{
    private readonly ShowcaseFixture _fixture;

    public TreeTableLibTests(ShowcaseFixture fixture) => _fixture = fixture;

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
        await page.GotoAsync($"{_fixture.BaseUrl}/_tests/tree-table-lib", new PageGotoOptions { WaitUntil = WaitUntilState.NetworkIdle });
        await page.Locator("#t-title").WaitForAsync();
        return page;
    }

    private static void AssertNoErrors(List<string> errors) =>
        Assert.True(errors.Count == 0, string.Join(" || ", errors));

    [Fact]
    public async Task Ordenacao_simples_ordena_raizes_e_filhos()
    {
        var errors = new List<string>();
        var page = await NewPageAsync(errors);
        var rows = page.Locator("#t1 .p-treetable-tbody tr");
        var nameTh = page.Locator("#t1 thead th").First;

        // Ordem original: Pictures, Documents, Videos.
        await Expect(rows.Nth(0)).ToContainTextAsync("Pictures");

        await nameTh.ClickAsync();
        await Expect(rows.Nth(0)).ToContainTextAsync("Documents");
        await Expect(rows.Nth(1)).ToContainTextAsync("Pictures");
        await Expect(rows.Nth(2)).ToContainTextAsync("Videos");
        await Expect(nameTh).ToHaveAttributeAsync("aria-sort", "ascending");
        await Expect(page.Locator("#t1-log")).ToHaveTextAsync("sort:Name:1:Name=1");

        // Filhos também ordenados (Home antes de Work).
        await rows.Nth(0).Locator(".p-treetable-node-toggle-button").ClickAsync();
        await Expect(rows.Nth(1)).ToContainTextAsync("Home");
        await Expect(rows.Nth(2)).ToContainTextAsync("Work");

        await nameTh.ClickAsync();
        await Expect(nameTh).ToHaveAttributeAsync("aria-sort", "descending");
        await Expect(rows.Nth(0)).ToContainTextAsync("Videos");
        await Expect(page.Locator("#t1-log")).ToHaveTextAsync("sort:Name:-1:Name=-1");

        // Navegação por teclado no cabeçalho.
        await nameTh.PressAsync("Enter");
        await Expect(page.Locator("#t1-log")).ToHaveTextAsync("sort:Name:1:Name=1");

        AssertNoErrors(errors);
        await page.CloseAsync();
    }

    [Fact]
    public async Task Ordenacao_multipla_com_meta_key()
    {
        var errors = new List<string>();
        var page = await NewPageAsync(errors);
        var rows = page.Locator("#t2 .p-treetable-tbody tr");
        var nameTh = page.Locator("#t2 thead th").First;
        var sizeTh = page.Locator("#t2 thead th").Nth(1);

        await sizeTh.ClickAsync();
        await Expect(rows.Nth(0)).ToContainTextAsync("Documents");
        await Expect(rows.Nth(1)).ToContainTextAsync("Videos");
        await Expect(rows.Nth(2)).ToContainTextAsync("Pictures");

        await nameTh.ClickAsync(new LocatorClickOptions { Modifiers = [KeyboardModifier.Meta] });
        await Expect(page.Locator("#t2-log")).ToHaveTextAsync("sort:Name:1:Size=1+Name=1");
        await Expect(page.Locator("#t2 .p-treetable-sort-badge")).ToHaveCountAsync(2);
        await Expect(rows.Nth(0)).ToContainTextAsync("Documents");

        // ⌘ no campo já ordenado inverte a ordem daquele campo.
        await nameTh.ClickAsync(new LocatorClickOptions { Modifiers = [KeyboardModifier.Meta] });
        await Expect(page.Locator("#t2-log")).ToHaveTextAsync("sort:Name:-1:Size=1+Name=-1");

        AssertNoErrors(errors);
        await page.CloseAsync();
    }

    [Fact]
    public async Task Ordenacao_removivel_volta_a_ordem_original_no_terceiro_clique()
    {
        var errors = new List<string>();
        var page = await NewPageAsync(errors);
        var rows = page.Locator("#t3 .p-treetable-tbody tr");
        var nameTh = page.Locator("#t3 thead th").First;

        await nameTh.ClickAsync();
        await Expect(page.Locator("#t3-log")).ToHaveTextAsync("sort:Name:1:Name=1");

        await nameTh.ClickAsync();
        await Expect(page.Locator("#t3-log")).ToHaveTextAsync("sort:Name:-1:Name=-1");

        await nameTh.ClickAsync();
        await Expect(page.Locator("#t3-log")).ToHaveTextAsync("sort:Name:0:");
        await Expect(rows.Nth(0)).ToContainTextAsync("Pictures");
        await Expect(rows.Nth(1)).ToContainTextAsync("Documents");
        await Expect(rows.Nth(2)).ToContainTextAsync("Videos");
        await Expect(nameTh).ToHaveAttributeAsync("aria-sort", "none");

        AssertNoErrors(errors);
        await page.CloseAsync();
    }

    [Fact]
    public async Task Filtro_por_coluna_lenient_e_strict()
    {
        var errors = new List<string>();
        var page = await NewPageAsync(errors);

        var lenientRows = page.Locator("#t4a .p-treetable-tbody tr");
        await page.Locator("#t4a .p-treetable-filter-row input").FillAsync("Product");

        // Lenient: o nó que casa mantém a subárvore inteira (Sales, Product A, Unit A1, Unit A2, Product B).
        await Expect(lenientRows).ToHaveCountAsync(5);
        await Expect(lenientRows.Nth(0)).ToContainTextAsync("Sales");
        await Expect(lenientRows.Nth(1)).ToContainTextAsync("Product A");
        await Expect(lenientRows.Nth(2)).ToContainTextAsync("Unit A1");
        await Expect(page.Locator("#t4a-log")).ToContainTextAsync("filter:Name=Product");

        var strictRows = page.Locator("#t4b .p-treetable-tbody tr");
        await page.Locator("#t4b .p-treetable-filter-row input").FillAsync("Product");

        // Strict: os descendentes continuam filtrados (só Sales, Product A e Product B).
        await Expect(strictRows).ToHaveCountAsync(3);
        await Expect(strictRows.Nth(2)).ToContainTextAsync("Product B");

        // Limpar o filtro volta à árvore recolhida.
        await page.Locator("#t4a .p-treetable-filter-row input").FillAsync("");
        await Expect(lenientRows).ToHaveCountAsync(2);

        AssertNoErrors(errors);
        await page.CloseAsync();
    }

    [Fact]
    public async Task Filtro_global_imperativo()
    {
        var errors = new List<string>();
        var page = await NewPageAsync(errors);
        var rows = page.Locator("#t5 .p-treetable-tbody tr");

        await Expect(rows).ToHaveCountAsync(2);
        await Expect(rows.Nth(0)).ToContainTextAsync("Sales");

        await page.Locator("#t5 input").FillAsync("Sup");
        await page.Locator("#t5-apply").ClickAsync();

        // O ancestral do resultado aparece (Services) e expande sozinho até o match (Support).
        await Expect(rows).ToHaveCountAsync(2);
        await Expect(rows.Nth(0)).ToContainTextAsync("Services");
        await Expect(rows.Nth(1)).ToContainTextAsync("Support");

        await page.Locator("#t5 input").FillAsync("");
        await page.Locator("#t5-apply").ClickAsync();
        await Expect(rows.Nth(0)).ToContainTextAsync("Sales");

        AssertNoErrors(errors);
        await page.CloseAsync();
    }

    [Fact]
    public async Task Gridlines_e_tamanho_aplicam_classes()
    {
        var errors = new List<string>();
        var page = await NewPageAsync(errors);

        var table = page.Locator("#t6 .p-treetable");
        await Expect(table).ToHaveClassAsync(new Regex("p-treetable-gridlines"));
        await Expect(table).ToHaveClassAsync(new Regex("p-treetable-sm"));

        AssertNoErrors(errors);
        await page.CloseAsync();
    }

    [Fact]
    public async Task Skeleton_de_carga()
    {
        var errors = new List<string>();
        var page = await NewPageAsync(errors);

        await Expect(page.Locator("#t7 .p-treetable-skeleton-row")).ToHaveCountAsync(3);
        await Expect(page.Locator("#t7 .p-skeleton")).ToHaveCountAsync(6);
        await Expect(page.Locator("#t7 .p-treetable-mask")).ToHaveCountAsync(0);
        await Expect(page.Locator("#t7 .p-treetable-tbody tr")).ToHaveCountAsync(3);

        AssertNoErrors(errors);
        await page.CloseAsync();
    }

    [Fact]
    public async Task Lazy_dispara_evento_e_carrega_filhos_no_expandir()
    {
        var errors = new List<string>();
        var page = await NewPageAsync(errors);
        var rows = page.Locator("#t8 .p-treetable-tbody tr");

        await Expect(page.Locator("#t8-log")).ToHaveTextAsync("lazy:0:10:-:0");
        await Expect(rows).ToHaveCountAsync(10);
        await Expect(rows.Nth(0)).ToContainTextAsync("Item 1");

        // Expandir um nó lazy mostra o spinner durante a "carga" e depois os filhos.
        await rows.Nth(0).Locator(".p-treetable-node-toggle-button").ClickAsync();
        await Expect(page.Locator("#t8 .p-treetable-node-toggle-icon.pi-spin")).ToBeVisibleAsync();
        await Expect(page.Locator("#t8 .p-treetable-node-toggle-icon.pi-spin")).ToHaveCountAsync(0);
        await Expect(rows).ToHaveCountAsync(12);
        await Expect(rows.Nth(1)).ToContainTextAsync("Item 1 - 0");
        await Expect(rows.Nth(2)).ToContainTextAsync("Item 1 - 1");

        // Paginação em lazy rebusca a página.
        await page.Locator("#t8 .p-paginator-next").ClickAsync();
        await Expect(page.Locator("#t8-log")).ToHaveTextAsync("lazy:10:10:-:0");
        await Expect(rows.Nth(0)).ToContainTextAsync("Item 11");

        AssertNoErrors(errors);
        await page.CloseAsync();
    }

    [Fact]
    public async Task Menu_de_contexto_na_linha()
    {
        var errors = new List<string>();
        var page = await NewPageAsync(errors);

        await page.Locator("#t9 .p-treetable-tbody tr").First.ClickAsync(new LocatorClickOptions { Button = MouseButton.Right });
        await Expect(page.Locator("#t9-log")).ToHaveTextAsync("context:Pictures");

        await page.Locator("#t9 .p-treetable-tbody tr").Nth(1).ClickAsync(new LocatorClickOptions { Button = MouseButton.Right });
        await Expect(page.Locator("#t9-log")).ToHaveTextAsync("context:Documents");

        AssertNoErrors(errors);
        await page.CloseAsync();
    }

    [Fact]
    public async Task Reordenacao_de_colunas_por_arraste()
    {
        var errors = new List<string>();
        var page = await NewPageAsync(errors);

        var firstTh = page.Locator("#t10 thead th").First;
        Assert.Equal("true", await firstTh.GetAttributeAsync("draggable"));

        await firstTh.DragToAsync(page.Locator("#t10 thead th").Nth(1));

        await Expect(page.Locator("#t10-log")).ToHaveTextAsync("Size,Name");
        await Expect(page.Locator("#t10 thead th").First).ToContainTextAsync("Tamanho");

        AssertNoErrors(errors);
        await page.CloseAsync();
    }

    [Fact]
    public async Task Cabecalho_agrupado_com_rowspan_e_colspan()
    {
        var errors = new List<string>();
        var page = await NewPageAsync(errors);

        var headRows = page.Locator("#t11 thead tr");
        await Expect(headRows).ToHaveCountAsync(2);
        await Expect(headRows.Nth(0).Locator("th")).ToHaveCountAsync(2);
        await Expect(headRows.Nth(0).Locator("th").First).ToHaveAttributeAsync("rowspan", "2");
        await Expect(headRows.Nth(0).Locator("th").Nth(1)).ToHaveAttributeAsync("colspan", "2");
        await Expect(page.Locator("#t11 tbody tr").First.Locator("td")).ToHaveCountAsync(3);

        AssertNoErrors(errors);
        await page.CloseAsync();
    }

    [Fact]
    public async Task Colunas_congeladas_com_altura_de_rolagem()
    {
        var errors = new List<string>();
        var page = await NewPageAsync(errors);

        await Expect(page.Locator("#t12 .p-treetable")).ToHaveClassAsync(new Regex("p-treetable-scrollable"));
        await Expect(page.Locator("#t12 .p-treetable-table-container")).ToHaveAttributeAsync("style", new Regex("max-height: 8rem"));

        var firstTd = page.Locator("#t12 .p-treetable-tbody tr").First.Locator("td").First;
        await Expect(firstTd).ToHaveClassAsync(new Regex("p-treetable-frozen-column"));
        await Expect(firstTd).ToHaveAttributeAsync("style", new Regex("left: 0px"));

        var lastTd = page.Locator("#t12 .p-treetable-tbody tr").First.Locator("td").Last;
        await Expect(lastTd).ToHaveAttributeAsync("style", new Regex("right: 0px"));

        AssertNoErrors(errors);
        await page.CloseAsync();
    }

    [Fact]
    public async Task Paginador_com_templates_rodape_e_estilo_condicional()
    {
        var errors = new List<string>();
        var page = await NewPageAsync(errors);
        var rows = page.Locator("#t13 .p-treetable-tbody tr");

        await Expect(page.Locator("#t13 .p-paginator-content-start .t13-refresh")).ToBeVisibleAsync();
        await Expect(page.Locator("#t13 .p-paginator-content-end .t13-download")).ToBeVisibleAsync();

        await page.Locator("#t13 .t13-refresh").ClickAsync();
        await Expect(page.Locator("#t13-log")).ToHaveTextAsync("refresh");
        await page.Locator("#t13 .t13-download").ClickAsync();
        await Expect(page.Locator("#t13-log")).ToHaveTextAsync("download");

        await Expect(page.Locator("#t13 .t13-big")).ToHaveCountAsync(1);
        await Expect(page.Locator("#t13 tfoot td")).ToHaveTextAsync(new[] { "Total: 5", "Soma: 150" });

        await Expect(rows.Nth(0)).ToContainTextAsync("Item 1");
        await page.Locator("#t13 .p-paginator-next").ClickAsync();
        await Expect(page.Locator("#t13-log")).ToHaveTextAsync("page:1:2");
        await Expect(rows.Nth(0)).ToContainTextAsync("Item 3");

        AssertNoErrors(errors);
        await page.CloseAsync();
    }

    [Fact]
    public async Task Expansao_controlada_por_mutacao_do_no()
    {
        var errors = new List<string>();
        var page = await NewPageAsync(errors);
        var rows = page.Locator("#t14 .p-treetable-tbody tr");

        await Expect(rows).ToHaveCountAsync(3);

        await page.Locator("#t14-toggle").ClickAsync();
        await Expect(rows).ToHaveCountAsync(4);
        await Expect(rows.Nth(1)).ToContainTextAsync("Vacation");

        await page.Locator("#t14-toggle").ClickAsync();
        await Expect(rows).ToHaveCountAsync(3);

        AssertNoErrors(errors);
        await page.CloseAsync();
    }
}
