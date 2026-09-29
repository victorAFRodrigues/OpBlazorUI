using System.Globalization;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;

namespace OpBlazorUI.Base.Components.InputNumber;

public partial class OpInputNumber : ComponentBase
{
    [Parameter] public decimal? Value { get; set; }
    [Parameter] public EventCallback<decimal?> ValueChanged { get; set; }
    [Parameter] public decimal? Min { get; set; }
    [Parameter] public decimal? Max { get; set; }
    [Parameter] public decimal Step { get; set; } = 1;
    [Parameter] public string Mode { get; set; } = "decimal";
    [Parameter] public string? Currency { get; set; }
    [Parameter] public string CurrencyDisplay { get; set; } = "symbol";
    [Parameter] public string? Locale { get; set; }
    [Parameter] public bool UseGrouping { get; set; } = true;
    [Parameter] public int? MinFractionDigits { get; set; }
    [Parameter] public int? MaxFractionDigits { get; set; }
    [Parameter] public string? Prefix { get; set; }
    [Parameter] public string? Suffix { get; set; }
    [Parameter] public bool ShowButtons { get; set; }
    [Parameter] public string ButtonLayout { get; set; } = "stacked";
    [Parameter] public bool ShowClear { get; set; }
    [Parameter] public string? IncrementButtonIcon { get; set; } = "pi pi-angle-up";
    [Parameter] public string? DecrementButtonIcon { get; set; } = "pi pi-angle-down";
    [Parameter] public bool Invalid { get; set; }
    [Parameter] public bool Disabled { get; set; }
    [Parameter] public bool Readonly { get; set; }
    [Parameter] public string Variant { get; set; } = "outlined";
    [Parameter] public string? Size { get; set; }
    [Parameter] public bool Fluid { get; set; }
    [Parameter] public string? Id { get; set; }
    [Parameter] public string? Placeholder { get; set; }
    [Parameter] public string? AriaLabel { get; set; }
    [Parameter] public string? Autocomplete { get; set; }
    [Parameter] public string? StyleClass { get; set; }
    [Parameter] public string? InputClass { get; set; }

    [Parameter] public EventCallback<decimal?> OnInput { get; set; }
    [Parameter] public EventCallback<decimal?> OnChange { get; set; }
    [Parameter] public EventCallback<FocusEventArgs> OnFocus { get; set; }
    [Parameter] public EventCallback<FocusEventArgs> OnBlur { get; set; }
    [Parameter] public EventCallback<KeyboardEventArgs> OnKeyDown { get; set; }
    [Parameter] public EventCallback OnClear { get; set; }

    [Parameter(CaptureUnmatchedValues = true)]
    public Dictionary<string, object>? AdditionalAttributes { get; set; }

    private static readonly Dictionary<string, string> CurrencySymbols = new(StringComparer.OrdinalIgnoreCase)
    {
        ["USD"] = "$", ["EUR"] = "€", ["INR"] = "₹", ["JPY"] = "¥",
        ["BRL"] = "R$", ["GBP"] = "£", ["CNY"] = "¥", ["CAD"] = "C$", ["AUD"] = "A$"
    };

    // Classes de inputnumberstyle.ts: layout só com botões; variante e p-filled vão no input.
    private string RootClass => BuildClass(
        "p-inputnumber p-component p-inputwrapper",
        Value is not null ? "p-inputwrapper-filled" : null,
        _focused ? "p-inputwrapper-focus" : null,
        ShowButtons && ButtonLayout == "stacked" ? "p-inputnumber-stacked" : null,
        ShowButtons && ButtonLayout == "horizontal" ? "p-inputnumber-horizontal" : null,
        ShowButtons && ButtonLayout == "vertical" ? "p-inputnumber-vertical" : null,
        Fluid ? "p-inputnumber-fluid" : null,
        Invalid ? "p-invalid" : null,
        StyleClass);

    private string InputClassValue => BuildClass(
        "p-inputnumber-input p-inputtext p-component",
        Value is not null ? "p-filled" : null,
        Variant == "filled" ? "p-variant-filled" : null,
        Fluid ? "p-inputtext-fluid" : null,
        Size == "small" ? "p-inputtext-sm" : null,
        Size == "large" ? "p-inputtext-lg" : null,
        Invalid ? "p-invalid" : null,
        InputClass);

    private CultureInfo GetCulture()
    {
        if (!string.IsNullOrEmpty(Locale))
        {
            try { return CultureInfo.GetCultureInfo(Locale); }
            catch (CultureNotFoundException) { }
        }

        return CultureInfo.CurrentCulture;
    }

