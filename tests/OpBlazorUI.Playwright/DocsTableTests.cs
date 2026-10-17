using System.Text;
using System.Text.RegularExpressions;
using Microsoft.Playwright;
using Xunit;
using static Microsoft.Playwright.Assertions;

namespace OpBlazorUI.Playwright;

/// <summary>
/// Página de documentação do <c>OpDataTable</c>: seções, ordem do upstream e demos
/// interativos.
/// </summary>
[Collection("showcase")]
public class DocsTableTests
{
    private readonly ShowcaseFixture _fixture;

    public DocsTableTests(ShowcaseFixture fixture) => _fixture = fixture;

    /// <summary>Ids das seções de recursos, na ordem-alvo (upstream + extras mantidos).</summary>
    private static readonly string[] SectionIds =
    [
        "import-demo", "basic", "dynamic", "template", "size", "gridlines", "striped", "table-style",
        "loading-mask", "loading-skeleton", "paginator-basic", "paginator-programmatic", "single-column-sort",
        "multiple-columns-sort", "pre-sort", "removable-sort", "filter-basic", "filter-advanced",
        "single-selection", "multiple-selection", "radio-button-selection", "checkbox-selection",
        "column-selection", "selection-events", "row-expansion", "cell-edit", "row-edit", "lazy-load",
        "vertical-scroll", "flex-scroll", "horizontal-scroll", "frozen-rows", "frozen-columns",
        "virtual-scroll-basic", "virtual-scroll-lazy", "column-group", "subheader", "expand", "row-span",
        "fit-mode", "expand-mode", "scrollable", "reorder", "column-toggle", "export", "context-menu",
        "stateful", "accessibility", "binding", "forms"
    ];

    private async Task<(IPage page, List<string> errors)> OpenAsync()
    {
        var errors = new List<string>();
        var page = await _fixture.Browser.NewPageAsync();
        page.SetDefaultTimeout(60000);
        page.Console += (_, msg) =>
        {
            if (msg.Type == "error")
            {
                errors.Add(msg.Text);
            }
        };
        page.PageError += (_, err) => errors.Add(err);

        await page.GotoAsync($"{_fixture.BaseUrl}/table",
            new PageGotoOptions { WaitUntil = WaitUntilState.NetworkIdle });
        return (page, errors);
    }

    private static ILocator Section(IPage page, string id) =>
        page.Locator($"section.doc-section:has(h2#{id})");

    private static void AssertNoErrors(List<string> errors) =>
        Assert.True(errors.Count == 0, string.Join(" || ", errors));

    [Fact]
    public async Task Pagina_da_tabela_tem_todas_as_secoes()
    {
        var (page, errors) = await OpenAsync();

        foreach (var id in SectionIds)
        {
            await Expect(Section(page, id)).ToBeVisibleAsync();
        }

        AssertNoErrors(errors);
        await page.CloseAsync();
    }

    [Fact]
    public async Task Abas_de_api_temas_passthrough_e_problemas()
    {
        var (page, errors) = await OpenAsync();

        await page.Locator(".doc-tabmenu button:has-text('API')").ClickAsync();
        foreach (var id in new[] { "api-props", "api-templates", "api-events", "api-column" })
        {
            await Expect(Section(page, id)).ToBeVisibleAsync();
        }

        await page.Locator(".doc-tabmenu button:has-text('TEMAS')").ClickAsync();
        await Expect(Section(page, "style")).ToBeVisibleAsync();
        await Expect(Section(page, "theming")).ToBeVisibleAsync();

        await page.Locator(".doc-tabmenu button:has-text('PASS THROUGH')").ClickAsync();
        await Expect(Section(page, "pt")).ToBeVisibleAsync();

        await page.Locator(".doc-tabmenu button:has-text('PROBLEMAS CONHECIDOS')").ClickAsync();
        await Expect(Section(page, "issues")).ToBeVisibleAsync();

        AssertNoErrors(errors);
        await page.CloseAsync();
    }

