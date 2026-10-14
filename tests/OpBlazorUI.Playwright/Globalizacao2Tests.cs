using Microsoft.Playwright;
using Xunit;
using static Microsoft.Playwright.Assertions;

namespace OpBlazorUI.Playwright;

/// <summary>
/// Lote 2 da globalização (seletores). Usa a página <c>/_tests/globalizacao-2</c>, que fixa a
/// tradução em pt-BR, e verifica que mensagens de vazio, rótulos de acessibilidade e o
/// comparador ordinal vêm da tradução global.
/// </summary>
[Collection("showcase")]
public class Globalizacao2Tests
{
    private readonly ShowcaseFixture _fixture;

    public Globalizacao2Tests(ShowcaseFixture fixture) => _fixture = fixture;

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
        await page.GotoAsync($"{_fixture.BaseUrl}/_tests/globalizacao-2", new PageGotoOptions { WaitUntil = WaitUntilState.NetworkIdle });
        await page.Locator("#t-title").WaitForAsync();
        return page;
    }

    [Fact]
    public async Task Select_usa_a_traducao_pt_br()
    {
        var errors = new List<string>();
        var page = await NewPageAsync(errors);

        await Expect(page.Locator("#t-select .p-select-dropdown")).ToHaveAttributeAsync("aria-label", "Mostrar opções");

        await page.Locator("#t-select .p-select-dropdown").ClickAsync();
        await Expect(page.Locator("#t-select .p-select-overlay")).ToBeVisibleAsync();

        await Expect(page.Locator("#t-select ul[role=listbox]")).ToHaveAttributeAsync("aria-label", "Lista de opções");
        await Expect(page.Locator("#t-select .p-select-filter")).ToHaveAttributeAsync("placeholder", "Filtrar");
        await Expect(page.Locator("#t-select .p-select-filter")).ToHaveAttributeAsync("aria-label", "Filtrar");
        await Expect(page.Locator("#t-select .p-select-empty-message")).ToHaveTextAsync("Nenhum resultado encontrado");

        Assert.True(errors.Count == 0, string.Join(" || ", errors));
        await page.CloseAsync();
    }

    [Fact]
    public async Task MultiSelect_usa_a_traducao_pt_br()
    {
        var errors = new List<string>();
        var page = await NewPageAsync(errors);

        await Expect(page.Locator("#t-multiselect .p-multiselect-dropdown")).ToHaveAttributeAsync("aria-label", "Mostrar opções");

        await page.Locator("#t-multiselect .p-multiselect-dropdown").ClickAsync();
        await Expect(page.Locator("#t-multiselect .p-multiselect-overlay")).ToBeVisibleAsync();

        await Expect(page.Locator("#t-multiselect ul[role=listbox]")).ToHaveAttributeAsync("aria-label", "Lista de opções");
        await Expect(page.Locator("#t-multiselect .p-multiselect-filter")).ToHaveAttributeAsync("placeholder", "Filtrar");

        var checkbox = page.Locator("#t-multiselect .p-multiselect-header input[type=checkbox]");
        await Expect(checkbox).ToHaveAttributeAsync("aria-label", "Todos os itens selecionados");
        await checkbox.ClickAsync();
        await Expect(checkbox).ToHaveAttributeAsync("aria-label", "Todos os itens desmarcados");

        Assert.True(errors.Count == 0, string.Join(" || ", errors));
        await page.CloseAsync();
    }

    [Fact]
    public async Task Listbox_e_TreeSelect_usam_a_traducao_pt_br()
    {
        var errors = new List<string>();
        var page = await NewPageAsync(errors);

        await Expect(page.Locator("#t-listbox .p-listbox-filter")).ToHaveAttributeAsync("placeholder", "Filtrar");
        await Expect(page.Locator("#t-listbox .p-listbox-empty-message")).ToHaveTextAsync("Nenhum resultado encontrado");

        await page.Locator("#t-treeselect .p-treeselect-dropdown").ClickAsync();
        await Expect(page.Locator("#t-treeselect .p-treeselect-overlay")).ToBeVisibleAsync();
        await Expect(page.Locator("#t-treeselect .p-treeselect-filter")).ToHaveAttributeAsync("placeholder", "Filtrar");
        await Expect(page.Locator("#t-treeselect .p-treeselect-empty-message")).ToHaveTextAsync("Nenhum resultado encontrado");

        Assert.True(errors.Count == 0, string.Join(" || ", errors));
        await page.CloseAsync();
    }

    [Fact]
    public async Task Filtro_dos_seletores_ignora_acentos_em_ordinal()
    {
        var errors = new List<string>();
        var page = await NewPageAsync(errors);

        await page.Locator("#t-multiselect .p-multiselect-dropdown").ClickAsync();
        await Expect(page.Locator("#t-multiselect .p-multiselect-overlay")).ToBeVisibleAsync();

        // "sao" (sem acento e minúsculo) casa com "São Paulo" via comparador ordinal.
        await page.Locator("#t-multiselect .p-multiselect-filter").FillAsync("sao");

        var options = page.Locator("#t-multiselect ul[role=listbox] li[role=option]");
        await Expect(options).ToHaveCountAsync(1);
        await Expect(options.First).ToContainTextAsync("São Paulo");

        Assert.True(errors.Count == 0, string.Join(" || ", errors));
        await page.CloseAsync();
    }

    [Fact]
    public async Task CascadeSelect_e_AutoComplete_usam_a_traducao_pt_br()
    {
        var errors = new List<string>();
        var page = await NewPageAsync(errors);

        await Expect(page.Locator("#t-cascadeselect .p-cascadeselect-dropdown")).ToHaveAttributeAsync("aria-label", "Mostrar opções");
        await Expect(page.Locator("#t-autocomplete .p-autocomplete-dropdown")).ToHaveAttributeAsync("aria-label", "Mostrar opções");

        Assert.True(errors.Count == 0, string.Join(" || ", errors));
        await page.CloseAsync();
    }
}
