using Microsoft.Playwright;
using Xunit;
using static Microsoft.Playwright.Assertions;

namespace OpBlazorUI.Playwright;

/// <summary>
/// Bloco 3.6 (abas, accordion e navegação). Usa a página <c>/_tests/bloco3-6</c>.
/// </summary>
[Collection("showcase")]
public class Bloco3_6Tests
{
    private readonly ShowcaseFixture _fixture;

    public Bloco3_6Tests(ShowcaseFixture fixture) => _fixture = fixture;

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
        await page.GotoAsync($"{_fixture.BaseUrl}/_tests/bloco3-6", new PageGotoOptions { WaitUntil = WaitUntilState.NetworkIdle });
        await page.Locator("#t-title").WaitForAsync();
        return page;
    }

    [Fact]
    public async Task Tabs_remove_sem_cabecalho_fantasma_e_ajusta_o_ativo()
    {
        var errors = new List<string>();
        var page = await NewPageAsync(errors);

        await Expect(page.Locator("#t-tabs .p-tab")).ToHaveCountAsync(3);

        await page.Locator("#t-tabs .p-tab").Nth(2).ClickAsync();
        await Expect(page.Locator("#t-tabs-active")).ToHaveTextAsync("2");
        await Expect(page.Locator("#t-tabs .p-tabpanel")).ToContainTextAsync("conteudo-tres");

        await page.Locator("#t-tabs-remove").ClickAsync();
        await Expect(page.Locator("#t-tabs .p-tab")).ToHaveCountAsync(2);
        await Expect(page.Locator("#t-tabs-active")).ToHaveTextAsync("1");
        await Expect(page.Locator("#t-tabs .p-tabpanel")).ToContainTextAsync("conteudo-dois");

        Assert.True(errors.Count == 0, string.Join(" || ", errors));
        await page.CloseAsync();
    }

    [Fact]
    public async Task Tabs_ids_unicos_entre_instancias()
    {
        var errors = new List<string>();
        var page = await NewPageAsync(errors);

        var id1 = await page.Locator("#t-tabs .p-tab").First.GetAttributeAsync("id");
        var id2 = await page.Locator("#t-tabs2 .p-tab").First.GetAttributeAsync("id");

        Assert.NotEqual(id1, id2);

        Assert.True(errors.Count == 0, string.Join(" || ", errors));
        await page.CloseAsync();
    }

    [Fact]
    public async Task Accordion_com_ActiveIndexes_nulo_nao_lanca()
    {
        var errors = new List<string>();
        var page = await NewPageAsync(errors);

        await Expect(page.Locator("#t-accordion-active")).ToHaveTextAsync("null");

        await page.Locator("#t-accordion .p-accordionheader").First.ClickAsync();
        await Expect(page.Locator("#t-accordion-active")).ToHaveTextAsync("0");
        await Expect(page.Locator("#t-accordion .p-accordioncontent").First).ToContainTextAsync("um");

        Assert.True(errors.Count == 0, string.Join(" || ", errors));
        await page.CloseAsync();
    }

    [Fact]
    public async Task Fieldset_Enter_alterna_uma_vez()
    {
        var errors = new List<string>();
        var page = await NewPageAsync(errors);

        var button = page.Locator("#t-fieldset .p-fieldset-toggle-button");
        await button.ClickAsync();
        await Expect(page.Locator("#t-fieldset-count")).ToHaveTextAsync("1");

        await button.FocusAsync();
        await page.Keyboard.PressAsync("Enter");
        await Expect(page.Locator("#t-fieldset-count")).ToHaveTextAsync("2");

        Assert.True(errors.Count == 0, string.Join(" || ", errors));
        await page.CloseAsync();
    }

    [Fact]
    public async Task Steps_Enter_dispara_uma_vez_e_executa_command()
    {
        var errors = new List<string>();
        var page = await NewPageAsync(errors);

        var step = page.Locator("#t-steps .p-steps-item-link").Nth(1);
        await step.FocusAsync();
        await page.Keyboard.PressAsync("Enter");

        await Expect(page.Locator("#t-steps-changes")).ToHaveTextAsync("1");
        await Expect(page.Locator("#t-steps-command")).ToHaveTextAsync("dois");

        Assert.True(errors.Count == 0, string.Join(" || ", errors));
        await page.CloseAsync();
    }

    [Fact]
    public async Task Inplace_PreventClick_nao_bloqueia_o_fechamento()
    {
        var errors = new List<string>();
        var page = await NewPageAsync(errors);

        await Expect(page.Locator("#t-inplace-content")).ToBeVisibleAsync();

        await page.Locator("#t-inplace .p-inplace-content .p-button").ClickAsync();
        await Expect(page.Locator("#t-inplace-active")).ToHaveTextAsync("False");
        await Expect(page.Locator("#t-inplace-display")).ToBeVisibleAsync();

        // PreventClick impede reabrir pelo display (continua fechado).
        await page.Locator("#t-inplace-display").ClickAsync();
        await Expect(page.Locator("#t-inplace-active")).ToHaveTextAsync("False");

        Assert.True(errors.Count == 0, string.Join(" || ", errors));
        await page.CloseAsync();
    }

    [Fact]
    public async Task Breadcrumb_respeita_Visible_Disabled_Command_e_Target()
    {
        var errors = new List<string>();
        var page = await NewPageAsync(errors);

        await Expect(page.Locator("#t-breadcrumb")).Not.ToContainTextAsync("Oculto");

        var disabled = page.Locator("#t-breadcrumb .p-breadcrumb-item-link.p-disabled");
        await Expect(disabled).ToHaveCountAsync(1);
        await Expect(disabled).ToContainTextAsync("Desabilitado");

        var first = page.Locator("#t-breadcrumb a.p-breadcrumb-item-link").First;
        await Expect(first).ToHaveAttributeAsync("target", "_blank");
        await first.ClickAsync();
        await Expect(page.Locator("#t-bc-command")).ToHaveTextAsync("um");

        Assert.True(errors.Count == 0, string.Join(" || ", errors));
        await page.CloseAsync();
    }

    [Fact]
    public async Task DataView_RowsChanged_e_controlavel()
    {
        var errors = new List<string>();
        var page = await NewPageAsync(errors);

        await Expect(page.Locator("#t-dv-rows")).ToHaveTextAsync("2");

        await page.Locator("#t-dataview .p-paginator-rpp-dropdown").SelectOptionAsync("5");
        await Expect(page.Locator("#t-dv-rows")).ToHaveTextAsync("5");

        Assert.True(errors.Count == 0, string.Join(" || ", errors));
        await page.CloseAsync();
    }

    [Fact]
    public async Task MeterGroup_renderiza_start_e_end_templates()
    {
        var errors = new List<string>();
        var page = await NewPageAsync(errors);

        await Expect(page.Locator("#t-meter-start")).ToHaveTextAsync("inicio");
        await Expect(page.Locator("#t-meter-end")).ToHaveTextAsync("fim");

        Assert.True(errors.Count == 0, string.Join(" || ", errors));
        await page.CloseAsync();
    }
}