    [Fact]
    public async Task Paginacao_programatica_e_pre_ordenacao()
    {
        var (page, errors) = await OpenAsync();

        var pager = Section(page, "paginator-programmatic");
        await pager.GetByRole(AriaRole.Button, new() { Name = "2", Exact = true }).First.ClickAsync();
        await Expect(pager.Locator(".text-sm")).ToContainTextAsync("first: 4");
        await Expect(pager.Locator(".text-sm")).ToContainTextAsync("página atual: 2");

        var pre = Section(page, "pre-sort");
        await Expect(pre.Locator(".p-datatable-tbody tr").First).ToContainTextAsync("Uva");
        await Expect(pre.Locator(".p-datatable-column-sorted")).ToHaveCountAsync(1);

        AssertNoErrors(errors);
        await page.CloseAsync();
    }

    [Fact]
    public async Task Ordenacao_multipla_com_badge_e_removivel()
    {
        var (page, errors) = await OpenAsync();

        var multi = Section(page, "multiple-columns-sort");
        var priceTh = multi.Locator("thead th").Nth(3);
        var quantityTh = multi.Locator("thead th").Nth(4);

        await priceTh.ClickAsync(new LocatorClickOptions { Modifiers = [KeyboardModifier.Meta] });
        await quantityTh.ClickAsync(new LocatorClickOptions { Modifiers = [KeyboardModifier.Meta] });
        await Expect(multi.Locator(".p-datatable-sort-badge")).ToHaveCountAsync(2);
        await Expect(priceTh.Locator(".p-datatable-sort-badge")).ToHaveTextAsync("1");
        await Expect(quantityTh.Locator(".p-datatable-sort-badge")).ToHaveTextAsync("2");

        // Sem a tecla, a ordenação volta a ser só daquela coluna (badge de prioridade 1).
        await priceTh.ClickAsync();
        await Expect(multi.Locator(".p-datatable-sort-badge")).ToHaveCountAsync(1);
        await Expect(multi.Locator(".p-datatable-column-sorted")).ToHaveCountAsync(1);

        var removable = Section(page, "removable-sort");
        var sortTh = removable.Locator("thead th").Nth(3);
        await sortTh.ClickAsync();
        await Expect(removable.Locator(".p-datatable-column-sorted")).ToHaveCountAsync(1);
        await Expect(removable.Locator(".text-sm")).ToContainTextAsync("asc");

        await sortTh.ClickAsync();
        await Expect(removable.Locator(".text-sm")).ToContainTextAsync("desc");

        await sortTh.ClickAsync();
        await Expect(removable.Locator(".p-datatable-column-sorted")).ToHaveCountAsync(0);
        await Expect(removable.Locator(".text-sm")).ToContainTextAsync("nenhuma");

        AssertNoErrors(errors);
        await page.CloseAsync();
    }

    [Fact]
    public async Task Filtro_global_reduz_linhas()
    {
        var (page, errors) = await OpenAsync();

        var section = Section(page, "filter-basic");
        await Expect(section.Locator(".p-datatable-tbody tr")).ToHaveCountAsync(10);
        await Expect(section.Locator(".p-datatable-filter-row")).ToHaveCountAsync(1);

        await section.Locator("input").First.FillAsync("Tomate");
        await Expect(section.Locator(".p-datatable-tbody tr")).ToHaveCountAsync(1);
        await Expect(section.Locator(".p-datatable-tbody tr").First).ToContainTextAsync("Tomate");

        AssertNoErrors(errors);
        await page.CloseAsync();
    }

