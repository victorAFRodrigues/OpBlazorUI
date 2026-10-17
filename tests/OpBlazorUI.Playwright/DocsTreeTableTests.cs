using System.Text.RegularExpressions;
using Microsoft.Playwright;
using Xunit;
using static Microsoft.Playwright.Assertions;

namespace OpBlazorUI.Playwright;

/// <summary>
/// Página de documentação do <c>OpTreeTable</c>: seções, ordem do upstream e demos
/// interativos.
/// </summary>
[Collection("showcase")]
public class DocsTreeTableTests
{
    private readonly ShowcaseFixture _fixture;

    public DocsTreeTableTests(ShowcaseFixture fixture) => _fixture = fixture;

    /// <summary>Ids das seções de recursos, na ordem-alvo (upstream + extras mantidos).</summary>
    private static readonly string[] SectionIds =
    [
        "import-demo", "basic", "dynamiccolumns", "controlled", "template", "size", "gridlines",
        "loading-mask", "loading-skeleton", "paginatorbasic", "paginatortemplate",
        "sortsinglecolumn", "sortmultiplecolumns", "sortremovable", "filter",
        "selectionsingle", "selectionmultiple", "checkbox", "events", "columngroup", "lazyload",
        "edit", "vertical", "flexible", "horizontal", "frozencolumns",
        "fitmode", "expandmode", "scrollable", "reorder", "columntoggle", "conditionalstyle",
        "contextmenu", "accessibility"
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

        await page.GotoAsync($"{_fixture.BaseUrl}/treetable",
            new PageGotoOptions { WaitUntil = WaitUntilState.NetworkIdle });
        return (page, errors);
    }

    private static ILocator Section(IPage page, string id) =>
        page.Locator($"section.doc-section:has(h2#{id})");

    private static void AssertNoErrors(List<string> errors) =>
        Assert.True(errors.Count == 0, string.Join(" || ", errors));

    [Fact]
    public async Task Pagina_da_treetable_tem_todas_as_secoes()
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
        foreach (var id in new[] { "api-props", "api-templates", "api-events", "api-column", "api-headergroup" })
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
    public async Task Basico_mostra_as_raizes_e_expande()
    {
        var (page, errors) = await OpenAsync();

        var section = Section(page, "basic");
        await Expect(section.Locator(".p-treetable-tbody tr")).ToHaveCountAsync(4);

        await section.Locator(".p-treetable-node-toggle-button").First.ClickAsync();
        await Expect(section.Locator(".p-treetable-tbody tr")).ToHaveCountAsync(6);

        AssertNoErrors(errors);
        await page.CloseAsync();
    }

    [Fact]
    public async Task Colunas_dinamicas_com_templates()
    {
        var (page, errors) = await OpenAsync();

        var section = Section(page, "dynamiccolumns");
        await Expect(section.Locator(".p-treetable-tbody tr").First.Locator(".pi-folder")).ToBeVisibleAsync();
        await Expect(section.Locator(".p-treetable-tbody tr").First).ToContainTextAsync("Documentos");
        await Expect(section.Locator(".p-treetable-tbody tr").First).ToContainTextAsync("125 KB");

        AssertNoErrors(errors);
        await page.CloseAsync();
    }

    [Fact]
    public async Task Expansao_controlada_pela_pagina()
    {
        var (page, errors) = await OpenAsync();

        var section = Section(page, "controlled");
        await Expect(section.Locator(".p-treetable-tbody tr")).ToHaveCountAsync(4);

        // O log vem dos eventos de expandir/recolher do componente...
        await section.Locator(".p-treetable-node-toggle-button").First.ClickAsync();
        await Expect(section.Locator(".p-treetable-tbody tr")).ToHaveCountAsync(6);
        await Expect(section.Locator(".text-sm")).ToContainTextAsync("expandido: Documentos");

        // ...e a página controla a expansão de todos os nós por código.
        await section.GetByRole(AriaRole.Button, new() { Name = "Alternar pastas" }).ClickAsync();
        await Expect(section.Locator(".p-treetable-tbody tr")).ToHaveCountAsync(12);

        await section.GetByRole(AriaRole.Button, new() { Name = "Alternar pastas" }).ClickAsync();
        await Expect(section.Locator(".p-treetable-tbody tr")).ToHaveCountAsync(4);

        AssertNoErrors(errors);
        await page.CloseAsync();
    }

    [Fact]
    public async Task Template_caption_e_rodape()
    {
        var (page, errors) = await OpenAsync();

        var section = Section(page, "template");
        await Expect(section.Locator(".p-treetable-header")).ToContainTextAsync("Sistema de arquivos");
        await Expect(section.Locator("tfoot")).ToContainTextAsync("Total de itens: 12");
        await Expect(section.Locator(".p-treetable-tbody tr").First.Locator(".pi-folder")).ToBeVisibleAsync();

        AssertNoErrors(errors);
        await page.CloseAsync();
    }

