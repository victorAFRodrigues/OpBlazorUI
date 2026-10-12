using System.Text;
using Microsoft.Playwright;
using Xunit;
using static Microsoft.Playwright.Assertions;

namespace OpBlazorUI.Playwright;

/// <summary>
/// Bloco 2 (exceções e travamentos): cenários que antes derrubavam o circuito ou travavam.
/// Usa a página <c>/_tests/bloco2</c> do Showcase.
/// </summary>
[Collection("showcase")]
public class Bloco2Tests
{
    private readonly ShowcaseFixture _fixture;

    public Bloco2Tests(ShowcaseFixture fixture) => _fixture = fixture;

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
        await page.GotoAsync($"{_fixture.BaseUrl}/_tests/bloco2", new PageGotoOptions { WaitUntil = WaitUntilState.NetworkIdle });
        await page.Locator("#t-title").WaitForAsync();
        return page;
    }

    private static FilePayload TextFile(string name, string mime, int sizeBytes = 4)
        => new() { Name = name, MimeType = mime, Buffer = Encoding.UTF8.GetBytes(new string('x', sizeBytes)) };

    [Fact]
    public async Task FileUpload_limite_excedido_nao_lanca()
    {
        var errors = new List<string>();
        var page = await NewPageAsync(errors);

        var input = page.Locator("#t-file-limit .op-fileupload-input").First;
        await input.SetInputFilesAsync(new[]
        {
            TextFile("a.txt", "text/plain"),
            TextFile("b.txt", "text/plain"),
            TextFile("c.txt", "text/plain"),
        });
        await Task.Delay(300);

        // FileLimit=2: aceita dois e mostra a mensagem, sem InvalidOperationException.
        await Expect(page.Locator("#t-file-limit .p-fileupload-file")).ToHaveCountAsync(2);
        await Expect(page.Locator("#t-file-limit .p-message")).ToBeVisibleAsync();

        Assert.Empty(errors);
        await page.CloseAsync();
    }

    [Fact]
    public async Task FileUpload_accept_wildcard_aceita_qualquer_tipo()
    {
        var errors = new List<string>();
        var page = await NewPageAsync(errors);

        var input = page.Locator("#t-file-accept .op-fileupload-input").First;
        await input.SetInputFilesAsync(TextFile("dados.bin", "application/octet-stream"));
        await Task.Delay(300);

        await Expect(page.Locator("#t-file-accept .p-fileupload-file-name")).ToHaveTextAsync("dados.bin");
        await Expect(page.Locator("#t-file-accept .p-message")).ToHaveCountAsync(0);

        Assert.Empty(errors);
        await page.CloseAsync();
    }

    [Fact]
    public async Task FileUpload_escolha_invalida_mantem_a_anterior()
    {
        var errors = new List<string>();
        var page = await NewPageAsync(errors);

        await page.Locator("#t-file-single .op-fileupload-input").First.SetInputFilesAsync(TextFile("ok.png", "image/png"));
        await Task.Delay(300);
        await Expect(page.Locator("#t-file-single .p-fileupload-file-name")).ToHaveTextAsync("ok.png");

        await page.Locator("#t-file-single .op-fileupload-input").First.SetInputFilesAsync(TextFile("nao.txt", "text/plain"));
        await Task.Delay(300);

        // A escolha inválida não apaga a válida anterior.
        await Expect(page.Locator("#t-file-single .p-fileupload-file-name")).ToHaveTextAsync("ok.png");

        Assert.Empty(errors);
        await page.CloseAsync();
    }

    [Fact]
    public async Task InputOtp_com_template_digita_sem_lancar()
    {
        var errors = new List<string>();
        var page = await NewPageAsync(errors);

        var first = page.Locator("#t-otp input").First;
        await first.FillAsync("1");
        await page.Keyboard.PressAsync("Tab");
        await Task.Delay(200);

        Assert.Empty(errors);
        await page.CloseAsync();
    }

    [Fact]
    public async Task SliderRange_com_valor_fora_do_intervalo_nao_lanca()
    {
        var errors = new List<string>();
        var page = await NewPageAsync(errors);

        var handle = page.Locator("#t-range .p-slider-handle").First;
        await handle.FocusAsync();
        await page.Keyboard.PressAsync("ArrowRight");
        await page.Keyboard.PressAsync("ArrowRight");
        await Task.Delay(200);

        Assert.Empty(errors);
        await page.CloseAsync();
    }

    [Fact]
    public async Task DataTable_5000_linhas_pagina_sem_excecao()
    {
        var errors = new List<string>();
        var page = await NewPageAsync(errors);

        var table = page.Locator("#t-table .p-datatable").First;
        await table.WaitForAsync();
        await Expect(table.Locator(".p-datatable-tbody tr")).ToHaveCountAsync(10);

        var firstBefore = await table.Locator(".p-datatable-tbody tr").First.InnerTextAsync();
        await page.Locator("#t-table .p-paginator-next").ClickAsync();
        await Task.Delay(250);
        var firstAfter = await table.Locator(".p-datatable-tbody tr").First.InnerTextAsync();

        Assert.NotEqual(firstBefore, firstAfter);
        Assert.Empty(errors);
        await page.CloseAsync();
    }
}