    [Fact]
    public async Task Selecao_unica_por_clique_e_multipla_com_meta()
    {
        var (page, errors) = await OpenAsync();

        var single = Section(page, "single-selection");
        var singleRows = single.Locator(".p-datatable-tbody tr");
        await singleRows.Nth(0).ClickAsync();
        await Expect(single.Locator(".text-sm")).ToContainTextAsync("Selecionado: Alface");
        await singleRows.Nth(1).ClickAsync();
        await Expect(single.Locator(".text-sm")).ToContainTextAsync("Selecionado: Tomate");
        await singleRows.Nth(1).ClickAsync();
        await Expect(single.Locator(".text-sm")).ToContainTextAsync("Selecionado: nenhum");

        var multi = Section(page, "multiple-selection");
        var multiRows = multi.Locator(".p-datatable-tbody tr");
        await multiRows.Nth(0).ClickAsync();
        await multiRows.Nth(1).ClickAsync();
        await Expect(multi.Locator(".text-sm").Last).ToContainTextAsync("Selecionados: 2");
        await multiRows.Nth(0).ClickAsync();
        await Expect(multi.Locator(".text-sm").Last).ToContainTextAsync("Selecionados: 1");

        // Com MetaKeySelection, clicar sem a tecla deixa a seleção só naquela linha.
        await multi.Locator("button.p-togglebutton").ClickAsync();
        await multiRows.Nth(2).ClickAsync();
        await Expect(multi.Locator(".text-sm").Last).ToContainTextAsync("Selecionados: 1 (Banana)");

        AssertNoErrors(errors);
        await page.CloseAsync();
    }

    [Fact]
    public async Task Colunas_de_checkbox_radio_e_selecao_customizada()
    {
        var (page, errors) = await OpenAsync();

        var check = Section(page, "checkbox-selection");
        await check.Locator("thead input[type=checkbox]").ClickAsync();
        await Expect(check.Locator(".text-sm")).ToContainTextAsync("Selecionados: 10");
        await check.Locator("thead input[type=checkbox]").ClickAsync();
        await Expect(check.Locator(".text-sm")).ToContainTextAsync("Selecionados: 0");

        var radio = Section(page, "radio-button-selection");
        await radio.Locator(".p-datatable-tbody tr").Nth(1).Locator("input[type=radio]").ClickAsync();
        await Expect(radio.Locator(".text-sm")).ToContainTextAsync("Selecionado: Tomate");

        var custom = Section(page, "column-selection");
        var button = custom.Locator(".p-datatable-tbody tr").First.Locator("button");
        await button.ClickAsync();
        await Expect(custom.Locator(".text-sm")).ToContainTextAsync("Selecionados: 1");
        await button.ClickAsync();
        await Expect(custom.Locator(".text-sm")).ToContainTextAsync("Selecionados: 0");

        AssertNoErrors(errors);
        await page.CloseAsync();
    }

    [Fact]
    public async Task Expansao_de_linha_individual_e_global()
    {
        var (page, errors) = await OpenAsync();

        var section = Section(page, "row-expansion");
        await section.Locator("button[aria-label='Alternar expansão']").First.ClickAsync();
        await Expect(section.Locator("h5")).ToHaveTextAsync("Pedidos de Alface");

        await section.GetByRole(AriaRole.Button, new() { Name = "Expandir tudo" }).ClickAsync();
        await Expect(section.Locator("h5")).ToHaveCountAsync(10);

        await section.GetByRole(AriaRole.Button, new() { Name = "Recolher tudo" }).ClickAsync();
        await Expect(section.Locator("h5")).ToHaveCountAsync(0);

        AssertNoErrors(errors);
        await page.CloseAsync();
    }