    [Fact]
    public async Task Tamanho_e_linhas_de_grade()
    {
        var (page, errors) = await OpenAsync();

        var size = Section(page, "size");
        await Expect(size.Locator(".p-treetable")).ToHaveClassAsync(new Regex("p-treetable-sm"));
        await size.Locator(".p-selectbutton").GetByText("large").ClickAsync();
        await Expect(size.Locator(".p-treetable")).ToHaveClassAsync(new Regex("p-treetable-lg"));

        await Expect(Section(page, "gridlines").Locator(".p-treetable")).ToHaveClassAsync(new Regex("p-treetable-gridlines"));

        AssertNoErrors(errors);
        await page.CloseAsync();
    }

    [Fact]
    public async Task Carregamento_mask_e_skeleton()
    {
        var (page, errors) = await OpenAsync();

        var mask = Section(page, "loading-mask");
        await mask.GetByRole(AriaRole.Button, new() { Name = "Carregar (2 s)" }).ClickAsync();
        await Expect(mask.Locator(".p-treetable-mask")).ToBeVisibleAsync();
        await Expect(mask.Locator(".p-treetable-mask")).ToBeHiddenAsync(new() { Timeout = 10000 });

        var skeleton = Section(page, "loading-skeleton");
        await skeleton.GetByRole(AriaRole.Button, new() { Name = "Carregar (2 s)" }).ClickAsync();
        await Expect(skeleton.Locator(".p-treetable-skeleton-row")).ToHaveCountAsync(5);

        AssertNoErrors(errors);
        await page.CloseAsync();
    }

    [Fact]
    public async Task Paginacao_basica_e_com_templates()
    {
        var (page, errors) = await OpenAsync();

        var basic = Section(page, "paginatorbasic");
        await Expect(basic.Locator(".p-treetable-tbody tr")).ToHaveCountAsync(2);
        await Expect(basic.Locator(".p-treetable-tbody tr").First).ToContainTextAsync("Documentos");
        await Expect(basic.Locator(".p-paginator")).ToHaveCountAsync(2);

        await basic.Locator(".p-paginator-next").First.ClickAsync();
        await Expect(basic.Locator(".p-treetable-tbody tr").First).ToContainTextAsync("Vídeos");
        await Expect(basic.Locator(".text-sm")).ToContainTextAsync("Última página navegada: 2");

        var template = Section(page, "paginatortemplate");
        await Expect(template.Locator(".p-paginator-content-end")).ToContainTextAsync("Raízes: 4");
        await template.Locator(".p-paginator-next").ClickAsync();
        await Expect(template.Locator(".p-treetable-tbody tr").First).ToContainTextAsync("Vídeos");
        await template.GetByRole(AriaRole.Button, new() { Name = "Reiniciar" }).ClickAsync();
        await Expect(template.Locator(".p-treetable-tbody tr").First).ToContainTextAsync("Documentos");

        AssertNoErrors(errors);
        await page.CloseAsync();
    }

    [Fact]
    public async Task Ordenacoes_simples_multipla_e_removivel()
    {
        var (page, errors) = await OpenAsync();

        // Coluna única: asc (menor tamanho primeiro) e desc.
        var single = Section(page, "sortsinglecolumn");
        var sizeHeader = single.Locator("thead th:has-text('Tamanho')");
        await sizeHeader.ClickAsync();
        await Expect(single.Locator(".p-treetable-tbody tr").First).ToContainTextAsync("README.md");
        await sizeHeader.ClickAsync();
        await Expect(single.Locator(".p-treetable-tbody tr").First).ToContainTextAsync("Vídeos");

        // Múltiplas colunas: meta + clique adiciona à ordenação.
        var multi = Section(page, "sortmultiplecolumns");
        await multi.Locator("thead th:has-text('Nome')").ClickAsync();
        await multi.Locator("thead th:has-text('Tamanho')").ClickAsync(
            new LocatorClickOptions { Modifiers = new[] { KeyboardModifier.Meta } });
        await Expect(multi.Locator(".text-sm")).ToContainTextAsync("name (asc)");
        await Expect(multi.Locator(".text-sm")).ToContainTextAsync("size (asc)");

        // Removível: terceiro clique remove a ordenação.
        var removable = Section(page, "sortremovable");
        var typeHeader = removable.Locator("thead th:has-text('Tipo')");
        await typeHeader.ClickAsync();
        await typeHeader.ClickAsync();
        await typeHeader.ClickAsync();
        await Expect(removable.Locator(".text-sm")).ToContainTextAsync("nenhuma");

        AssertNoErrors(errors);
        await page.CloseAsync();
    }

