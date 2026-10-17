using Microsoft.Playwright;
using Xunit;
using static Microsoft.Playwright.Assertions;

namespace OpBlazorUI.Playwright;

[Collection("showcase")]
public class DocsLote3Tests
{
    private readonly ShowcaseFixture _fixture;

    public DocsLote3Tests(ShowcaseFixture fixture) => _fixture = fixture;

    private async Task<(IPage page, List<string> errors)> OpenAsync(string route)
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

        await page.GotoAsync($"{_fixture.BaseUrl}{route}",
            new PageGotoOptions { WaitUntil = WaitUntilState.NetworkIdle });
        return (page, errors);
    }

    private static ILocator Section(IPage page, string id) =>
        page.Locator($"section.doc-section:has(h2#{id})");

    private static async Task AssertSectionsAsync(IPage page, params string[] ids)
    {
        foreach (var id in ids)
        {
            await Expect(Section(page, id)).ToBeVisibleAsync();
        }
    }

    [Fact]
    public async Task Ripple_secao_default()
    {
        var (page, errors) = await OpenAsync("/ripple");
        await AssertSectionsAsync(page, "import", "default", "custom", "accessibility");
        Assert.Empty(errors);
        await page.CloseAsync();
    }

    [Fact]
    public async Task Toolbar_secao_custom_com_avatar()
    {
        var (page, errors) = await OpenAsync("/toolbar");
        await AssertSectionsAsync(page, "import", "basic", "custom", "content", "accessibility");
        await Expect(Section(page, "custom").Locator(".p-toolbar .p-avatar").First).ToBeVisibleAsync();
        Assert.Empty(errors);
        await page.CloseAsync();
    }

    [Fact]
    public async Task Accordion_secoes_novas()
    {
        var (page, errors) = await OpenAsync("/accordion");
        await AssertSectionsAsync(page, "import", "basic", "dynamic", "multiple", "disabled", "controlled", "template", "accessibility");

        var controlled = Section(page, "controlled").Locator(".p-accordion").First;
        await Expect(controlled.Locator(".p-accordionpanel").Nth(0)).ToHaveClassAsync(new System.Text.RegularExpressions.Regex("p-accordionpanel-active"));
        await Section(page, "controlled").Locator("button:has-text('2')").First.ClickAsync();
        await Expect(controlled.Locator(".p-accordionpanel").Nth(1)).ToHaveClassAsync(new System.Text.RegularExpressions.Regex("p-accordionpanel-active"));

        // O painel desabilitado não abre: o cabeçalho fica indisponível para interação.
        var disabled = Section(page, "disabled").Locator(".p-accordion").First;
        await Expect(disabled.Locator(".p-accordionheader").Nth(3)).ToHaveAttributeAsync("aria-disabled", "true");
        await Expect(disabled.Locator(".p-accordionpanel").Nth(3)).Not.ToHaveClassAsync(new System.Text.RegularExpressions.Regex("p-accordionpanel-active"));

        Assert.Empty(errors);
        await page.CloseAsync();
    }

    [Fact]
    public async Task Button_secao_directive()
    {
        var (page, errors) = await OpenAsync("/button");
        await AssertSectionsAsync(page, "import", "basic", "directive", "anchor", "accessibility");
        Assert.Empty(errors);
        await page.CloseAsync();
    }

    [Fact]
    public async Task Slider_secao_filter()
    {
        var (page, errors) = await OpenAsync("/slider");
        await AssertSectionsAsync(page, "import", "basic", "filter", "vertical");

        var filter = Section(page, "filter");
        await Expect(filter.Locator(".p-selectbutton")).ToBeVisibleAsync();
        await Expect(filter.Locator(".p-slider")).ToBeVisibleAsync();
        await Expect(filter.Locator("img")).ToBeVisibleAsync();

        Assert.Empty(errors);
        await page.CloseAsync();
    }

    [Fact]
    public async Task AutoComplete_secoes_novas()
    {
        var (page, errors) = await OpenAsync("/autocomplete");
        // No branch atual as seções existentes ainda não têm Id (o PR de auditoria adiciona);
        // aqui validamos as seções novas, que já nascem com Id.
        await AssertSectionsAsync(page, "basicchips", "advancedchips", "floatlabel", "iftalabel", "filled");

        // Chips avançado: o autocomplete com múltiplos + dropdown renderiza os chips.
        var advanced = Section(page, "advancedchips");
        await Expect(advanced.Locator(".p-autocomplete .p-autocomplete-input-multiple")).ToBeVisibleAsync();
        await Expect(advanced.Locator(".p-autocomplete-dropdown")).ToBeVisibleAsync();

        // FloatLabel e IftaLabel embrulham o autocomplete.
        await Expect(Section(page, "floatlabel").Locator(".p-floatlabel .p-autocomplete").First).ToBeVisibleAsync();
        await Expect(Section(page, "iftalabel").Locator(".p-iftalabel .p-autocomplete").First).ToBeVisibleAsync();

        Assert.Empty(errors);
        await page.CloseAsync();
    }

    // --- Lote 3, etapa 3A: menus (command/router/template e correlatos) ---

    [Fact]
    public async Task ContextMenu_secoes_novas()
    {
        var (page, errors) = await OpenAsync("/contextmenu");
        await AssertSectionsAsync(page, "import", "basic", "document", "template", "command", "router", "table", "accessibility");
        Assert.Empty(errors);
        await page.CloseAsync();
    }

    [Fact]
    public async Task PanelMenu_secoes_novas()
    {
        var (page, errors) = await OpenAsync("/panelmenu");
        await AssertSectionsAsync(page, "import", "basic", "multiple", "controlled", "template", "command", "router", "accessibility");

        // Controlado: o botão expande todos os painéis.
        var controlled = Section(page, "controlled").Locator(".p-panelmenu").First;
        await Section(page, "controlled").Locator("button").First.ClickAsync();
        await Expect(controlled.Locator(".p-panelmenu-header-active").First).ToBeVisibleAsync();

        Assert.Empty(errors);
        await page.CloseAsync();
    }

    [Fact]
    public async Task MegaMenu_secoes_novas()
    {
        var (page, errors) = await OpenAsync("/megamenu");
        await AssertSectionsAsync(page, "import", "basic", "vertical", "template", "command", "router", "accessibility");
        await Expect(page.Locator(".p-megamenu-vertical").First).ToBeVisibleAsync();
        Assert.Empty(errors);
        await page.CloseAsync();
    }

    [Fact]
    public async Task TieredMenu_secoes_novas()
    {
        var (page, errors) = await OpenAsync("/tieredmenu");
        await AssertSectionsAsync(page, "import", "basic", "popup", "template", "command", "router", "accessibility");

        // Popup: o botão "Alternar" abre o overlay do tieredmenu.
        await Section(page, "popup").Locator("button").First.ClickAsync();
        await Expect(page.Locator(".p-tieredmenu-overlay").First).ToBeVisibleAsync();

        Assert.Empty(errors);
        await page.CloseAsync();
    }

    [Fact]
    public async Task Menubar_secoes_novas()
    {
        var (page, errors) = await OpenAsync("/menubar");
        await AssertSectionsAsync(page, "import", "basic", "icons", "template", "command", "router", "accessibility");
        Assert.Empty(errors);
        await page.CloseAsync();
    }

    [Fact]
    public async Task Breadcrumb_secoes_novas()
    {
        var (page, errors) = await OpenAsync("/breadcrumb");
        await AssertSectionsAsync(page, "import", "basic", "home", "icons", "template", "router", "accessibility");
        Assert.Empty(errors);
        await page.CloseAsync();
    }

    // --- Lote 3, etapa 3B: abas, passos e overlays ---

    [Fact]
    public async Task Tabs_secoes_novas()
    {
        var (page, errors) = await OpenAsync("/tabs");
        await AssertSectionsAsync(page, "import", "basic", "dynamic", "controlled", "lazy", "disabled", "template", "tabmenu", "accessibility");

        // Dinâmico: a lista de abas renderiza normalmente.
        await Expect(Section(page, "dynamic").Locator(".p-tablist").First).ToBeVisibleAsync();

        // TabMenu: componente dedicado de navegação por abas.
        await Expect(Section(page, "tabmenu").Locator(".p-tabmenu").First).ToBeVisibleAsync();

        Assert.Empty(errors);
        await page.CloseAsync();
    }

    [Fact]
    public async Task Steps_secoes_novas()
    {
        var (page, errors) = await OpenAsync("/steps");
        await AssertSectionsAsync(page, "import", "basic", "controlled", "interactive", "routing", "accessibility");
        await Expect(Section(page, "interactive").Locator(".p-steps").First).ToBeVisibleAsync();
        Assert.Empty(errors);
        await page.CloseAsync();
    }

    [Fact]
    public async Task DynamicDialog_secoes_novas()
    {
        var (page, errors) = await OpenAsync("/dynamicdialog");
        await AssertSectionsAsync(page, "import", "usage", "open", "customization", "passingdata", "close", "example");

        // Exemplo: abrir o diálogo de seleção de produto.
        await Section(page, "example").Locator("button").First.ClickAsync();
        await Expect(page.Locator(".p-dialog").First).ToBeVisibleAsync();

        Assert.Empty(errors);
        await page.CloseAsync();
    }

    [Fact]
    public async Task ConfirmPopup_secoes_novas()
    {
        var (page, errors) = await OpenAsync("/confirmpopup");
        await AssertSectionsAsync(page, "import", "basic", "custom", "template", "headless", "events", "accessibility");

        // Template: abre o popup ancorado ao botão.
        await Section(page, "template").Locator("button").First.ClickAsync();
        await Expect(page.Locator(".p-confirmpopup").First).ToBeVisibleAsync();

        Assert.Empty(errors);
        await page.CloseAsync();
    }

    [Fact]
    public async Task FileUpload_secoes_novas()
    {
        var (page, errors) = await OpenAsync("/fileupload");
        await AssertSectionsAsync(page, "import", "basic", "auto", "advanced", "simple", "custom", "validation", "template", "accessibility");
        await Expect(Section(page, "advanced").Locator(".p-fileupload-advanced").First).ToBeVisibleAsync();
        Assert.Empty(errors);
        await page.CloseAsync();
    }

    // --- Lote 3, etapa 3C: galeria e árvore ---

    [Fact]
    public async Task Galleria_secoes_novas()
    {
        var (page, errors) = await OpenAsync("/galleria");
        await AssertSectionsAsync(page,
            "import", "basic", "controlled",
            "indicator", "clickevent", "hoverevent", "position", "template",
            "thumbnail", "responsive",
            "fullscreen", "withthumbnails", "withtouthumbnails", "customcontent",
            "navigator", "itemwiththumbnails", "itemwithtouthumbnails", "hover", "withindicators",
            "autoplay", "caption", "advanced", "accessibility");

        // Indicadores: a lista de indicadores é renderizada.
        await Expect(Section(page, "clickevent").Locator(".p-galleria-indicator").First).ToBeVisibleAsync();

        // Navegadores: botões de anterior/próximo sobre o item.
        await Expect(Section(page, "itemwiththumbnails").Locator(".p-galleria-nav-button").First).ToBeVisibleAsync();

        Assert.Empty(errors);
        await page.CloseAsync();
    }

    [Fact]
    public async Task Tree_secoes_nao_suportadas()
    {
        var (page, errors) = await OpenAsync("/tree");
        await AssertSectionsAsync(page,
            "import", "basic", "controlled", "single", "multiple", "checkbox", "event", "lazy", "template", "filter",
            "preload", "lazyvirtualscroll", "dragdrop-single", "dragdrop-multiple", "contextmenu", "accessibility");
        Assert.Empty(errors);
        await page.CloseAsync();
    }

    // --- Lote 3, etapa 3D: Knob em formulários ---

    [Fact]
    public async Task Knob_secao_forms()
    {
        var (page, errors) = await OpenAsync("/knob");
        await AssertSectionsAsync(page, "import", "basic", "minmax", "step", "template", "stroke", "size", "color", "readonly", "forms", "disabled", "accessibility");
        await Expect(Section(page, "forms").Locator("form").First).ToBeVisibleAsync();
        Assert.Empty(errors);
        await page.CloseAsync();
    }
}
