using Microsoft.Playwright;
using Xunit;
using static Microsoft.Playwright.Assertions;

namespace OpBlazorUI.Playwright;

/// <summary>
/// Base comum (bloco 1): fechamento de overlays por clique fora e Escape, descarte do
/// Accordion e números do Slider numa cultura com vírgula decimal. Usa a página
/// <c>/_tests/bloco1</c> do Showcase.
/// </summary>
[Collection("showcase")]
public class Bloco1Tests
{
    private readonly ShowcaseFixture _fixture;

    public Bloco1Tests(ShowcaseFixture fixture) => _fixture = fixture;

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
        await page.GotoAsync($"{_fixture.BaseUrl}/_tests/bloco1", new PageGotoOptions { WaitUntil = WaitUntilState.NetworkIdle });
        await page.Locator("#t-select .p-select").WaitForAsync();
        return page;
    }

    // O registro para clique-fora/Escape acontece no primeiro render via JS. O z-index inline é
    // aplicado no mesmo passo do registro (attachParent), então serve de sinal determinístico.
    private static Task WaitOverlayRegisteredAsync(ILocator overlay)
        => Expect(overlay).ToHaveAttributeAsync("style", new System.Text.RegularExpressions.Regex("z-index"));

    [Fact]
    public async Task Select_aberto_pela_seta_fecha_com_clique_fora()
    {
        var errors = new List<string>();
        var page = await NewPageAsync(errors);

        // A seta não dá foco ao combobox: antes, sem focusout, o painel nunca fechava.
        await page.Locator("#t-select .p-select-dropdown").ClickAsync();
        var panel = page.Locator(".p-select-overlay");
        await Expect(panel).ToBeVisibleAsync();
        await WaitOverlayRegisteredAsync(panel);

        await page.Locator("#t-title").ClickAsync();
        await Expect(panel).ToHaveCountAsync(0);

        Assert.Empty(errors);
        await page.CloseAsync();
    }

    [Fact]
    public async Task Escape_num_Select_dentro_do_Dialog_fecha_so_o_Select()
    {
        var errors = new List<string>();
        var page = await NewPageAsync(errors);

        await page.Locator("#t-open-dialog").ClickAsync();
        var dialog = page.Locator(".p-dialog");
        await Expect(dialog).ToBeVisibleAsync();
        await WaitOverlayRegisteredAsync(page.Locator(".p-dialog-mask"));

        await page.Locator("#t-dialog-select .p-select-dropdown").ClickAsync();
        var panel = page.Locator(".p-select-overlay");
        await Expect(panel).ToBeVisibleAsync();
        await WaitOverlayRegisteredAsync(panel);

        await page.Keyboard.PressAsync("Escape");
        await Expect(panel).ToHaveCountAsync(0);
        await Expect(dialog).ToBeVisibleAsync();

        await page.Keyboard.PressAsync("Escape");
        await Expect(dialog).ToHaveCountAsync(0);

        Assert.Empty(errors);
        await page.CloseAsync();
    }

    [Fact]
    public async Task Menu_popup_fecha_com_clique_fora_e_com_Escape()
    {
        var errors = new List<string>();
        var page = await NewPageAsync(errors);

        var toggle = page.Locator("#t-menu-toggle");
        var menu = page.Locator(".p-menu-overlay");

        await toggle.ClickAsync();
        await Expect(menu).ToBeVisibleAsync();
        await WaitOverlayRegisteredAsync(menu);
        await page.Locator("#t-title").ClickAsync();
        await Expect(menu).ToHaveCountAsync(0);

        await toggle.ClickAsync();
        await Expect(menu).ToBeVisibleAsync();
        await WaitOverlayRegisteredAsync(menu);
        await page.Keyboard.PressAsync("Escape");
        await Expect(menu).ToHaveCountAsync(0);

        // Clicar no próprio botão alterna (não é "clique fora").
        await toggle.ClickAsync();
        await Expect(menu).ToBeVisibleAsync();
        await WaitOverlayRegisteredAsync(menu);
        await toggle.ClickAsync();
        await Expect(menu).ToHaveCountAsync(0);

        Assert.Empty(errors);
        await page.CloseAsync();
    }

    [Fact]
    public async Task Accordion_renumera_os_paineis_ao_remover_um()
    {
        var errors = new List<string>();
        var page = await NewPageAsync(errors);

        var headers = page.Locator("#t-accordion .p-accordionheader");
        await Expect(headers).ToHaveCountAsync(2);

        // Remove o primeiro: o segundo passa a ser o índice 0.
        await page.Locator("#t-toggle-first").ClickAsync();
        await Expect(headers).ToHaveCountAsync(1);

        await headers.First.ClickAsync(); // fecha (o índice 0 estava ativo)
        await headers.First.ClickAsync(); // abre de novo
        await Expect(page.Locator("#t-active")).ToHaveTextAsync("0");
        await Expect(headers.First).ToHaveAttributeAsync("aria-expanded", "true");

        // Recoloca o primeiro: cada painel tem um índice distinto, sem colidir.
        await page.Locator("#t-toggle-first").ClickAsync();
        await Expect(headers).ToHaveCountAsync(2);
        await Expect(page.Locator("#t-accordion .p-accordionheader[aria-expanded=true]")).ToHaveCountAsync(1);

        Assert.Empty(errors);
        await page.CloseAsync();
    }

    [Fact]
    public async Task Slider_usa_ponto_decimal_no_estilo_numa_cultura_com_virgula()
    {
        var errors = new List<string>();
        var page = await NewPageAsync(errors);

        // Value=30, Max=80 → 37,5%: com vírgula o navegador descartava o estilo.
        var handle = page.Locator("#t-slider .p-slider-handle");
        await Expect(handle).ToHaveAttributeAsync("style", new System.Text.RegularExpressions.Regex(@"37\.5%"));
        await Expect(page.Locator("#t-slider .p-slider-range")).ToHaveAttributeAsync("style", new System.Text.RegularExpressions.Regex(@"width: 37\.5%"));

        Assert.Empty(errors);
        await page.CloseAsync();
    }
}