    [Fact]
    public async Task Filtro_por_coluna_e_global()
    {
        var (page, errors) = await OpenAsync();

        var section = Section(page, "filter");
        await Expect(section.Locator(".p-treetable-filter-row")).ToBeVisibleAsync();

        await section.Locator(".p-treetable-filter-row input").First.FillAsync("contrato");
        await Expect(section.Locator(".p-treetable-tbody tr")).ToHaveCountAsync(3);
        await Expect(section.Locator(".p-treetable-tbody tr").Last).ToContainTextAsync("contrato.pdf");

        await section.Locator(".p-treetable-filter-row input").First.FillAsync("");
        await Expect(section.Locator(".p-treetable-tbody tr")).ToHaveCountAsync(4);

        // Filtro global desenhado na caption: ancestral expande até o match.
        await section.Locator(".p-treetable-header input").FillAsync("docx");
        await Expect(section.Locator(".p-treetable-tbody tr")).ToHaveCountAsync(3);
        await Expect(section.Locator(".p-treetable-tbody tr").First).ToContainTextAsync("Documentos");
        await Expect(section.Locator(".p-treetable-tbody tr").Last).ToContainTextAsync("curriculo.docx");

        // Trocar para strict mantém o resultado deste match.
        await section.Locator(".p-selectbutton").GetByText("strict").ClickAsync();
        await Expect(section.Locator(".p-treetable-tbody tr")).ToHaveCountAsync(3);

        AssertNoErrors(errors);
        await page.CloseAsync();
    }

    [Fact]
    public async Task Selecoes_unica_multipla_e_checkbox()
    {
        var (page, errors) = await OpenAsync();

        var single = Section(page, "selectionsingle");
        await single.Locator(".p-treetable-tbody tr").Nth(1).ClickAsync();
        await Expect(single.Locator(".text-sm")).ToContainTextAsync("Selecionado: Imagens");
        await Expect(single.Locator(".p-treetable-tbody tr").Nth(1)).ToHaveAttributeAsync("aria-selected", "true");

        var multiple = Section(page, "selectionmultiple");
        await multiple.Locator(".p-treetable-tbody tr").Nth(0).ClickAsync();
        await multiple.Locator(".p-treetable-tbody tr").Nth(2).ClickAsync();
        await Expect(multiple.Locator(".text-sm")).ToContainTextAsync("Selecionados: 2");

        var checkbox = Section(page, "checkbox");
        await checkbox.Locator(".p-treetable-tbody tr").First.Locator("input[type=checkbox]").ClickAsync();
        await Expect(checkbox.Locator(".text-sm")).ToContainTextAsync("Marcados: 6");
        await checkbox.Locator("thead input[type=checkbox]").ClickAsync();
        await Expect(checkbox.Locator(".text-sm")).ToContainTextAsync("Marcados: 12");

        AssertNoErrors(errors);
        await page.CloseAsync();
    }

    [Fact]
    public async Task Eventos_de_selecao_registram_o_no()
    {
        var (page, errors) = await OpenAsync();

        var section = Section(page, "events");
        await section.Locator(".p-treetable-tbody tr").First.ClickAsync();
        await Expect(section.Locator(".text-sm")).ToContainTextAsync("selecionado: Documentos");

        await section.Locator(".p-treetable-tbody tr").First.ClickAsync();
        await Expect(section.Locator(".text-sm")).ToContainTextAsync("desselecionado: Documentos");

        AssertNoErrors(errors);
        await page.CloseAsync();
    }

    [Fact]
    public async Task Colunas_agrupadas_com_rowspan_e_colspan()
    {
        var (page, errors) = await OpenAsync();

        var section = Section(page, "columngroup");
        await Expect(section.Locator("thead tr")).ToHaveCountAsync(2);

        var firstRow = section.Locator("thead tr").First;
        await Expect(firstRow.Locator("th")).ToHaveCountAsync(2);
        await Expect(firstRow.Locator("th").First).ToHaveAttributeAsync("rowspan", "2");
        await Expect(firstRow.Locator("th").First).ToContainTextAsync("Arquivo");
        await Expect(firstRow.Locator("th").Nth(1)).ToHaveAttributeAsync("colspan", "2");

        var leafRow = section.Locator("thead tr").Nth(1);
        await Expect(leafRow.Locator("th")).ToHaveCountAsync(2);
        await Expect(leafRow.Locator("th").First).ToContainTextAsync("Tamanho");
        await Expect(leafRow.Locator("th").Nth(1)).ToContainTextAsync("Tipo");

        await Expect(section.Locator("tfoot td").First).ToContainTextAsync("Totais");
        await Expect(section.Locator("tfoot td").Nth(1)).ToContainTextAsync("2.134 KB");

        AssertNoErrors(errors);
        await page.CloseAsync();
    }

