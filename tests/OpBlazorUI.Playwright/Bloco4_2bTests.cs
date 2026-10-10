using Microsoft.Playwright;
using Xunit;
using static Microsoft.Playwright.Assertions;

namespace OpBlazorUI.Playwright;

/// <summary>
/// Bloco 4.2b: blockScroll (Dialog/BlockUI), arrastar e redimensionar o Dialog.
/// Usa a página <c>/_tests/bloco4-2b</c> do Showcase.
/// </summary>
[Collection("showcase")]
public class Bloco4_2bTests
{
    private readonly ShowcaseFixture _fixture;

    public Bloco4_2bTests(ShowcaseFixture fixture) => _fixture = fixture;

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
        await page.GotoAsync($"{_fixture.BaseUrl}/_tests/bloco4-2b", new PageGotoOptions { WaitUntil = WaitUntilState.NetworkIdle });
        await page.Locator("#t-title").WaitForAsync();
        return page;
    }

    [Fact]
    public async Task Dialog_com_blockScroll_trava_e_restaura_a_rolagem()
    {
        var errors = new List<string>();
        var page = await NewPageAsync(errors);

        await page.Locator("#t-open-scroll").ClickAsync();
        await Expect(page.Locator("#t-scroll .p-dialog")).ToBeVisibleAsync();
        await Expect(page.Locator("#t-scroll .p-dialog-header")).ToBeVisibleAsync();

        Assert.Equal("hidden", await page.EvaluateAsync<string>("() => document.body.style.overflow"));

        await page.Locator("#t-scroll .p-dialog-close-button").ClickAsync();
        await Expect(page.Locator("#t-scroll .p-dialog")).ToHaveCountAsync(0);
        Assert.NotEqual("hidden", await page.EvaluateAsync<string>("() => document.body.style.overflow"));

        Assert.True(errors.Count == 0, string.Join(" || ", errors));
        await page.CloseAsync();
    }

    [Fact]
    public async Task Dialog_arrasta_pelo_cabecalho()
    {
        var errors = new List<string>();
        var page = await NewPageAsync(errors);

        await page.Locator("#t-open-drag").ClickAsync();
        await Expect(page.Locator("#t-drag .p-dialog")).ToBeVisibleAsync();

        var header = page.Locator("#t-drag .p-dialog-header");
        var box = await header.BoundingBoxAsync();
        Assert.NotNull(box);

        await page.Mouse.MoveAsync(box!.X + 20, box.Y + box.Height / 2);
        await page.Mouse.DownAsync();
        await page.Mouse.MoveAsync(box.X + 140, box.Y + box.Height / 2 + 60, new() { Steps = 5 });
        await page.Mouse.UpAsync();

        var transform = await page.Locator("#t-drag .p-dialog").EvaluateAsync<string>("el => el.style.transform || ''");
        Assert.Contains("translate", transform);

        Assert.True(errors.Count == 0, string.Join(" || ", errors));
        await page.CloseAsync();
    }

    [Fact]
    public async Task Dialog_redimensiona_pelo_canto()
    {
        var errors = new List<string>();
        var page = await NewPageAsync(errors);

        await page.Locator("#t-open-drag").ClickAsync();
        var dialog = page.Locator("#t-drag .p-dialog");
        await Expect(dialog).ToBeVisibleAsync();

        var before = await dialog.BoundingBoxAsync();
        Assert.NotNull(before);

        var handle = page.Locator("#t-drag [data-op-resize=se]");
        var hb = await handle.BoundingBoxAsync();
        Assert.NotNull(hb);

        await page.Mouse.MoveAsync(hb!.X + hb.Width / 2, hb.Y + hb.Height / 2);
        await page.Mouse.DownAsync();
        await page.Mouse.MoveAsync(hb.X + hb.Width / 2 + 120, hb.Y + hb.Height / 2 + 80, new() { Steps = 5 });
        await page.Mouse.UpAsync();

        var after = await dialog.BoundingBoxAsync();
        Assert.NotNull(after);
        Assert.True(after!.Width > before!.Width + 50, $"largura {before.Width} -> {after.Width}");

        Assert.True(errors.Count == 0, string.Join(" || ", errors));
        await page.CloseAsync();
    }

    [Fact]
    public async Task BlockUI_fullscreen_trava_a_rolagem()
    {
        var errors = new List<string>();
        var page = await NewPageAsync(errors);

        await page.Locator("#t-toggle-block").ClickAsync();
        await Expect(page.Locator("#t-blockui .p-blockui-mask")).ToBeVisibleAsync();
        Assert.Equal("hidden", await page.EvaluateAsync<string>("() => document.body.style.overflow"));

        // A máscara cobre o botão (pointer-events); alterna pelo teclado com o botão em foco.
        await page.Keyboard.PressAsync("Enter");
        await Expect(page.Locator("#t-blockui .p-blockui-mask")).ToHaveCountAsync(0);
        Assert.NotEqual("hidden", await page.EvaluateAsync<string>("() => document.body.style.overflow"));

        Assert.True(errors.Count == 0, string.Join(" || ", errors));
        await page.CloseAsync();
    }

    [Fact]
    public async Task Tooltip_some_sozinho_com_Life()
    {
        var errors = new List<string>();
        var page = await NewPageAsync(errors);

        await page.Locator("#t-tt-btn").HoverAsync();
        var tooltip = page.Locator("#t-tooltip .p-tooltip");
        await Expect(tooltip).ToBeVisibleAsync();

        // Life=600: some sozinho mesmo com o cursor parado sobre o alvo.
        await Expect(tooltip).ToHaveCountAsync(0);

        Assert.True(errors.Count == 0, string.Join(" || ", errors));
        await page.CloseAsync();
    }

    [Fact]
    public async Task Toast_pausa_o_fechamento_no_hover()
    {
        var errors = new List<string>();
        var page = await NewPageAsync(errors);

        await page.Locator("#t-toast-add").ClickAsync();
        var toast = page.Locator("#t-toast .p-toast-message");
        await Expect(toast).ToBeVisibleAsync();

        await toast.HoverAsync();
        await Task.Delay(1800);
        await Expect(toast).ToBeVisibleAsync();

        await page.Mouse.MoveAsync(5, 5);
        await Expect(toast).ToHaveCountAsync(0);

        Assert.True(errors.Count == 0, string.Join(" || ", errors));
        await page.CloseAsync();
    }

    [Fact]
    public async Task ContextMenu_global_abre_no_clique_direito()
    {
        var errors = new List<string>();
        var page = await NewPageAsync(errors);

        var area = page.Locator("#t-ctx-area");
        var box = await area.BoundingBoxAsync();
        Assert.NotNull(box);

        await page.Mouse.ClickAsync(box!.X + 5, box.Y + 5, new() { Button = MouseButton.Right });
        await Expect(page.Locator("#t-ctx .p-contextmenu")).ToBeVisibleAsync();

        Assert.True(errors.Count == 0, string.Join(" || ", errors));
        await page.CloseAsync();
    }
}
