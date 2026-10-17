using System.Globalization;
using Microsoft.Playwright;
using OpBlazorUI.Services;
using Xunit;
using static Microsoft.Playwright.Assertions;

namespace OpBlazorUI.Playwright;

/// <summary>
/// Serviço de filtro (<see cref="OpFilterService"/>): modos de match, datas e registros
/// customizados nos testes unitários, e o demo da página <c>/filterservice</c> com troca de modo.
/// </summary>
[Collection("showcase")]
public class FilterServiceTests
{
    private sealed record Row(string Name, int Age, DateTime Born, DateOnly Day);

    private static readonly List<Row> Rows = new()
    {
        new Row("Ana", 30, new DateTime(1994, 5, 10), new DateOnly(2020, 1, 10)),
        new Row("Bia", 25, new DateTime(1999, 8, 20), new DateOnly(2021, 2, 20)),
        new Row("Cecília", 40, new DateTime(1984, 3, 1), new DateOnly(2022, 3, 1))
    };

    private readonly ShowcaseFixture _fixture;
    private readonly OpFilterService _service = new();

    public FilterServiceTests(ShowcaseFixture fixture) => _fixture = fixture;

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
        return page;
    }

    [Fact]
    public async Task FilterService_filtra_conforme_o_match_mode()
    {
        var errors = new List<string>();
        var page = await NewPageAsync(errors);

        await page.GotoAsync($"{_fixture.BaseUrl}/filterservice",
            new PageGotoOptions { WaitUntil = WaitUntilState.NetworkIdle });

        var rows = page.Locator("table tbody tr");
        var input = page.Locator("input[placeholder='Filtrar...']");
        var mode = page.Locator(".card select").First;
        await input.WaitForAsync();

        // startsWith (padrão): "an" não inicia nenhum nome/categoria.
        await input.FillAsync("an");
        await Expect(rows).ToHaveCountAsync(0);

        // contains: Banana e Orange.
        await mode.SelectOptionAsync("contains");
        await Expect(rows).ToHaveCountAsync(2);

        // endsWith: "an" não termina nenhum nome/categoria.
        await mode.SelectOptionAsync("endsWith");
        await Expect(rows).ToHaveCountAsync(0);

        // endsWith: "na" termina Banana.
        await input.FillAsync("na");
        await Expect(rows).ToHaveCountAsync(1);

        Assert.Empty(errors);
        await page.CloseAsync();
    }

    [Fact]
    public void Filtro_null_casa_tudo()
    {
        var result = _service.Filter(Rows, new[] { "Age" }, null, OpFilterMatchMode.Gt);
        Assert.Equal(Rows.Count, result.Count);
    }

    [Fact]
    public void Modos_de_data_aceitam_DateOnly()
    {
        var result = _service.Filter(Rows, new[] { "Day" }, new DateOnly(2021, 2, 20), OpFilterMatchMode.DateIs);
        Assert.Single(result);
        Assert.Equal("Bia", result[0].Name);
    }

    [Fact]
    public void Modos_de_data_aceitam_DateTime()
    {
        var result = _service.Filter(Rows, new[] { "Born" }, new DateTime(1994, 5, 10), OpFilterMatchMode.DateBefore);
        Assert.Single(result);
        Assert.Equal("Cecília", result[0].Name);
    }

    [Fact]
    public void Modo_custom_nao_registrado_nao_esvazia()
    {
        var result = _service.Filter(Rows, new[] { "Name" }, "x", "modoInexistente");
        Assert.Equal(Rows.Count, result.Count);
    }

    [Fact]
    public void Modo_custom_registrado_funciona()
    {
        _service.Register("idadePar", (value, filter, culture) => value is int i && i % 2 == 0);
        var result = _service.Filter(Rows, new[] { "Age" }, "x", "idadePar");
        Assert.Equal(2, result.Count);
    }

    [Fact]
    public void Contem_ignora_acentos_e_caixa()
    {
        var result = _service.Filter(Rows, new[] { "Name" }, "CECILIA", OpFilterMatchMode.Contains);
        Assert.Single(result);
    }

    [Fact]
    public void Numero_em_texto_usa_a_cultura()
    {
        var value = new List<double> { 1.5, 10.25 };
        var result = _service.Filter(value.Select(v => new { V = v }), new[] { "V" }, "1,5",
            OpFilterMatchMode.Equals, CultureInfo.GetCultureInfo("pt-BR"));
        Assert.Single(result);
    }
}
