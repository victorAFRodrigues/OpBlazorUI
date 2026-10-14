using System.Text.RegularExpressions;
using Microsoft.Playwright;
using Xunit;
using static Microsoft.Playwright.Assertions;

namespace OpBlazorUI.Playwright;

/// <summary>
/// Bloco 3.8 (overlays). Usa a página <c>/_tests/bloco3-8</c>.
/// </summary>
[Collection("showcase")]
public class Bloco3_8Tests
{
    private readonly ShowcaseFixture _fixture;

    public Bloco3_8Tests(ShowcaseFixture fixture) => _fixture = fixture;

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
        await page.GotoAsync($"{_fixture.BaseUrl}/_tests/bloco3-8", new PageGotoOptions { WaitUntil = WaitUntilState.NetworkIdle });
        await page.Locator("#t-title").WaitForAsync();
        return page;
    }

    private static async Task WaitForFocusAsync(IPage page, string id)
    {
        for (var i = 0; i < 50; i++)
        {
            var active = await page.EvaluateAsync<string>("() => (document.activeElement && document.activeElement.id) || ''");
            if (active == id) return;
            await Task.Delay(100);
        }

        var final = await page.EvaluateAsync<string>("() => (document.activeElement && document.activeElement.id) || ''");
        Assert.Equal(id, final);
    }

    [Fact]
    public async Task Dialog_restaura_foco_ao_fechar_pelo_pai()
    {
        var errors = new List<string>();
        var page = await NewPageAsync(errors);

        await page.Locator("#t-dialog-open").ClickAsync();
        await Expect(page.Locator("#t-dialog [role=dialog]")).ToBeVisibleAsync();

        await page.Locator("#t-dialog-close-parent").ClickAsync();
        await Expect(page.Locator("#t-dialog [role=dialog]")).ToHaveCountAsync(0);
        await WaitForFocusAsync(page, "t-dialog-open");

        Assert.True(errors.Count == 0, string.Join(" || ", errors));
        await page.CloseAsync();
    }

    [Fact]
    public async Task Dialog_restaura_foco_no_Dispose()
    {
        var errors = new List<string>();
        var page = await NewPageAsync(errors);

        await page.Locator("#t-dialog2-open").ClickAsync();
        await Expect(page.Locator("#t-dialog-dispose [role=dialog]")).ToBeVisibleAsync();

        await page.Locator("#t-dialog2-remove").ClickAsync();
        await Expect(page.Locator("#t-dialog-dispose [role=dialog]")).ToHaveCountAsync(0);
        await WaitForFocusAsync(page, "t-dialog2-open");

        Assert.True(errors.Count == 0, string.Join(" || ", errors));
        await page.CloseAsync();
    }

    [Fact]
    public async Task Drawer_restaura_foco_ao_fechar_pelo_pai()
    {
        var errors = new List<string>();
        var page = await NewPageAsync(errors);

        await page.Locator("#t-drawer-open").ClickAsync();
        await Expect(page.Locator("#t-drawer [role=complementary]")).ToBeVisibleAsync();

        await page.Locator("#t-drawer-close-parent").ClickAsync();
        await Expect(page.Locator("#t-drawer [role=complementary]")).ToHaveCountAsync(0);
        await WaitForFocusAsync(page, "t-drawer-open");

        Assert.True(errors.Count == 0, string.Join(" || ", errors));
        await page.CloseAsync();
    }

    [Fact]
    public async Task ConfirmDialog_usa_Position_e_DismissableMask_resolvidos()
    {
        var errors = new List<string>();
        var page = await NewPageAsync(errors);

        await page.Locator("#t-confirm-open").ClickAsync();
        await Expect(page.Locator(".p-confirmdialog")).ToBeVisibleAsync();
        await Expect(page.Locator(".p-dialog-mask")).ToHaveClassAsync(new Regex("p-dialog-top"));

        // DismissableMask vindo das opções: clique na máscara fecha.
        await page.Mouse.ClickAsync(5, 5);
        await Expect(page.Locator(".p-confirmdialog")).ToHaveCountAsync(0);

        Assert.True(errors.Count == 0, string.Join(" || ", errors));
        await page.CloseAsync();
    }

    [Fact]
    public async Task ConfirmDialog_Close_do_servico_fecha()
    {
        var errors = new List<string>();
        var page = await NewPageAsync(errors);

        await page.Locator("#t-confirm-svc-open").ClickAsync();
        await Expect(page.Locator(".p-confirmdialog")).ToBeVisibleAsync();

        await page.Locator("#t-confirm-close-service").ClickAsync();
        await Expect(page.Locator(".p-confirmdialog")).ToHaveCountAsync(0);

        Assert.True(errors.Count == 0, string.Join(" || ", errors));
        await page.CloseAsync();
    }

    [Fact]
    public async Task ConfirmDialog_Accept_encadeia_nova_confirmacao()
    {
        var errors = new List<string>();
        var page = await NewPageAsync(errors);

        await page.Locator("#t-confirm-chain").ClickAsync();
        var dialog = page.Locator(".p-confirmdialog");
        await Expect(dialog).ToContainTextAsync("Primeiro");

        await page.Locator(".p-confirmdialog .p-confirmdialog-accept-button").ClickAsync();
        await Expect(dialog).ToBeVisibleAsync();
        await Expect(dialog).ToContainTextAsync("Segundo");

        Assert.True(errors.Count == 0, string.Join(" || ", errors));
        await page.CloseAsync();
    }

    [Fact]
    public async Task VirtualScroller_disabled_renderiza_tudo()
    {
        var errors = new List<string>();
        var page = await NewPageAsync(errors);

        await Expect(page.Locator("#t-vs-disabled .vs-item")).ToHaveCountAsync(20);

        Assert.True(errors.Count == 0, string.Join(" || ", errors));
        await page.CloseAsync();
    }

    [Fact]
    public async Task VirtualScroller_lazy_com_dados_iniciais_vazios_carrega()
    {
        var errors = new List<string>();
        var page = await NewPageAsync(errors);

        await Expect(page.Locator("#t-vs-lazy-count")).ToHaveTextAsync("5");
        await Expect(page.Locator("#t-vs-lazy .vs-item").First).ToBeVisibleAsync();

        Assert.True(errors.Count == 0, string.Join(" || ", errors));
        await page.CloseAsync();
    }

    [Fact]
    public async Task VirtualScroller_horizontal_usa_scrollLeft()
    {
        var errors = new List<string>();
        var page = await NewPageAsync(errors);

        await Expect(page.Locator("#t-vs-h-first")).ToHaveTextAsync("0");

        await page.EvaluateAsync(
            "() => { const el = document.getElementById('t-vs-h-scroller'); el.scrollLeft = 600; el.dispatchEvent(new Event('scroll')); }");

        await Expect(page.Locator("#t-vs-h-first")).Not.ToHaveTextAsync("0");

        Assert.True(errors.Count == 0, string.Join(" || ", errors));
        await page.CloseAsync();
    }

    [Fact]
    public async Task Editor_normaliza_conteudo_vazio_para_null()
    {
        var errors = new List<string>();
        var page = await NewPageAsync(errors);

        await page.Locator("#t-editor .ql-editor").ClickAsync();
        await page.Keyboard.TypeAsync("abc");
        await Expect(page.Locator("#t-editor-value")).Not.ToHaveTextAsync("null");

        await page.Keyboard.PressAsync("ControlOrMeta+A");
        await page.Keyboard.PressAsync("Delete");
        await Expect(page.Locator("#t-editor-value")).ToHaveTextAsync("null");

        Assert.True(errors.Count == 0, string.Join(" || ", errors));
        await page.CloseAsync();
    }

    [Fact]
    public async Task Overlay_nao_modal_fecha_com_clique_fora()
    {
        var errors = new List<string>();
        var page = await NewPageAsync(errors);

        await page.Locator("#t-overlay-toggle").ClickAsync();
        await Expect(page.Locator("#t-overlay-content")).ToBeVisibleAsync();

        await page.Locator("#t-title").ClickAsync();
        await Expect(page.Locator("#t-overlay-content")).ToHaveCountAsync(0);

        Assert.True(errors.Count == 0, string.Join(" || ", errors));
        await page.CloseAsync();
    }
}
