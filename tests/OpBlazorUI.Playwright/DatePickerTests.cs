using System.Text.RegularExpressions;
using Microsoft.Playwright;
using Xunit;
using static Microsoft.Playwright.Assertions;

namespace OpBlazorUI.Playwright;

/// <summary>
/// DatePicker: range, tokens do DateFormat, Time, validação no EditForm e formato pt-BR.
/// Usa as páginas <c>/_tests/date-picker</c> e <c>/_tests/date-picker-pt</c> do Showcase.
/// </summary>
[Collection("showcase")]
public class DatePickerTests
{
    private readonly ShowcaseFixture _fixture;

    public DatePickerTests(ShowcaseFixture fixture) => _fixture = fixture;

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
        await page.GotoAsync($"{_fixture.BaseUrl}/_tests/date-picker", new PageGotoOptions { WaitUntil = WaitUntilState.NetworkIdle });
        await page.Locator("#t-title").WaitForAsync();
        return page;
    }

    private static string DayOf(int day) => $"{DateTime.Today:yyyy-MM}-{day:00}";

    [Fact]
    public async Task Range_clear_dispara_o_callback()
    {
        var errors = new List<string>();
        var page = await NewPageAsync(errors);

        await page.Locator($"#t-range-clear .p-datepicker-day[data-date='{DayOf(5)}']").ClickAsync();
        await page.Locator($"#t-range-clear .p-datepicker-day[data-date='{DayOf(10)}']").ClickAsync();
        await Expect(page.Locator("#t-range-value")).ToContainTextAsync("|");

        await page.Locator(".p-datepicker-clear-button").ClickAsync();
        await Expect(page.Locator("#t-range-value")).ToHaveTextAsync("");

        Assert.True(errors.Count == 0, string.Join(" || ", errors));
        await page.CloseAsync();
    }

    [Fact]
    public async Task Digitacao_usa_o_DateFormat()
    {
        var errors = new List<string>();
        var page = await NewPageAsync(errors);

        var input = page.Locator("#t-typing-input");
        await input.FillAsync("15/01/2026");
        await page.Keyboard.PressAsync("Tab");
        await Expect(page.Locator("#t-typing-value")).ToHaveTextAsync("2026-01-15");

        // Entrada parcial não é aceita (mantém o valor anterior).
        await input.FillAsync("15/01");
        await page.Keyboard.PressAsync("Tab");
        await Expect(page.Locator("#t-typing-value")).ToHaveTextAsync("2026-01-15");

        Assert.True(errors.Count == 0, string.Join(" || ", errors));
        await page.CloseAsync();
    }

    [Fact]
    public async Task MinDate_muda_e_reconstroi_a_grade()
    {
        var errors = new List<string>();
        var page = await NewPageAsync(errors);

        var before = page.Locator($"#t-min .p-datepicker-day[data-date='{DayOf(10)}']");
        await Expect(before).ToHaveClassAsync(new Regex("p-disabled"));

        await page.Locator("#t-min-toggle").ClickAsync();
        await Expect(before).Not.ToHaveClassAsync(new Regex("p-disabled"));

        Assert.True(errors.Count == 0, string.Join(" || ", errors));
        await page.CloseAsync();
    }

    [Fact]
    public async Task Reabrir_volta_para_a_view_configurada()
    {
        var errors = new List<string>();
        var page = await NewPageAsync(errors);

        await page.Locator("#t-view-input").ClickAsync();
        await Expect(page.Locator("#t-view .p-datepicker-month-view")).ToBeVisibleAsync();

        // Vai para a view de ano e fecha.
        await page.Locator("#t-view .p-datepicker-select-year").ClickAsync();
        await Expect(page.Locator("#t-view .p-datepicker-year-view")).ToBeVisibleAsync();
        await page.Keyboard.PressAsync("Escape");

        // Reabrir volta para a view configurada (Month).
        await page.Locator("#t-view-input").ClickAsync();
        await Expect(page.Locator("#t-view .p-datepicker-month-view")).ToBeVisibleAsync();

        Assert.True(errors.Count == 0, string.Join(" || ", errors));
        await page.CloseAsync();
    }

    [Fact]
    public async Task Formato_com_nome_de_mes_nao_duplica()
    {
        var errors = new List<string>();
        var page = await NewPageAsync(errors);

        // "MMMM" (mês completo) não pode virar "MM"+"MM" -> "OctoberOctober".
        await Expect(page.Locator("#t-format-input")).ToHaveValueAsync("26 de October de 2026");

        Assert.True(errors.Count == 0, string.Join(" || ", errors));
        await page.CloseAsync();
    }

    [Fact]
    public async Task Tokens_de_ano_y_e_yy()
    {
        var errors = new List<string>();
        var page = await NewPageAsync(errors);

        await Expect(page.Locator("#t-token-y2")).ToHaveValueAsync("26");
        await Expect(page.Locator("#t-token-y4")).ToHaveValueAsync("2026");

        Assert.True(errors.Count == 0, string.Join(" || ", errors));
        await page.CloseAsync();
    }

    [Fact]
    public async Task Token_o_e_oo_dia_do_ano()
    {
        var errors = new List<string>();
        var page = await NewPageAsync(errors);

        // 05/01/2026 é o 5º dia do ano.
        await Expect(page.Locator("#t-token-o")).ToHaveValueAsync("5");
        await Expect(page.Locator("#t-token-oo")).ToHaveValueAsync("005");

        Assert.True(errors.Count == 0, string.Join(" || ", errors));
        await page.CloseAsync();
    }

    [Fact]
    public async Task Token_at_e_aspa_literal()
    {
        var errors = new List<string>();
        var page = await NewPageAsync(errors);

        var expected = new DateTimeOffset(new DateTime(2026, 10, 26)).ToUnixTimeMilliseconds().ToString();
        await Expect(page.Locator("#t-token-at")).ToHaveValueAsync(expected);
        await Expect(page.Locator("#t-token-quote")).ToHaveValueAsync("'");

        Assert.True(errors.Count == 0, string.Join(" || ", errors));
        await page.CloseAsync();
    }

    [Fact]
    public async Task Time_fecha_ao_escolher_a_data()
    {
        var errors = new List<string>();
        var page = await NewPageAsync(errors);

        await page.Locator("#t-time-input").ClickAsync();
        await Expect(page.Locator("#t-time-input-panel")).ToBeVisibleAsync();

        // HideOnDateTimeSelect é true por padrão (paridade): escolher a data fecha o painel.
        await page.Locator($"#t-time-input-panel .p-datepicker-day[data-date='{DayOf(5)}']").ClickAsync();
        await Expect(page.Locator("#t-time-input-panel")).ToBeHiddenAsync();

        Assert.True(errors.Count == 0, string.Join(" || ", errors));
        await page.CloseAsync();
    }

    [Fact]
    public async Task TimeOnly_mostra_e_aceita_a_hora_digitada()
    {
        var errors = new List<string>();
        var page = await NewPageAsync(errors);

        var input = page.Locator("#t-time-only-input");
        await input.FillAsync("14:30");
        await Expect(input).ToHaveValueAsync("14:30");
        await Expect(page.Locator("#t-time-only-value")).ToHaveTextAsync("14:30:00");

        // Entrada inválida mantém o valor anterior.
        await input.FillAsync("25:99");
        await Expect(page.Locator("#t-time-only-value")).ToHaveTextAsync("14:30:00");

        Assert.True(errors.Count == 0, string.Join(" || ", errors));
        await page.CloseAsync();
    }

    [Fact]
    public async Task ShowTime_texto_traz_data_e_hora_e_nao_sobrescreve_o_digitado()
    {
        var errors = new List<string>();
        var page = await NewPageAsync(errors);

        var input = page.Locator("#t-time-24-input");
        await input.FillAsync("15/10/2026 14:30");
        await Expect(input).ToHaveValueAsync("15/10/2026 14:30");
        await Expect(page.Locator("#t-time-24-value")).ToHaveTextAsync("2026-10-15 14:30");

        // A hora digitada vale no painel e escolher outro dia a mantém (e não 00:00).
        await input.ClickAsync();
        await Expect(page.Locator("#t-time-24-input-panel")).ToBeVisibleAsync();
        await Expect(page.Locator("#t-time-24-input-panel .p-datepicker-hour-picker span").First).ToHaveTextAsync("14");
        await page.Locator("#t-time-24-input-panel .p-datepicker-day[data-date='2026-10-20']").ClickAsync();
        await Expect(page.Locator("#t-time-24-value")).ToHaveTextAsync("2026-10-20 14:30");
        await Expect(input).ToHaveValueAsync("20/10/2026 14:30");

        Assert.True(errors.Count == 0, string.Join(" || ", errors));
        await page.CloseAsync();
    }

    [Fact]
    public async Task Time_12h_formata_e_interpreta_ampm()
    {
        var errors = new List<string>();
        var page = await NewPageAsync(errors);

        var input = page.Locator("#t-time-12-input");
        await input.FillAsync("15/10/2026 02:30 PM");
        await Expect(input).ToHaveValueAsync("15/10/2026 02:30 PM");
        await Expect(page.Locator("#t-time-12-value")).ToHaveTextAsync("2026-10-15 14:30");

        // Em 12h o AM/PM é obrigatório (como no upstream).
        await input.FillAsync("15/10/2026 02:30");
        await Expect(page.Locator("#t-time-12-value")).ToHaveTextAsync("2026-10-15 14:30");

        // TimeOnly em 12h: só a hora com o sufixo.
        var only = page.Locator("#t-time-12-only-input");
        await only.FillAsync("12:30 AM");
        await Expect(page.Locator("#t-time-12-only-value")).ToHaveTextAsync("00:30");

        Assert.True(errors.Count == 0, string.Join(" || ", errors));
        await page.CloseAsync();
    }

    [Fact]
    public async Task ShowSeconds_inclui_os_segundos_no_texto_e_no_parse()
    {
        var errors = new List<string>();
        var page = await NewPageAsync(errors);

        var input = page.Locator("#t-time-seconds-input");
        await input.FillAsync("15/10/2026 14:30:45");
        await Expect(input).ToHaveValueAsync("15/10/2026 14:30:45");
        await Expect(page.Locator("#t-time-seconds-value")).ToHaveTextAsync("2026-10-15 14:30:45");

        Assert.True(errors.Count == 0, string.Join(" || ", errors));
        await page.CloseAsync();
    }

    [Fact]
    public async Task Range_no_EditForm_valida_o_campo()
    {
        var errors = new List<string>();
        var page = await NewPageAsync(errors);

        // Sem valor, o submit é inválido e a mensagem aparece.
        await page.Locator("#t-form-submit").ClickAsync();
        await Expect(page.Locator("#t-form-msg")).ToContainTextAsync("Informe o período.");
        await Expect(page.Locator("#t-form-submitted")).ToHaveTextAsync("False");

        // Escolher as duas pontas limpa a validação e permite o submit.
        await page.Locator($"#t-form .p-datepicker-day[data-date='{DayOf(5)}']").ClickAsync();
        await page.Locator($"#t-form .p-datepicker-day[data-date='{DayOf(10)}']").ClickAsync();
        await page.Locator("#t-form-submit").ClickAsync();
        await Expect(page.Locator("#t-form-submitted")).ToHaveTextAsync("True");

        Assert.True(errors.Count == 0, string.Join(" || ", errors));
        await page.CloseAsync();
    }

    [Fact]
    public async Task Pagina_de_doc_carrega_sem_erros()
    {
        var errors = new List<string>();
        var page = await _fixture.Browser.NewPageAsync();
        page.SetDefaultTimeout(60000);
        page.Console += (_, msg) =>
        {
            if (msg.Type == "error")
            {
                errors.Add(msg.Text);
            }
        };

        await page.GotoAsync($"{_fixture.BaseUrl}/datepicker", new PageGotoOptions { WaitUntil = WaitUntilState.NetworkIdle });

        // A seção Formulários deve renderizar os três campos (Single/Range/Multiple).
        await Expect(page.Locator("#form-date")).ToBeVisibleAsync();
        await Expect(page.Locator("#form-range")).ToBeVisibleAsync();
        await Expect(page.Locator("#form-multiple")).ToBeVisibleAsync();

        Assert.True(errors.Count == 0, string.Join(" || ", errors));
        await page.CloseAsync();
    }

    [Fact]
    public async Task Traducao_pt_br_nos_nomes_e_rotulos()
    {
        var errors = new List<string>();
        var page = await _fixture.Browser.NewPageAsync();
        page.SetDefaultTimeout(60000);
        page.Console += (_, msg) =>
        {
            if (msg.Type == "error")
            {
                errors.Add(msg.Text);
            }
        };

        await page.GotoAsync($"{_fixture.BaseUrl}/_tests/date-picker-pt", new PageGotoOptions { WaitUntil = WaitUntilState.NetworkIdle });
        await page.Locator("#t-title").WaitForAsync();

        await Expect(page.Locator("#t-pt-format-input")).ToHaveValueAsync("26 de outubro de 2026");
        await Expect(page.Locator("#t-pt-labels .p-datepicker-today-button")).ToContainTextAsync("Hoje");
        await Expect(page.Locator("#t-pt-labels .p-datepicker-clear-button")).ToContainTextAsync("Limpar");

        Assert.True(errors.Count == 0, string.Join(" || ", errors));
        await page.CloseAsync();
    }
}
