using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;

namespace OpBlazorUI.Base.Components.InputOtp;

public sealed class OtpTokenContext
{
    public required int Index { get; init; }
    public required char? Token { get; init; }
    public required EventCallback<ChangeEventArgs> OnInput { get; init; }
    public required EventCallback<KeyboardEventArgs> OnKeyDown { get; init; }
}

public partial class OpInputOtp : ComponentBase
{
    [Parameter] public string? Value { get; set; }
    [Parameter] public EventCallback<string?> ValueChanged { get; set; }
    [Parameter] public int Length { get; set; } = 4;
    [Parameter] public bool Mask { get; set; }
    [Parameter] public bool IntegerOnly { get; set; }
    [Parameter] public bool Invalid { get; set; }
    [Parameter] public bool Disabled { get; set; }
    [Parameter] public bool Readonly { get; set; }
    [Parameter] public string Variant { get; set; } = "outlined";
    [Parameter] public string? Size { get; set; }
    [Parameter] public string? StyleClass { get; set; }
    [Parameter] public string? InputClass { get; set; }

    [Parameter] public EventCallback<string?> OnChange { get; set; }
    [Parameter] public EventCallback<FocusEventArgs> OnFocus { get; set; }
    [Parameter] public EventCallback<FocusEventArgs> OnBlur { get; set; }
    [Parameter] public EventCallback<KeyboardEventArgs> OnKeyDown { get; set; }

    [Parameter] public RenderFragment<OtpTokenContext>? Template { get; set; }

    [Parameter(CaptureUnmatchedValues = true)]
    public Dictionary<string, object>? AdditionalAttributes { get; set; }

    private char?[] _tokens = [];
    private ElementReference[] _inputs = [];
    private string _lastJoined = "";

    protected override void OnInitialized()
    {
        _tokens = new char?[Length];
        _inputs = new ElementReference[Length];
        FillTokens(Value);
        _lastJoined = Value ?? "";
    }

    protected override void OnParametersSet()
    {
        if (_tokens.Length != Length)
        {
            _tokens = new char?[Length];
            _inputs = new ElementReference[Length];
            FillTokens(Value);
            _lastJoined = Value ?? "";
        }
        else if (Value != _lastJoined)
        {
            FillTokens(Value);
            _lastJoined = Value ?? "";
        }
    }

    private string RootClass => BuildClass(
        "p-inputotp p-component",
        Invalid ? "p-invalid" : null,
        Disabled ? "p-disabled" : null,
        Variant == "filled" ? "p-variant-filled" : null,
        StyleClass);

    private string InputClassValue => BuildClass(
        "p-inputotp-input p-inputtext p-component",
        Size == "small" ? "p-inputtext-sm" : null,
        Size == "large" ? "p-inputtext-lg" : null,
        Invalid ? "p-invalid" : null,
        InputClass);

    private void FillTokens(string? value)
    {
        Array.Clear(_tokens);
        if (string.IsNullOrEmpty(value)) return;
        for (var i = 0; i < value.Length && i < _tokens.Length; i++) _tokens[i] = value[i];
    }

    private char? TokenAt(int index) => index < _tokens.Length ? _tokens[index] : null;

    private async Task UpdateValueAsync()
    {
        var joined = string.Concat(_tokens.Where(c => c.HasValue).Select(c => c!.Value));
        _lastJoined = joined;
        Value = joined;
        await ValueChanged.InvokeAsync(joined);
        await OnChange.InvokeAsync(joined);
    }

    private async Task HandleInput(int index, ChangeEventArgs e)
    {
        if (Disabled || Readonly) return;
        var text = e.Value?.ToString() ?? "";
        var ch = text.Length > 0 ? text[^1] : (char?)null;

        if (ch is char c && IntegerOnly && !char.IsDigit(c))
        {
            _tokens[index] = null;
            await UpdateValueAsync();
            return;
        }

        _tokens[index] = ch;
        await UpdateValueAsync();

        if (ch is not null && index < Length - 1) await _inputs[index + 1].FocusAsync();
    }

    private async Task HandleKeyDown(int index, KeyboardEventArgs e)
    {
        if (Disabled || Readonly)
        {
            await OnKeyDown.InvokeAsync(e);
            return;
        }

        switch (e.Key)
        {
            case "Backspace":
                if (_tokens[index] is null && index > 0)
                {
                    _tokens[index - 1] = null;
                    await UpdateValueAsync();
                    await _inputs[index - 1].FocusAsync();
                }
                else if (_tokens[index] is not null)
                {
                    _tokens[index] = null;
                    await UpdateValueAsync();
                }

                break;
            case "ArrowLeft":
                if (index > 0) await _inputs[index - 1].FocusAsync();
                break;
            case "ArrowRight":
                if (index < Length - 1) await _inputs[index + 1].FocusAsync();
                break;
        }

        await OnKeyDown.InvokeAsync(e);
    }

    private static string BuildClass(params string?[] classes)
        => string.Join(' ', classes.Where(c => !string.IsNullOrWhiteSpace(c)));
}