    [Fact]
    public async Task Lazy_carrega_os_filhos_ao_expandir()
    {
        var (page, errors) = await OpenAsync();

        var section = Section(page, "lazyload");
        await Expect(section.Locator(".p-treetable-tbody tr")).ToHaveCountAsync(2);

        await section.Locator(".p-treetable-node-toggle-button").First.ClickAsync();
        await Expect(section.Locator(".p-treetable-node-toggle-icon.pi-spin")).ToBeVisibleAsync();
        await Expect(section.Locator(".p-treetable-tbody tr")).ToHaveCountAsync(4);
        await Expect(section.Locator(".text-sm")).ToContainTextAsync("carregados");

        AssertNoErrors(errors);
        await page.CloseAsync();
    }

    [Fact]
    public async Task Rolagens_vertical_flexivel_horizontal_e_frozen()
    {
        var (page, errors) = await OpenAsync();

        await Expect(Section(page, "vertical").Locator(".p-treetable-table-container"))
            .ToHaveAttributeAsync("style", new Regex("max-height: 200px"));

        var horizontal = Section(page, "horizontal");
        await Expect(horizontal.Locator(".p-treetable-table")).ToHaveAttributeAsync("style", new Regex("min-width: 60rem"));

        var frozen = Section(page, "frozencolumns");
        await Expect(frozen.Locator(".p-treetable-tbody tr").First.Locator("td").First)
            .ToHaveAttributeAsync("style", new Regex("left: 0px"));
        await Expect(frozen.Locator(".p-treetable-tbody tr").First.Locator("td").Last)
            .ToHaveAttributeAsync("style", new Regex("right: 0px"));

        // Flexível: a tabela dentro do diálogo usa a altura do pai.
        await Section(page, "flexible").GetByRole(AriaRole.Button, new() { Name = "Mostrar" }).ClickAsync();
        await Expect(page.Locator(".p-dialog .p-treetable.p-treetable-flex-scrollable")).ToBeVisibleAsync();

        AssertNoErrors(errors);
        await page.CloseAsync();
    }

    [Fact]
    public async Task Reordenacao_e_alternancia_de_colunas()
    {
        var (page, errors) = await OpenAsync();

        var reorder = Section(page, "reorder");
        await Expect(reorder.Locator(".text-sm")).ToContainTextAsync("Colunas: Nome, Tamanho, Tipo");
        await reorder.Locator("thead th").Nth(0).DragToAsync(reorder.Locator("thead th").Nth(2));
        await Expect(reorder.Locator(".text-sm")).ToContainTextAsync("Colunas: Tamanho, Tipo, Nome");

        var toggle = Section(page, "columntoggle");
        await Expect(toggle.Locator("thead th")).ToHaveCountAsync(3);
        await toggle.Locator(".p-multiselect-label-container").ClickAsync();
        await page.Locator(".p-multiselect-option").Filter(new LocatorFilterOptions { HasText = "Tipo" })
            .First.ClickAsync();
        await Expect(toggle.Locator("thead th")).ToHaveCountAsync(2);
        await Expect(toggle.Locator("thead")).Not.ToContainTextAsync("Tipo");

        AssertNoErrors(errors);
        await page.CloseAsync();
    }

    [Fact]
    public async Task Estilo_condicional_e_menu_de_contexto()
    {
        var (page, errors) = await OpenAsync();

        var conditional = Section(page, "conditionalstyle");
        await Expect(conditional.Locator(".p-treetable-tbody tr").First).ToHaveClassAsync(new Regex("font-bold"));
        await Expect(conditional.Locator(".p-treetable-tbody tr").Last).Not.ToHaveClassAsync(new Regex("font-bold"));

        var context = Section(page, "contextmenu");
        await context.Locator(".p-treetable-tbody tr").First
            .ClickAsync(new LocatorClickOptions { Button = MouseButton.Right });
        await Expect(page.Locator(".p-contextmenu")).ToBeVisibleAsync();
        await Expect(context.Locator(".text-sm")).ToContainTextAsync("Último clique direito: Documentos");

        await page.GetByRole(AriaRole.Menuitem, new() { Name = "Excluir" }).ClickAsync();
        await Expect(context.Locator(".text-sm")).ToContainTextAsync("Ação: Excluir Documentos");

        AssertNoErrors(errors);
        await page.CloseAsync();
    }

    [Fact]
    public async Task Edicao_e_redimensionamento_ficam_documentados_sem_demo()
    {
        var (page, errors) = await OpenAsync();

        foreach (var id in new[] { "edit", "fitmode", "expandmode", "scrollable" })
        {
            var section = Section(page, id);
            await Expect(section).ToContainTextAsync("não foi portad");
        }

        AssertNoErrors(errors);
        await page.CloseAsync();
    }
}
