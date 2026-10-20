using System.Text.RegularExpressions;
using Microsoft.Playwright;
using Xunit;
using static Microsoft.Playwright.Assertions;

namespace OpBlazorUI.Playwright;

/// <summary>
/// ColorPicker, Slider e diretivas. Usa a página <c>/_tests/color-picker-slider</c> do
/// Showcase. O <c>OpFilterService</c> é coberto por <see cref="FilterServiceTests"/>.
/// </summary>
[Collection("showcase")]
public class ColorPickerSliderTests
{
    private readonly ShowcaseFixture _fixture;

    public ColorPickerSliderTests(ShowcaseFixture fixture) => _fixture = fixture;

    private async Task<IPage> NewPageAsync(List<string> consoleErrors, TimeSpan? moduleDelay = null)
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

        if (moduleDelay is not null)
        {
            await page.RouteAsync("**/colorpicker.interop.js", async route =>
            {
                await Task.Delay(moduleDelay.Value);
                await route.ContinueAsync();
            });
        }

        await page.GotoAsync($"{_fixture.BaseUrl}/_tests/color-picker-slider", new PageGotoOptions { WaitUntil = WaitUntilState.NetworkIdle });
        await page.Locator("#t-title").WaitForAsync();
        return page;
    }

    [Fact]
    public async Task ColorPicker_mantem_a_precisao_do_hex()
    {
        var errors = new List<string>();
        var page = await NewPageAsync(errors);

        await page.Locator("#t-color-input").ClickAsync();
        var selector = page.Locator("#t-color .p-colorpicker-color-selector");
        await Expect(selector).ToBeVisibleAsync();
        await page.WaitForTimeoutAsync(300);

        var box = await selector.BoundingBoxAsync();
        Assert.NotNull(box);
        await page.Mouse.ClickAsync((float)(box!.X + box.Width * (2.0 / 3.0)), (float)(box.Y + box.Height * 0.4));

        await Expect(page.Locator("#t-color-value")).ToHaveTextAsync("#336699");

        Assert.True(errors.Count == 0, string.Join(" || ", errors));
        await page.CloseAsync();
    }

    [Fact]
    public async Task ColorPicker_aceita_hex_de_8_digitos_e_invalido()
    {
        var errors = new List<string>();
        var page = await NewPageAsync(errors);

        var style8 = await page.Locator("#t-color8-input").GetAttributeAsync("style");
        Assert.Contains("336699", style8);

        var styleBad = await page.Locator("#t-color-bad-input").GetAttributeAsync("style");
        Assert.Contains("ff0000", styleBad);

        Assert.True(errors.Count == 0, string.Join(" || ", errors));
        await page.CloseAsync();
    }

    [Fact]
    public async Task ColorPicker_disabled_inline_recebe_p_disabled()
    {
        var errors = new List<string>();
        var page = await NewPageAsync(errors);

        await Expect(page.Locator("#t-color-disabled .p-colorpicker")).ToHaveClassAsync(new Regex("p-disabled"));

        Assert.True(errors.Count == 0, string.Join(" || ", errors));
        await page.CloseAsync();
    }

    [Fact]
    public async Task ColorPicker_preview_mostra_a_cor_sem_texto()
    {
        var errors = new List<string>();
        var page = await NewPageAsync(errors);

        // Como no upstream, a prévia é só o quadrado colorido: sem value/texto.
        await Expect(page.Locator("#t-color-input")).ToHaveValueAsync("");

        var style = await page.Locator("#t-color-input").GetAttributeAsync("style");
        Assert.Contains("336699", style);

        Assert.True(errors.Count == 0, string.Join(" || ", errors));
        await page.CloseAsync();
    }

    [Fact]
    public async Task ColorPicker_fecha_ao_clicar_fora()
    {
        var errors = new List<string>();

        // Atrasa o interop como no CI: o painel só pode aparecer depois que o listener de
        // clique fora estiver registrado, senão o clique imediato no título se perde.
        var page = await NewPageAsync(errors, TimeSpan.FromMilliseconds(500));

        await page.Locator("#t-color-input").ClickAsync();
        await Expect(page.Locator("#t-color .p-colorpicker-color-selector")).ToBeVisibleAsync();

        // O clique fora chega por [JSInvokable]; sem render o painel continuaria no DOM.
        await page.Locator("#t-title").ClickAsync();
        await Expect(page.Locator("#t-color .p-colorpicker-color-selector")).ToHaveCountAsync(0);

        Assert.True(errors.Count == 0, string.Join(" || ", errors));
        await page.CloseAsync();
    }

    [Fact]
    public async Task ColorPicker_arraste_sem_binding_atualiza_a_previa()
    {
        var errors = new List<string>();
        var page = await NewPageAsync(errors);

        // Caso com Value estático (sem ValueChanged): o arraste chega por [JSInvokable] e a
        // prévia precisa acompanhar mesmo sem re-render do consumidor.
        var before = await page.Locator("#t-color8-input").GetAttributeAsync("style");
        Assert.Contains("336699", before);

        await page.Locator("#t-color8-input").ClickAsync();
        var selector = page.Locator("#t-color8 .p-colorpicker-color-selector");
        await Expect(selector).ToBeVisibleAsync();
        await page.WaitForTimeoutAsync(300);

        var box = await selector.BoundingBoxAsync();
        Assert.NotNull(box);
        await page.Mouse.ClickAsync((float)(box!.X + box.Width * 0.5), (float)(box.Y + box.Height * 0.5));

        await Expect(page.Locator("#t-color8-input")).Not.ToHaveAttributeAsync("style", new Regex("336699"));

        Assert.True(errors.Count == 0, string.Join(" || ", errors));
        await page.CloseAsync();
    }

    [Fact]
    public async Task Slider_nao_controlado_move_o_handle()
    {
        var errors = new List<string>();
        var page = await NewPageAsync(errors);

        var slider = page.Locator("#t-slider .p-slider");
        var box = await slider.BoundingBoxAsync();
        Assert.NotNull(box);
        await page.Mouse.ClickAsync((float)(box!.X + box.Width * 0.5), (float)(box.Y + box.Height * 0.5));

        await Expect(page.Locator("#t-slider .p-slider-handle")).ToHaveAttributeAsync("aria-valuenow", "50");

        Assert.True(errors.Count == 0, string.Join(" || ", errors));
        await page.CloseAsync();
    }

    [Fact]
    public async Task Slider_setas_nao_rolam_a_pagina()
    {
        var errors = new List<string>();
        var page = await NewPageAsync(errors);

        var handle = page.Locator("#t-slider .p-slider-handle");
        await handle.FocusAsync();

        var before = await page.EvaluateAsync<double>("() => window.scrollY");
        await page.Keyboard.PressAsync("ArrowRight");
        await Expect(handle).ToHaveAttributeAsync("aria-valuenow", "1");
        var after = await page.EvaluateAsync<double>("() => window.scrollY");

        Assert.Equal(before, after);

        Assert.True(errors.Count == 0, string.Join(" || ", errors));
        await page.CloseAsync();
    }

    [Fact]
    public async Task SliderRange_destrava_com_os_dois_handles_no_max()
    {
        var errors = new List<string>();
        var page = await NewPageAsync(errors);

        await Expect(page.Locator("#t-range-values")).ToHaveTextAsync("100,100");

        // Com os dois handles no Max, o handle de cima precisa conseguir descer.
        var handle1 = page.Locator("#t-slider-range .p-slider-handle[data-op-slider-handle='1']");
        await handle1.FocusAsync();
        await page.Keyboard.PressAsync("ArrowLeft");

        await Expect(page.Locator("#t-range-values")).ToHaveTextAsync("100,99");

        Assert.True(errors.Count == 0, string.Join(" || ", errors));
        await page.CloseAsync();
    }

    [Fact]
    public async Task StyleClass_reinicia_quando_o_parametro_muda()
    {
        var errors = new List<string>();
        var page = await NewPageAsync(errors);

        await page.Locator("#t-sc-btn").ClickAsync();
        await Expect(page.Locator("#t-sc-target")).ToHaveClassAsync(new Regex("ativo"));

        await page.Locator("#t-sc-change").ClickAsync();
        await page.Locator("#t-sc-btn").ClickAsync();
        await Expect(page.Locator("#t-sc-target")).ToHaveClassAsync(new Regex("marcado"));

        Assert.True(errors.Count == 0, string.Join(" || ", errors));
        await page.CloseAsync();
    }
}
