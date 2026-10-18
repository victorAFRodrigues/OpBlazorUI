using Microsoft.Playwright;
using Xunit;
using static Microsoft.Playwright.Assertions;

namespace OpBlazorUI.Playwright;

/// <summary>
/// Páginas de dados do Showcase (Tree, TreeTable, DataView, OrderList, PickList,
/// ConfirmPopup, InlineMessage, MeterGroup e Timeline) renderizam e respondem.
/// </summary>
[Collection("showcase")]
public class PaginasDadosTests
{
    private readonly ShowcaseFixture _fixture;

    public PaginasDadosTests(ShowcaseFixture fixture) => _fixture = fixture;

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
    public async Task Tree_expande_e_seleciona()
    {
        var errors = new List<string>();
        var page = await NewPageAsync(errors, "/tree");

        var firstTree = page.Locator(".p-tree").First;
        await firstTree.WaitForAsync();
        await firstTree.Locator(".p-tree-node-toggle-button").First.ClickAsync();
        await Expect(firstTree.Locator(".p-tree-node-children").First).ToBeVisibleAsync();

        await firstTree.Locator(".p-tree-node-content").First.ClickAsync();
        await Expect(firstTree.Locator(".p-tree-node-selected").First).ToBeVisibleAsync();

        Assert.Empty(errors);
        await page.CloseAsync();
    }

    [Fact]
    public async Task TreeTable_expande_linha()
    {
        var errors = new List<string>();
        var page = await NewPageAsync(errors, "/treetable");

        var table = page.Locator(".p-treetable").First;
        await table.WaitForAsync();
        var rows = table.Locator(".p-treetable-tbody .p-treetable-row");
        var before = await rows.CountAsync();

        await table.Locator(".p-treetable-node-toggle-button").First.ClickAsync();
        await Task.Delay(200);

        Assert.NotEqual(before, await rows.CountAsync());

        Assert.Empty(errors);
        await page.CloseAsync();
    }

    [Fact]
    public async Task DataView_renderiza_conteudo()
    {
        var errors = new List<string>();
        var page = await NewPageAsync(errors, "/dataview");

        await Expect(page.Locator(".p-dataview").First).ToBeVisibleAsync();
        await Expect(page.Locator(".p-dataview-content").First).ToBeVisibleAsync();

        Assert.Empty(errors);
        await page.CloseAsync();
    }

    [Fact]
    public async Task OrderList_move_item_selecionado()
    {
        var errors = new List<string>();
        var page = await NewPageAsync(errors, "/orderlist");

        var root = page.Locator(".p-orderlist").First;
        await root.WaitForAsync();
        await Expect(root.Locator(".p-orderlist-controls")).ToBeVisibleAsync();

        var firstOption = root.Locator(".p-listbox-option").Nth(1);
        var label = (await firstOption.InnerTextAsync()).Trim();
        await firstOption.ClickAsync();
        await Task.Delay(250);

        await root.Locator(".p-orderlist-controls button").First.ClickAsync();
        await Task.Delay(250);

        var firstLabelAfter = (await root.Locator(".p-listbox-option").First.InnerTextAsync()).Trim();
        Assert.True(firstLabelAfter == label, "O item selecionado deveria ir para o topo.");

        Assert.Empty(errors);
        await page.CloseAsync();
    }

    [Fact]
    public async Task PickList_transfere_item()
    {
        var errors = new List<string>();
        var page = await NewPageAsync(errors, "/picklist");

        var root = page.Locator(".p-picklist").First;
        await root.WaitForAsync();
        Assert.True(await root.Locator(".p-picklist-list-container").CountAsync() >= 2);

        var sourceCountBefore = await root.Locator(".p-picklist-list-container").First.Locator(".p-listbox-option").CountAsync();
        await root.Locator(".p-picklist-list-container").First.Locator(".p-listbox-option").First.ClickAsync();
        // Grupo de controles de transferência fica entre as duas listas (nth(1)).
        await root.Locator(".p-picklist-controls").Nth(1).Locator("button").First.ClickAsync();
        await Task.Delay(150);

        Assert.True(await root.Locator(".p-picklist-list-container").First.Locator(".p-listbox-option").CountAsync() < sourceCountBefore);

        Assert.Empty(errors);
        await page.CloseAsync();
    }

