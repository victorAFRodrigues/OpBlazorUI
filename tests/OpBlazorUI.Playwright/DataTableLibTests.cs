using System.Text.RegularExpressions;
using Microsoft.Playwright;
using Xunit;
using static Microsoft.Playwright.Assertions;

namespace OpBlazorUI.Playwright;

/// <summary>
/// OpDataTable em nível de lib: expansão de linhas, grupos (subheader/rowspan/
/// expansíveis), cabeçalho agrupado, linhas/colunas congeladas e reordenação. Usa a
/// página <c>/_tests/data-table-lib</c> do Showcase.
/// </summary>
[Collection("showcase")]
public class DataTableLibTests
{
    private readonly ShowcaseFixture _fixture;

    public DataTableLibTests(ShowcaseFixture fixture) => _fixture = fixture;

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
        await page.GotoAsync($"{_fixture.BaseUrl}/_tests/data-table-lib", new PageGotoOptions { WaitUntil = WaitUntilState.NetworkIdle });
        await page.Locator("#t-title").WaitForAsync();
        return page;
    }

    [Fact]
    public async Task Expansao_de_linhas_abre_e_fecha()
    {
        var errors = new List<string>();
        var page = await NewPageAsync(errors);

        await Expect(page.Locator("#t1 .p-datatable-tbody tr")).ToHaveCountAsync(3);

        await page.Locator("#t1 .t1-toggle").First.ClickAsync();
        await Expect(page.Locator("#t1 .t1-detail")).ToHaveCountAsync(1);
        await Expect(page.Locator("#t1 .t1-detail")).ToContainTextAsync("Detalhe de Alface");

        // Mais de uma linha pode ficar aberta ao mesmo tempo.
        await page.Locator("#t1 .t1-toggle").Nth(2).ClickAsync();
        await Expect(page.Locator("#t1 .t1-detail")).ToHaveCountAsync(2);

        await page.Locator("#t1 .t1-toggle").First.ClickAsync();
        await Expect(page.Locator("#t1 .t1-detail")).ToHaveCountAsync(1);
        await Expect(page.Locator("#t1 .t1-detail")).ToContainTextAsync("Detalhe de Banana");

        Assert.True(errors.Count == 0, string.Join(" || ", errors));
        await page.CloseAsync();
    }

    [Fact]
    public async Task Grupo_subheader_renderiza_faixa_e_rodape()
    {
        var errors = new List<string>();
        var page = await NewPageAsync(errors);

        await Expect(page.Locator("#t2 .p-datatable-row-group-header")).ToHaveCountAsync(3);
        await Expect(page.Locator("#t2 .t2-group")).ToHaveTextAsync(new[] { "Verdura (2)", "Legume (1)", "Fruta (1)" });
        await Expect(page.Locator("#t2 .t2-group-footer")).ToHaveTextAsync(new[] { "Total: 2", "Total: 1", "Total: 1" });
        await Expect(page.Locator("#t2 .p-datatable-tbody tr:not(.p-datatable-row-group-header):not(.p-rowgroup-footer)"))
            .ToHaveCountAsync(4);

        Assert.True(errors.Count == 0, string.Join(" || ", errors));
        await page.CloseAsync();
    }

    [Fact]
    public async Task Grupos_expansiveis_usam_o_toggle_padrao()
    {
        var errors = new List<string>();
        var page = await NewPageAsync(errors);

        await Expect(page.Locator("#t3 .p-datatable-row-group-header")).ToHaveCountAsync(3);
        await Expect(page.Locator("#t3 .p-datatable-row-toggle-button")).ToHaveCountAsync(3);
        await Expect(page.Locator("#t3 .p-datatable-tbody tr")).ToHaveCountAsync(3);

        await page.Locator("#t3 .p-datatable-row-toggle-button").First.ClickAsync();
        await Expect(page.Locator("#t3 .p-datatable-tbody tr")).ToHaveCountAsync(5);
        await Expect(page.Locator("#t3 .p-datatable-row-toggle-icon").First)
            .ToHaveClassAsync(new Regex("pi-chevron-down"));

        await page.Locator("#t3 .p-datatable-row-toggle-button").First.ClickAsync();
        await Expect(page.Locator("#t3 .p-datatable-tbody tr")).ToHaveCountAsync(3);

        Assert.True(errors.Count == 0, string.Join(" || ", errors));
        await page.CloseAsync();
    }

    [Fact]
    public async Task Grupos_expansiveis_com_template_customizado()
    {
        var errors = new List<string>();
        var page = await NewPageAsync(errors);

        await Expect(page.Locator("#t4 .t4-group")).ToHaveTextAsync(new[] { "Verdura", "Legume", "Fruta" });
        await Expect(page.Locator("#t4 .p-datatable-tbody tr")).ToHaveCountAsync(3);

        await page.Locator("#t4 .t4-toggle").First.ClickAsync();
        await Expect(page.Locator("#t4 .p-datatable-tbody tr")).ToHaveCountAsync(5);

        await page.Locator("#t4 .t4-toggle").First.ClickAsync();
        await Expect(page.Locator("#t4 .p-datatable-tbody tr")).ToHaveCountAsync(3);

        Assert.True(errors.Count == 0, string.Join(" || ", errors));
        await page.CloseAsync();
    }

    [Fact]
    public async Task Grupo_rowspan_estende_a_celula_da_coluna()
    {
        var errors = new List<string>();
        var page = await NewPageAsync(errors);

        var rows = page.Locator("#t5 .p-datatable-tbody tr");
        await Expect(rows).ToHaveCountAsync(4);

        var first = rows.Nth(0);
        await Expect(first.Locator("td")).ToHaveCountAsync(3);
        Assert.Equal("2", await first.Locator("td").First.GetAttributeAsync("rowspan"));
        await Expect(first.Locator("td").First).ToHaveTextAsync("Verdura");

        // A segunda linha do grupo não repete a célula com rowspan.
        var second = rows.Nth(1);
        await Expect(second.Locator("td")).ToHaveCountAsync(2);

        Assert.True(errors.Count == 0, string.Join(" || ", errors));
        await page.CloseAsync();
    }

    [Fact]
    public async Task Cabecalho_agrupado_renderiza_linhas_extras()
    {
        var errors = new List<string>();
        var page = await NewPageAsync(errors);

        await Expect(page.Locator("#t6 thead tr")).ToHaveCountAsync(2);

        var produto = page.Locator("#t6 thead th").First;
        await Expect(produto).ToHaveTextAsync("Produto");
        Assert.Equal("2", await produto.GetAttributeAsync("rowspan"));

        // A coluna "Nome" fica oculta na linha normal: sobra só "Preço".
        var headerRow = page.Locator("#t6 thead tr").Nth(1);
        await Expect(headerRow.Locator("th")).ToHaveCountAsync(1);
        await Expect(headerRow.Locator("th").First).ToContainTextAsync("Preço");

        Assert.True(errors.Count == 0, string.Join(" || ", errors));
        await page.CloseAsync();
    }

    [Fact]
    public async Task Colunas_congeladas_usam_sticky_com_deslocamento()
    {
        var errors = new List<string>();
        var page = await NewPageAsync(errors);

        var firstTh = page.Locator("#t7 thead th").First;
        await Expect(firstTh).ToHaveClassAsync(new Regex("p-datatable-frozen-column"));
        Assert.Contains("left: 0px", await firstTh.GetAttributeAsync("style"));

        var lastTh = page.Locator("#t7 thead th").Last;
        await Expect(lastTh).ToHaveClassAsync(new Regex("p-datatable-frozen-column"));
        Assert.Contains("right: 0px", await lastTh.GetAttributeAsync("style"));

        var position = await firstTh.EvaluateAsync<string>("el => getComputedStyle(el).position");
        Assert.Equal("sticky", position);

        await Expect(page.Locator("#t7 .p-datatable-tbody td").First)
            .ToHaveClassAsync(new Regex("p-datatable-frozen-column"));

        Assert.True(errors.Count == 0, string.Join(" || ", errors));
        await page.CloseAsync();
    }

    [Fact]
    public async Task Linhas_congeladas_renderizam_em_tbody_proprio()
    {
        var errors = new List<string>();
        var page = await NewPageAsync(errors);

        await Expect(page.Locator("#t8 .p-datatable-frozen-tbody tr")).ToHaveCountAsync(2);
        await Expect(page.Locator("#t8 .p-datatable-tbody tr")).ToHaveCountAsync(25);
        await Expect(page.Locator("#t8 .p-datatable-frozen-tbody tr").First).ToContainTextAsync("Item 1");

        Assert.True(errors.Count == 0, string.Join(" || ", errors));
        await page.CloseAsync();
    }

    [Fact]
    public async Task Reordenacao_de_colunas_por_arraste()
    {
        var errors = new List<string>();
        var page = await NewPageAsync(errors);

        await Expect(page.Locator("#t9-log")).ToHaveTextAsync("Name,Price");
        await Expect(page.Locator("#t9 thead th").First)
            .ToHaveClassAsync(new Regex("p-datatable-reorderable-column"));
        Assert.Equal("true", await page.Locator("#t9 thead th").First.GetAttributeAsync("draggable"));

        // Arraste real (mouse) — eventos sintéticos não chegam ao Blazor.
        await page.Locator("#t9 thead th").First.DragToAsync(page.Locator("#t9 thead th").Nth(1));
        await Expect(page.Locator("#t9-log")).ToHaveTextAsync("Price,Name");

        Assert.True(errors.Count == 0, string.Join(" || ", errors));
        await page.CloseAsync();
    }

    [Fact]
    public async Task Reordenacao_de_linhas_por_arraste()
    {
        var errors = new List<string>();
        var page = await NewPageAsync(errors);

        await Expect(page.Locator("#t10-log")).ToHaveTextAsync("Alface,Tomate,Banana");
        await Expect(page.Locator("#t10 .p-datatable-reorderable-row-handle")).ToHaveCountAsync(3);
        Assert.Equal("true", await page.Locator("#t10 .p-datatable-tbody tr").First.GetAttributeAsync("draggable"));

        await page.Locator("#t10 .p-datatable-tbody tr").First
            .DragToAsync(page.Locator("#t10 .p-datatable-tbody tr").Nth(2));
        await Expect(page.Locator("#t10-log")).ToHaveTextAsync("Tomate,Banana,Alface");

        Assert.True(errors.Count == 0, string.Join(" || ", errors));
        await page.CloseAsync();
    }

    [Fact]
    public async Task Edicao_de_celula_salva_com_enter_e_cancela_com_escape()
    {
        var errors = new List<string>();
        var page = await NewPageAsync(errors);

        var priceCell = page.Locator("#t11 .p-datatable-tbody tr").First.Locator("td").Nth(1);

        await priceCell.ClickAsync();
        await Expect(priceCell).ToHaveClassAsync(new Regex("p-cell-editing"));
        await Expect(priceCell.Locator("input")).ToHaveCountAsync(1);
        await Expect(page.Locator("#t11-log")).ToHaveTextAsync("init:Alface");

        await priceCell.Locator("input").FillAsync("9");
        await priceCell.Locator("input").PressAsync("Enter");
        await Expect(page.Locator("#t11-log")).ToHaveTextAsync("complete:Alface");
        await Expect(priceCell).ToContainTextAsync("9");
        await Expect(priceCell.Locator("input")).ToHaveCountAsync(0);

        // Escape descarta o rascunho: o valor salvo (9) permanece.
        await priceCell.ClickAsync();
        await priceCell.Locator("input").FillAsync("3");
        await priceCell.Locator("input").PressAsync("Escape");
        await Expect(page.Locator("#t11-log")).ToHaveTextAsync("cancel:Alface");
        await Expect(priceCell).ToContainTextAsync("9");
        await Expect(priceCell).Not.ToContainTextAsync("3");

        Assert.True(errors.Count == 0, string.Join(" || ", errors));
        await page.CloseAsync();
    }

    [Fact]
    public async Task Edicao_de_linha_salva_e_cancela_restaurando()
    {
        var errors = new List<string>();
        var page = await NewPageAsync(errors);

        var firstRow = page.Locator("#t12 .p-datatable-tbody tr").First;
        await firstRow.Locator(".t12-init").ClickAsync();
        await Expect(page.Locator("#t12-log")).ToHaveTextAsync("init:Alface");
        await Expect(firstRow.Locator("input").First).ToHaveValueAsync("Alface");

        await firstRow.Locator("input").First.FillAsync("Alface Editada");
        await firstRow.Locator(".t12-save").ClickAsync();
        await Expect(page.Locator("#t12-log")).ToHaveTextAsync("save:Alface Editada");
        await Expect(firstRow.Locator("input")).ToHaveCountAsync(0);
        await Expect(firstRow).ToContainTextAsync("Alface Editada");

        var secondRow = page.Locator("#t12 .p-datatable-tbody tr").Nth(1);
        await secondRow.Locator(".t12-init").ClickAsync();
        await Expect(page.Locator("#t12-log")).ToHaveTextAsync("init:Tomate");
        await secondRow.Locator("input").First.FillAsync("Tomate Editado");
        await secondRow.Locator(".t12-cancel").ClickAsync();
        await Expect(page.Locator("#t12-log")).ToHaveTextAsync("cancel:Tomate");
        await Expect(secondRow).Not.ToContainTextAsync("Tomate Editado");
        await Expect(secondRow).ToContainTextAsync("Tomate");

        Assert.True(errors.Count == 0, string.Join(" || ", errors));
        await page.CloseAsync();
    }

    [Fact]
    public async Task Stateful_restaura_pagina_ordenacao_e_filtro()
    {
        var errors = new List<string>();
        var page = await NewPageAsync(errors);

        await page.Locator("#t13 .p-datatable-filter-row input").FillAsync("Par");
        await Expect(page.Locator("#t13-log")).ToHaveTextAsync("filter:Category=Par");

        await page.Locator("#t13 thead th").First.ClickAsync();
        await Expect(page.Locator("#t13 thead th").First).ToHaveClassAsync(new Regex("p-datatable-column-sorted"));

        await page.Locator("#t13 .p-paginator-next").ClickAsync();
        await Expect(page.Locator("#t13-log")).ToHaveTextAsync("page:1");
        await Expect(page.Locator("#t13 .p-paginator-page-selected")).ToHaveTextAsync("2");

        await page.ReloadAsync(new PageReloadOptions { WaitUntil = WaitUntilState.NetworkIdle });
        await page.Locator("#t-title").WaitForAsync();

        await Expect(page.Locator("#t13 .p-paginator-page-selected")).ToHaveTextAsync("2");
        await Expect(page.Locator("#t13 thead th").First).ToHaveClassAsync(new Regex("p-datatable-column-sorted"));
        await Expect(page.Locator("#t13 .p-datatable-filter-row input")).ToHaveValueAsync("Par");

        Assert.True(errors.Count == 0, string.Join(" || ", errors));
        await page.CloseAsync();
    }
}
