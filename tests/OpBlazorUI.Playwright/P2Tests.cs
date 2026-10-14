using Microsoft.Playwright;
using Xunit;
using static Microsoft.Playwright.Assertions;

namespace OpBlazorUI.Playwright;

[Collection("showcase")]
public class P2Tests
{
    private readonly ShowcaseFixture _fixture;

    public P2Tests(ShowcaseFixture fixture) => _fixture = fixture;

    private async Task<IPage> NewPageAsync(List<string> consoleErrors, string route)
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
        await page.GotoAsync($"{_fixture.BaseUrl}{route}", new PageGotoOptions { WaitUntil = WaitUntilState.NetworkIdle });
        return page;
    }

    [Fact]
    public async Task Panel_expande_e_colapsa()
    {
        var errors = new List<string>();
        var page = await NewPageAsync(errors, "/panel");

        var panel = page.Locator(".p-panel-toggleable").First;
        await panel.WaitForAsync();
        var toggle = panel.Locator(".p-panel-toggle-button").First;
        await toggle.WaitForAsync();

        var before = await toggle.GetAttributeAsync("aria-expanded");
        await toggle.ClickAsync();
        await Task.Delay(150);
        var after = await toggle.GetAttributeAsync("aria-expanded");

        Assert.NotEqual(before, after);
        Assert.Empty(errors);
        await page.CloseAsync();
    }

    [Fact]
    public async Task Splitter_renderiza_e_move_divisor()
    {
        var errors = new List<string>();
        var page = await NewPageAsync(errors, "/splitter");

        var splitter = page.Locator(".p-splitter").First;
        await splitter.WaitForAsync();
        Assert.True(await splitter.Locator(".p-splitterpanel").CountAsync() >= 2);
        Assert.True(await splitter.Locator(".p-splitter-gutter").CountAsync() >= 1);

        var firstPanel = splitter.Locator(".p-splitterpanel").First;
        var before = await firstPanel.GetAttributeAsync("style");

        var handle = splitter.Locator(".p-splitter-gutter-handle").First;
        await handle.FocusAsync();
        await page.Keyboard.PressAsync("ArrowRight");
        await Task.Delay(150);

        var after = await firstPanel.GetAttributeAsync("style");
        Assert.NotEqual(before, after);
        Assert.Empty(errors);
        await page.CloseAsync();
    }

    [Fact]
    public async Task ScrollPanel_renderiza_conteudo()
    {
        var errors = new List<string>();
        var page = await NewPageAsync(errors, "/scrollpanel");

        await Expect(page.Locator(".p-scrollpanel").First).ToBeVisibleAsync();
        await Expect(page.Locator(".p-scrollpanel-content").First).ToBeVisibleAsync();

        Assert.Empty(errors);
        await page.CloseAsync();
    }

    [Fact]
    public async Task Knob_responde_ao_teclado()
    {
        var errors = new List<string>();
        var page = await NewPageAsync(errors, "/knob");

        var svg = page.Locator(".p-knob svg").First;
        await svg.WaitForAsync();
        var before = await svg.GetAttributeAsync("aria-valuenow");

        await svg.FocusAsync();
        await page.Keyboard.PressAsync("ArrowUp");
        await Task.Delay(150);
        var after = await svg.GetAttributeAsync("aria-valuenow");

        Assert.NotEqual(before, after);
        Assert.Empty(errors);
        await page.CloseAsync();
    }

    [Fact]
    public async Task Image_abre_preview()
    {
        var errors = new List<string>();
        var page = await NewPageAsync(errors, "/image");

        await Expect(page.Locator(".p-image img").First).ToBeVisibleAsync();
        var mask = page.Locator(".p-image-preview-mask").First;
        await mask.WaitForAsync();
        await mask.ClickAsync();

        await Expect(page.Locator(".p-image-mask").First).ToBeVisibleAsync();
        Assert.Empty(errors);
        await page.CloseAsync();
    }

    [Fact]
    public async Task ImageCompare_move_controle()
    {
        var errors = new List<string>();
        var page = await NewPageAsync(errors, "/imagecompare");

        var root = page.Locator(".p-imagecompare").First;
        await root.WaitForAsync();
        var slider = root.Locator(".p-imagecompare-slider").First;
        await slider.WaitForAsync();

        await slider.EvaluateAsync("el => { el.value = 70; el.dispatchEvent(new Event('input', { bubbles: true })); }");
        await Task.Delay(150);

        var style = await root.GetAttributeAsync("style");
        Assert.Contains("70", style);
        Assert.Empty(errors);
        await page.CloseAsync();
    }

    [Fact]
    public async Task Galleria_troca_item_por_miniatura()
    {
        var errors = new List<string>();
        var page = await NewPageAsync(errors, "/galleria");

        var galleria = page.Locator(".p-galleria").First;
        await galleria.WaitForAsync();
        var thumbs = galleria.Locator(".p-galleria-thumbnail-item");
        Assert.True(await thumbs.CountAsync() >= 3);

        await thumbs.Nth(1).ClickAsync();
        await Task.Delay(150);

        await Expect(thumbs.Nth(1)).ToHaveClassAsync(new System.Text.RegularExpressions.Regex("p-galleria-thumbnail-item-current"));
        Assert.Empty(errors);
        await page.CloseAsync();
    }

    [Fact]
    public async Task Carousel_avanca_pagina()
    {
        var errors = new List<string>();
        var page = await NewPageAsync(errors, "/carousel");

        var carousel = page.Locator(".p-carousel").First;
        await carousel.WaitForAsync();
        var list = carousel.Locator(".p-carousel-item-list").First;
        var before = await list.GetAttributeAsync("style");

        await carousel.Locator(".p-carousel-next-button").First.ClickAsync();
        await Task.Delay(600);
        var after = await list.GetAttributeAsync("style");

        Assert.NotEqual(before, after);
        Assert.Empty(errors);
        await page.CloseAsync();
    }

    [Theory]
    [InlineData("/panel", ".p-panel")]
    [InlineData("/splitter", ".p-splitter")]
    [InlineData("/scrollpanel", ".p-scrollpanel")]
    [InlineData("/knob", ".p-knob")]
    [InlineData("/image", ".p-image")]
    [InlineData("/imagecompare", ".p-imagecompare")]
    [InlineData("/galleria", ".p-galleria")]
    [InlineData("/carousel", ".p-carousel")]
    public async Task Paginas_renderizam_no_modo_escuro(string route, string selector)
    {
        var errors = new List<string>();
        var page = await NewPageAsync(errors, route);

        await page.EvaluateAsync("document.documentElement.classList.add('app-dark')");
        await Expect(page.Locator(selector).First).ToBeVisibleAsync();

        Assert.Empty(errors);
        await page.CloseAsync();
    }
}