    [Fact]
    public async Task ConfirmPopup_abre_ancorado()
    {
        var errors = new List<string>();
        var page = await NewPageAsync(errors, "/confirmpopup");

        var trigger = page.Locator("button:has-text('Salvar')").First;
        await trigger.WaitForAsync();
        await trigger.ClickAsync();

        await Expect(page.Locator(".p-confirmpopup").First).ToBeVisibleAsync();

        Assert.Empty(errors);
        await page.CloseAsync();
    }

    [Fact]
    public async Task ConfirmPopup_reject_e_secundario_e_outlined()
    {
        var errors = new List<string>();
        var page = await NewPageAsync(errors, "/confirmpopup");

        var trigger = page.Locator("button:has-text('Salvar')").First;
        await trigger.WaitForAsync();
        await trigger.ClickAsync();

        var popup = page.Locator(".p-confirmpopup").First;
        await Expect(popup).ToBeVisibleAsync();

        // Paridade com o upstream: o rejeitar é secundário e outlined, não primário preenchido.
        var reject = popup.Locator("[data-pc-focus='reject']").First;
        await Expect(reject).ToHaveClassAsync(new System.Text.RegularExpressions.Regex("p-button-secondary"));
        await Expect(reject).ToHaveClassAsync(new System.Text.RegularExpressions.Regex("p-button-outlined"));

        // O popup força size small nos dois botões, como no upstream.
        await Expect(reject).ToHaveClassAsync(new System.Text.RegularExpressions.Regex("p-button-sm"));

        // O aceitar continua primário (sem variante secundária/outlined), mas também small.
        var accept = popup.Locator("[data-pc-focus='accept']").First;
        await Expect(accept).ToHaveClassAsync(new System.Text.RegularExpressions.Regex("p-button-sm"));
        await Expect(accept).Not.ToHaveClassAsync(new System.Text.RegularExpressions.Regex("p-button-outlined"));
        await Expect(accept).Not.ToHaveClassAsync(new System.Text.RegularExpressions.Regex("p-button-secondary"));

        Assert.Empty(errors);
        await page.CloseAsync();
    }

    [Fact]
    public async Task ConfirmPopup_events_usa_reject_secundario_e_outlined()
    {
        var errors = new List<string>();
        var page = await NewPageAsync(errors, "/confirmpopup");

        var trigger = page.Locator("button:has-text('Confirmar ação')").First;
        await trigger.WaitForAsync();
        await trigger.ClickAsync();

        var popup = page.Locator(".p-confirmpopup").First;
        await Expect(popup).ToBeVisibleAsync();

        // O demo de Eventos também declara RejectButtonProps (secondary + outlined).
        var reject = popup.Locator("[data-pc-focus='reject']").First;
        await Expect(reject).ToHaveClassAsync(new System.Text.RegularExpressions.Regex("p-button-secondary"));
        await Expect(reject).ToHaveClassAsync(new System.Text.RegularExpressions.Regex("p-button-outlined"));
        await Expect(reject).ToHaveClassAsync(new System.Text.RegularExpressions.Regex("p-button-sm"));

        var accept = popup.Locator("[data-pc-focus='accept']").First;
        await Expect(accept).ToHaveClassAsync(new System.Text.RegularExpressions.Regex("p-button-sm"));
        await Expect(accept).Not.ToHaveClassAsync(new System.Text.RegularExpressions.Regex("p-button-outlined"));

        Assert.Empty(errors);
        await page.CloseAsync();
    }

    [Fact]
    public async Task InlineMessage_renderiza_severidades()
    {
        var errors = new List<string>();
        var page = await NewPageAsync(errors, "/inlinemessage");

        await Expect(page.Locator(".p-inlinemessage").First).ToBeVisibleAsync();
        await Expect(page.Locator(".p-inlinemessage-info").First).ToBeVisibleAsync();
        await Expect(page.Locator(".p-inlinemessage-error").First).ToBeVisibleAsync();

        Assert.Empty(errors);
        await page.CloseAsync();
    }

    [Fact]
    public async Task MeterGroup_renderiza_medidores_e_rotulos()
    {
        var errors = new List<string>();
        var page = await NewPageAsync(errors, "/metergroup");

        await Expect(page.Locator(".p-metergroup").First).ToBeVisibleAsync();
        Assert.True(await page.Locator(".p-metergroup-meter").CountAsync() >= 1);
        Assert.True(await page.Locator(".p-metergroup-label").CountAsync() >= 1);

        Assert.Empty(errors);
        await page.CloseAsync();
    }