    [Fact]
    public async Task Edicao_de_celula_salva_com_enter_e_cancela_com_escape()
    {
        var (page, errors) = await OpenAsync();

        var section = Section(page, "cell-edit");
        var nameCell = section.Locator(".p-datatable-tbody tr").First.Locator("td").Nth(1);

        await nameCell.ClickAsync();
        await Expect(nameCell.Locator("input")).ToHaveCountAsync(1);
        await Expect(section.Locator(".text-sm")).ToContainTextAsync("editar:Alface");

        await nameCell.Locator("input").FillAsync("Alface Editada");
        await nameCell.Locator("input").PressAsync("Enter");
        await Expect(section.Locator(".text-sm")).ToContainTextAsync("salvo:Alface");
        await Expect(nameCell).ToContainTextAsync("Alface Editada");

        // Escape descarta o rascunho: o valor salvo permanece.
        await nameCell.ClickAsync();
        await nameCell.Locator("input").FillAsync("ZZZ");
        await nameCell.Locator("input").PressAsync("Escape");
        await Expect(section.Locator(".text-sm")).ToContainTextAsync("cancelado:Alface Editada");
        await Expect(nameCell).ToContainTextAsync("Alface Editada");

        AssertNoErrors(errors);
        await page.CloseAsync();
    }

    [Fact]
    public async Task Edicao_de_linha_salva_e_cancela_restaurando()
    {
        var (page, errors) = await OpenAsync();

        var section = Section(page, "row-edit");
        var firstRow = section.Locator(".p-datatable-tbody tr").First;
        await firstRow.Locator("button[aria-label='Editar']").ClickAsync();
        await Expect(firstRow.Locator("p-celleditor")).ToHaveCountAsync(2);

        await firstRow.Locator("p-celleditor input").First.FillAsync("Alface L");
        await firstRow.Locator("button[aria-label='Salvar']").ClickAsync();
        await Expect(section.Locator(".text-sm")).ToContainTextAsync("salvo:Alface L");
        await Expect(firstRow).ToContainTextAsync("Alface L");

        var secondRow = section.Locator(".p-datatable-tbody tr").Nth(1);
        await secondRow.Locator("button[aria-label='Editar']").ClickAsync();
        await secondRow.Locator("p-celleditor input").First.FillAsync("Tomate L");
        await secondRow.Locator("button[aria-label='Cancelar']").ClickAsync();
        await Expect(section.Locator(".text-sm")).ToContainTextAsync("cancelado:Tomate");
        await Expect(secondRow).ToContainTextAsync("Tomate");
        await Expect(secondRow).Not.ToContainTextAsync("Tomate L");

        AssertNoErrors(errors);
        await page.CloseAsync();
    }

    [Fact]
    public async Task Lazy_exibe_a_primeira_pagina_e_avanca()
    {
        var (page, errors) = await OpenAsync();

        var section = Section(page, "lazy-load");
        await Expect(section.Locator(".p-datatable-tbody tr")).ToHaveCountAsync(5);
        await Expect(section.Locator(".p-datatable-tbody tr").First).ToContainTextAsync("Item 1");

        await section.Locator(".p-paginator-next").ClickAsync();
        await Expect(section.Locator(".p-datatable-tbody tr").First).ToContainTextAsync("Item 6");

        AssertNoErrors(errors);
        await page.CloseAsync();
    }

    [Fact]
    public async Task Rolagens_vertical_horizontal_e_flex()
    {
        var (page, errors) = await OpenAsync();

        var vertical = Section(page, "vertical-scroll");
        var verticalStyle = await vertical.Locator(".p-datatable-table-container").GetAttributeAsync("style");
        Assert.Contains("400px", verticalStyle);

        var horizontal = Section(page, "horizontal-scroll");
        var thStyle = await horizontal.Locator("thead th").First.GetAttributeAsync("style");
        Assert.Contains("200px", thStyle);

        var flex = Section(page, "flex-scroll");
        await flex.GetByRole(AriaRole.Button, new() { Name = "Mostrar" }).ClickAsync();
        await Expect(page.Locator(".p-dialog")).ToBeVisibleAsync();
        await Expect(page.Locator(".p-dialog .p-datatable-flex-scrollable")).ToBeVisibleAsync();

        AssertNoErrors(errors);
        await page.CloseAsync();
    }

