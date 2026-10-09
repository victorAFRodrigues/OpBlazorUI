using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using OpBlazorUI.Base.Components.Forms;

namespace OpBlazorUI.Base.Components.InputOtp;

public sealed class OtpTokenContext
{
    public required int Index { get; init; }
    public required char? Token { get; init; }
    public required EventCallback<ChangeEventArgs> OnInput { get; init; }
    public required EventCallback<KeyboardEventArgs> OnKeyDown { get; init; }
}

public partial class OpInputOtp : OpInputBase<string>
{
    [Parameter] public int Length { get; set; } = 4;
    [Parameter] public bool Mask { get; set; }
    [Parameter] public bool IntegerOnly { get; set; }
    [Parameter] public bool Readonly { get; set; }
    [Parameter] public string Variant { get; set; } = "outlined";
    [Parameter] public string? Size { get; set; }
    [Parameter] public string? InputClass { get; set; }

    [Parameter] public EventCallback<string?> OnChange { get; set; }
    [Parameter] public EventCallback<FocusEventArgs> OnFocus { get; set; }
    [Parameter] public EventCallback<FocusEventArgs> OnBlur { get; set; }
    [Parameter] public EventCallback<KeyboardEventArgs> OnKeyDown { get; set; }

    [Parameter] public RenderFragment<OtpTokenContext>? Template { get; set; }

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

    private string RootClass => OpCss.BuildClass(
        "p-inputotp p-component",
        StyleClass);

    // Cada input é um pInputText com size, variant e invalid (inputotp.ts).
    private string InputClassFor(int index) => OpCss.BuildClass(
        "p-inputotp-input p-inputtext p-component",
        TokenAt(index) is not null ? "p-filled" : null,
        Variant == "filled" ? "p-variant-filled" : null,
        Size == "small" ? "p-inputtext-sm" : null,
        Size == "large" ? "p-inputtext-lg" : null,
        IsInvalid ? "p-invalid" : null,
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
        CurrentValue = joined;
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

        if (ch is not null && index < Length - 1) await TryFocusAsync(index + 1);
    }

    // Com Template os inputs são renderizados pelo usuário e o ElementReference pode não ter
    // sido capturado: sem a guarda, FocusAsync lançava InvalidOperationException.
    private async Task TryFocusAsync(int index)
    {
        if (index < 0 || index >= _inputs.Length || _inputs[index].Context is null)
        {
            return;
        }

        try
        {
            await _inputs[index].FocusAsync();
        }
        catch (InvalidOperationException)
        {
            // elemento ainda não disponível
        }
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
                    await TryFocusAsync(index - 1);
                }
                else if (_tokens[index] is not null)
                {
                    _tokens[index] = null;
                    await UpdateValueAsync();
                }

                break;
            case "ArrowLeft":
                if (index > 0) await TryFocusAsync(index - 1);
                break;
            case "ArrowRight":
                if (index < Length - 1) await TryFocusAsync(index + 1);
                break;
        }

        await OnKeyDown.InvokeAsync(e);
    }
}