    [Fact]
    public async Task ConfirmPopup_fecha_ao_clicar_fora_por_padrao()
    {
        var errors = new List<string>();
        var page = await NewPageAsync(errors, "/confirmpopup");

        await page.Locator("button:has-text('Salvar')").First.ClickAsync();
        var popup = page.Locator(".p-confirmpopup").First;
        await Expect(popup).ToBeVisibleAsync();

        // DismissableMask é true por padrão (dismissableMask !== false no upstream).
        await page.Locator("h1").First.ClickAsync();
        await Expect(popup).ToHaveCountAsync(0);

        Assert.Empty(errors);
        await page.CloseAsync();
    }

    [Fact]
    public async Task ConfirmPopup_seta_recuada_e_flip_acima()
    {
        var errors = new List<string>();
        var page = await NewPageAsync(errors, "/confirmpopup");

        await page.Locator("button:has-text('Salvar')").First.ClickAsync();
        var popup = page.Locator(".p-confirmpopup").First;
        await Expect(popup).ToBeVisibleAsync();

        // Paridade com o upstream (alignArrow): alinhado ao alvo, a variável fica em 0px e a ponta
        // da seta para a 1.25rem (--p-confirmpopup-arrow-offset) da borda esquerda, não na quina.
        var arrow = await popup.EvaluateAsync<double[]>(
            "el => { const before = getComputedStyle(el, '::before');" +
            " const rem = parseFloat(getComputedStyle(document.documentElement).fontSize) * 1.25;" +
            " return [parseFloat(before.left), rem," +
            " el.style.getPropertyValue('--p-confirmpopup-arrow-left') === '0px' ? 1 : 0]; }");
        Assert.Equal(1, arrow[2]);
        Assert.InRange(arrow[0], arrow[1] - 0.5, arrow[1] + 0.5);

        // Abrindo acima, a seta troca de lado (classe estilizada pelo tema + atributo do upstream)
        // e a margem inverte, mantendo o mesmo gap.
        await page.Keyboard.PressAsync("Escape");
        await Expect(popup).ToHaveCountAsync(0);
        await page.SetViewportSizeAsync(1280, 320);
        var trigger = page.Locator("button:has-text('Salvar')").First;
        await trigger.EvaluateAsync("el => el.scrollIntoView({ block: 'end' })");
        await trigger.ClickAsync();
        await Expect(popup).ToBeVisibleAsync();
        await Expect(popup).ToHaveClassAsync(new System.Text.RegularExpressions.Regex("p-confirmpopup-flipped"));
        await Expect(popup).ToHaveAttributeAsync("data-p-confirmpopup-flipped", "true");

        var flip = await popup.EvaluateAsync<double[]>(
            "el => [parseFloat(getComputedStyle(el, '::before').top), el.offsetHeight]");
        Assert.True(flip[0] >= flip[1] - 4, $"a seta deveria estar embaixo; top={flip[0]} altura={flip[1]}");

        Assert.Empty(errors);
        await page.CloseAsync();
    }

    [Fact]
    public async Task Timeline_renderiza_eventos()
    {
        var errors = new List<string>();
        var page = await NewPageAsync(errors, "/timeline");

        await Expect(page.Locator(".p-timeline-event").First).ToBeVisibleAsync();
        Assert.True(await page.Locator(".p-timeline-event").CountAsync() >= 2);

        Assert.Empty(errors);
        await page.CloseAsync();
    }

    [Fact]
    public async Task ConfirmPopup_abre_sem_rolar_a_pagina()
    {
        var errors = new List<string>();
        var page = await NewPageAsync(errors, "/confirmpopup");

        // Scroll manual instantâneo até a seção de Eventos (o CSS pode ter scroll suave).
        await page.EvaluateAsync(
            "() => document.getElementById('events').scrollIntoView({ behavior: 'instant', block: 'center' })");
        await page.WaitForTimeoutAsync(100);
        var before = await page.EvaluateAsync<double>("() => Math.round(window.scrollY)");
        Assert.True(before > 200, $"a página deveria estar rolada; scrollY={before}");

        // O foco inicial não pode rolar a página para o topo: no instante do focus o painel
        // ainda está em position:absolute/top:0, e o navegador rolava até ele.
        await page.Locator("button:has-text('Confirmar ação')").First.ClickAsync();
        await Expect(page.Locator(".p-confirmpopup").First).ToBeVisibleAsync();

        var after = await page.EvaluateAsync<double>("() => Math.round(window.scrollY)");
        Assert.Equal(before, after);

        Assert.Empty(errors);
        await page.CloseAsync();
    }
}