    private NumberFormatInfo GetNumberFormat(CultureInfo culture)
    {
        var nfi = (NumberFormatInfo)culture.NumberFormat.Clone();
        if (!UseGrouping)
        {
            nfi.NumberGroupSeparator = string.Empty;
            nfi.CurrencyGroupSeparator = string.Empty;
            nfi.NumberGroupSizes = [0];
            nfi.CurrencyGroupSizes = [0];
        }

        return nfi;
    }

    private string FormatValue(decimal? value)
    {
        if (value is null) return string.Empty;
        var v = value.Value;
        var culture = GetCulture();
        var nfi = GetNumberFormat(culture);

        if (Mode == "currency")
        {
            if (MinFractionDigits is int mn) nfi.CurrencyDecimalDigits = mn;
            if (!string.IsNullOrEmpty(Currency))
            {
                nfi.CurrencySymbol = CurrencyDisplay == "code"
                    ? Currency + " "
                    : GetCurrencySymbol(Currency);
            }

            return v.ToString("C", nfi);
        }

        var fractionDigits = MaxFractionDigits ?? MinFractionDigits;
        if (fractionDigits is int digits)
        {
            nfi.NumberDecimalDigits = digits;
            return v.ToString("N", nfi);
        }

        return v.ToString(UseGrouping ? "#,##0.#############################" : "0.#############################", nfi);
    }

    private static string GetCurrencySymbol(string currency) =>
        CurrencySymbols.TryGetValue(currency, out var symbol) ? symbol : currency;

    private bool TryParseNumber(string text, out decimal value)
    {
        var culture = GetCulture();
        var nfi = GetNumberFormat(culture);
        var styles = NumberStyles.Number;
        if (Mode == "currency") styles |= NumberStyles.AllowCurrencySymbol;

        if (decimal.TryParse(text, styles, nfi, out value)) return true;
        return false;
    }

    private decimal Clamp(decimal value)
    {
        if (Min is decimal mn && value < mn) return mn;
        if (Max is decimal mx && value > mx) return mx;
        return value;
    }

    private async Task SetValueAsync(decimal? value, bool raiseInput)
    {
        Value = value;
        await ValueChanged.InvokeAsync(value);
        if (raiseInput) await OnInput.InvokeAsync(value);
    }

    private async Task HandleInput(ChangeEventArgs e)
    {
        if (Readonly || Disabled) return;
        var text = e.Value?.ToString() ?? string.Empty;

        if (string.IsNullOrWhiteSpace(text))
        {
            await SetValueAsync(null, true);
            return;
        }

        // Min/Max só são aplicados no change (blur/Enter): limitar a cada tecla impede digitar
        // valores de vários dígitos (ex.: Min=10 transformaria o "5" de "50" em "10").
        if (TryParseNumber(text, out var parsed))
        {
            await SetValueAsync(parsed, true);
        }
    }

    private async Task HandleChange(ChangeEventArgs e)
    {
        if (Readonly || Disabled) return;
        var text = e.Value?.ToString() ?? string.Empty;
        if (string.IsNullOrWhiteSpace(text))
        {
            await OnChange.InvokeAsync(null);
            return;
        }

        if (TryParseNumber(text, out var parsed))
        {
            var clamped = Clamp(parsed);
            Value = clamped;
            await ValueChanged.InvokeAsync(clamped);
            await OnChange.InvokeAsync(clamped);
        }
    }

    private bool _focused;

    private async Task HandleFocus(FocusEventArgs e)
    {
        _focused = true;
        await OnFocus.InvokeAsync(e);
    }

    private async Task HandleBlur(FocusEventArgs e)
    {
        _focused = false;
        await OnBlur.InvokeAsync(e);
    }

    private async Task HandleKeyDown(KeyboardEventArgs e)
    {
        if (Readonly || Disabled)
        {
            await OnKeyDown.InvokeAsync(e);
            return;
        }

        if (e.Key == "ArrowUp")
        {
            await StepAsync(1);
            return;
        }

        if (e.Key == "ArrowDown")
        {
            await StepAsync(-1);
            return;
        }

        await OnKeyDown.InvokeAsync(e);
    }

    private async Task Increment() => await StepAsync(1);

    private async Task Decrement() => await StepAsync(-1);

    private async Task StepAsync(int direction)
    {
        if (Readonly || Disabled) return;
        var current = Value ?? 0;
        var next = Clamp(current + Step * direction);
        if (next != current)
        {
            await SetValueAsync(next, true);
        }
    }

    private async Task Clear()
    {
        await SetValueAsync(null, true);
        await OnClear.InvokeAsync();
    }

    private static string BuildClass(params string?[] classes)
        => string.Join(' ', classes.Where(c => !string.IsNullOrWhiteSpace(c)));
}