    [Fact]
    public async Task Linhas_congeladas_movem_para_o_tbody_proprio()
    {
        var (page, errors) = await OpenAsync();

        var section = Section(page, "frozen-rows");
        await Expect(section.Locator(".p-datatable-frozen-tbody tr")).ToHaveCountAsync(1);
        await Expect(section.Locator(".p-datatable-frozen-tbody")).ToContainTextAsync("Sofia Almeida");

        await section.Locator("button[aria-label='Congelar']").First.ClickAsync();
        await Expect(section.Locator(".p-datatable-frozen-tbody tr")).ToHaveCountAsync(2);
        await Expect(section.Locator(".p-datatable-frozen-tbody")).ToContainTextAsync("Pedro Santos");

        AssertNoErrors(errors);
        await page.CloseAsync();
    }

    [Fact]
    public async Task Colunas_congeladas_alternam_pelo_botao()
    {
        var (page, errors) = await OpenAsync();

        var section = Section(page, "frozen-columns");
        await Expect(section.Locator("thead th").First)
            .ToHaveClassAsync(new Regex("p-datatable-frozen-column"));
        await Expect(section.Locator("thead th").Last)
            .Not.ToHaveClassAsync(new Regex("p-datatable-frozen-column"));

        await section.Locator("button.p-togglebutton").ClickAsync();

        await Expect(section.Locator("thead th").Last)
            .ToHaveClassAsync(new Regex("p-datatable-frozen-column"));

        AssertNoErrors(errors);
        await page.CloseAsync();
    }

    [Fact]
    public async Task Grupos_de_colunas_subheader_rowspan_e_expansao()
    {
        var (page, errors) = await OpenAsync();

        var group = Section(page, "column-group");
        await Expect(group.Locator("thead tr")).ToHaveCountAsync(3);
        await Expect(group.Locator("th[rowspan='3']")).ToHaveTextAsync("Produto");
        await Expect(group.Locator("th[colspan='4']")).ToHaveTextAsync("Taxa de venda");
        await Expect(group.Locator("th[colspan='2']")).ToHaveCountAsync(2);
        await Expect(group.Locator("tfoot")).ToContainTextAsync("Totais");

        var subheader = Section(page, "subheader");
        await Expect(subheader.Locator(".p-datatable-row-group-header")).ToHaveCountAsync(3);
        await Expect(subheader.Locator(".p-datatable-row-group-header").First).ToContainTextAsync("Amy Elsner");
        await Expect(subheader.Locator(".p-rowgroup-footer")).ToHaveCountAsync(3);
        await Expect(subheader.Locator(".p-rowgroup-footer").First).ToContainTextAsync("Total de clientes");

        var expand = Section(page, "expand");
        await Expect(expand.Locator("button[aria-label='Alternar grupo']")).ToHaveCountAsync(3);
        await Expect(expand.Locator(".p-datatable-tbody")).ToContainTextAsync("Sofia Almeida");
        await expand.Locator("button[aria-label='Alternar grupo']").First.ClickAsync();
        await Expect(expand.Locator(".p-datatable-tbody")).Not.ToContainTextAsync("Sofia Almeida");

        var rowSpan = Section(page, "row-span");
        await Expect(rowSpan.Locator(".p-datatable-tbody td[rowspan]")).ToHaveCountAsync(3);
        await Expect(rowSpan.Locator(".p-datatable-tbody td[rowspan]").First).ToContainTextAsync("Amy Elsner");

        AssertNoErrors(errors);
        await page.CloseAsync();
    }

    [Fact]
    public async Task Reordenacao_de_colunas_e_linhas_por_arraste()
    {
        var (page, errors) = await OpenAsync();

        var section = Section(page, "reorder");
        await Expect(section.Locator("thead th").Nth(1)).ToContainTextAsync("Nome");

        await section.Locator("thead th").Nth(1).DragToAsync(section.Locator("thead th").Nth(2));
        await Expect(section.Locator("thead th").Nth(1)).ToContainTextAsync("Categoria");

        await Expect(section.Locator(".p-datatable-tbody tr").First).ToContainTextAsync("Alface");
        await section.Locator(".p-datatable-tbody tr").First
            .DragToAsync(section.Locator(".p-datatable-tbody tr").Nth(2));
        await Expect(section.Locator(".p-datatable-tbody tr").First).ToContainTextAsync("Tomate");

        AssertNoErrors(errors);
        await page.CloseAsync();
    }

    [Fact]
    public async Task Alternar_colunas_com_multiselect_remove_coluna()
    {
        var (page, errors) = await OpenAsync();

        var section = Section(page, "column-toggle");
        await Expect(section.Locator("thead th")).ToHaveCountAsync(5);

        await section.Locator(".p-multiselect-label-container").ClickAsync();
        await page.Locator(".p-multiselect-option").Filter(new LocatorFilterOptions { HasText = "Estoque" })
            .First.ClickAsync();

        await Expect(section.Locator("thead th")).ToHaveCountAsync(4);
        await Expect(section.Locator("thead")).Not.ToContainTextAsync("Estoque");

        AssertNoErrors(errors);
        await page.CloseAsync();
    }

    [Fact]
    public async Task Exportacao_gera_csv_em_data_uri()
    {
        var (page, errors) = await OpenAsync();

        var link = Section(page, "export").Locator("a.p-button");
        await Expect(link).ToHaveAttributeAsync("download", "produtos.csv");

        var href = await link.GetAttributeAsync("href");
        Assert.NotNull(href);
        const string prefix = "data:text/csv;charset=utf-8;base64,";
        Assert.StartsWith(prefix, href);

        var csv = Encoding.UTF8.GetString(Convert.FromBase64String(href![prefix.Length..]));
        Assert.Contains("Código", csv);
        Assert.Contains("Alface", csv);

        AssertNoErrors(errors);
        await page.CloseAsync();
    }

    [Fact]
    public async Task Menu_de_contexto_exclui_a_linha()
    {
        var (page, errors) = await OpenAsync();

        var section = Section(page, "context-menu");
        await section.Locator(".p-datatable-tbody tr").First
            .ClickAsync(new LocatorClickOptions { Button = MouseButton.Right });

        await Expect(page.Locator(".p-contextmenu")).ToBeVisibleAsync();
        await Expect(section.Locator(".text-sm")).ToContainTextAsync("Último clique direito: Alface");

        await page.GetByRole(AriaRole.Menuitem, new() { Name = "Excluir" }).ClickAsync();
        await Expect(section.Locator(".text-sm")).ToContainTextAsync("excluído:Alface");
        await Expect(section.Locator(".p-datatable-tbody tr")).ToHaveCountAsync(9);

        AssertNoErrors(errors);
        await page.CloseAsync();
    }

    [Fact]
    public async Task Estado_persistente_restaura_filtro_e_ordenacao()
    {
        var (page, errors) = await OpenAsync();

        var section = Section(page, "stateful");
        await section.Locator(".p-datatable-filter-row input").First.FillAsync("Sofia");
        await Expect(section.Locator(".p-datatable-tbody tr")).ToHaveCountAsync(1);
        await Expect(section.Locator(".text-sm")).ToContainTextAsync("Filtros ativos: 1");

        await section.Locator("thead th").Nth(1).ClickAsync();
        await Expect(section.Locator(".p-datatable-column-sorted")).ToHaveCountAsync(1);

        await page.ReloadAsync(new PageReloadOptions { WaitUntil = WaitUntilState.NetworkIdle });

        await Expect(section.Locator(".p-datatable-filter-row input").First).ToHaveValueAsync("Sofia");
        await Expect(section.Locator(".p-datatable-tbody tr")).ToHaveCountAsync(1);
        await Expect(section.Locator(".p-datatable-column-sorted")).ToHaveCountAsync(1);

        AssertNoErrors(errors);
        await page.CloseAsync();
    }
}
