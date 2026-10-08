using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;
using OpBlazorUI.Base.Components.Forms;

namespace OpBlazorUI.Base.Components.InputChips;

public partial class OpInputChips : OpInputBase<List<string>>
{
    private static readonly List<string> Empty = new();

    private ElementReference _input;
    private string? _inputValue;
    private bool _focused;

    [Parameter] public string? Placeholder { get; set; }
    [Parameter] public string? Id { get; set; }
    [Parameter] public string? Name { get; set; }
    [Parameter] public string? AriaLabel { get; set; }
    [Parameter] public string? InputClass { get; set; }
    [Parameter] public string Variant { get; set; } = "outlined";
    [Parameter] public string RemoveIcon { get; set; } = "pi pi-times";
    [Parameter] public string? ChipIcon { get; set; }
    [Parameter] public int? Max { get; set; }
    [Parameter] public bool AllowDuplicate { get; set; }
    [Parameter] public bool AddOnBlur { get; set; }
    [Parameter] public bool AddOnTab { get; set; }
    [Parameter] public bool Readonly { get; set; }

    [Parameter] public IReadOnlyCollection<string>? SeparatorKeys { get; set; }
    [Parameter] public RenderFragment<string>? ChipTemplate { get; set; }

    [Parameter] public EventCallback<string> OnAdd { get; set; }
    [Parameter] public EventCallback<int> OnRemove { get; set; }
    [Parameter] public EventCallback<FocusEventArgs> OnFocus { get; set; }
    [Parameter] public EventCallback<FocusEventArgs> OnBlur { get; set; }

    private IReadOnlyList<string> Chips => CurrentValue ?? Empty;

    private string RootClass => OpCss.BuildClass(
        "p-inputchips p-component",
        _focused ? "p-focus" : null,
        Disabled ? "p-disabled" : null,
        IsInvalid ? "p-invalid" : null,
        StyleClass);

    private string InputListClass => OpCss.BuildClass(
        "p-inputchips-input",
        Variant == "filled" ? "p-variant-filled" : null);

    private string RemoveIconClass => OpCss.BuildClass("p-chip-remove-icon", RemoveIcon);

    private async Task AddChipAsync(string? raw)
    {
        var value = raw?.Trim() ?? string.Empty;
        if (value.Length == 0)
        {
            _inputValue = string.Empty;
            return;
        }

        var list = CurrentValue ?? new List<string>();

        if (!AllowDuplicate && list.Exists(c => string.Equals(c, value, StringComparison.Ordinal)))
        {
            _inputValue = string.Empty;
            return;
        }

        if (Max is int max && list.Count >= max)
        {
            return;
        }

        var next = new List<string>(list) { value };
        CurrentValue = next;
        _inputValue = string.Empty;
        await OnAdd.InvokeAsync(value);
    }

    private async Task RemoveChipAsync(int index)
    {
        var list = CurrentValue;
        if (list is null || index < 0 || index >= list.Count)
        {
            return;
        }

        var next = new List<string>(list);
        next.RemoveAt(index);
        CurrentValue = next;
        await OnRemove.InvokeAsync(index);
    }

    private async Task HandleInput(ChangeEventArgs e)
    {
        var text = e.Value?.ToString() ?? string.Empty;

        if (text.IndexOf(',') >= 0 || text.IndexOf(';') >= 0)
        {
            var parts = text.Split(new[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            _inputValue = string.Empty;

            foreach (var part in parts)
            {
                await AddChipAsync(part);
            }

            return;
        }

        _inputValue = text;
    }

    private async Task HandleKeyDownAsync(KeyboardEventArgs e)
    {
        switch (e.Key)
        {
            case "Enter":
                if (!string.IsNullOrWhiteSpace(_inputValue))
                {
                    await AddChipAsync(_inputValue);
                }
                break;

            case "Backspace":
                if (string.IsNullOrEmpty(_inputValue) && Chips.Count > 0)
                {
                    await RemoveChipAsync(Chips.Count - 1);
                }
                break;

            case "Tab":
                if (AddOnTab)
                {
                    await AddChipAsync(_inputValue);
                }
                break;

            default:
                if (IsSeparator(e.Key))
                {
                    await AddChipAsync(_inputValue);
                }
                break;
        }
    }

    private async Task HandleBlurAsync(FocusEventArgs e)
    {
        _focused = false;

        if (AddOnBlur && !string.IsNullOrWhiteSpace(_inputValue))
        {
            await AddChipAsync(_inputValue);
        }

        await OnBlur.InvokeAsync(e);
    }

    private async Task HandleFocus(FocusEventArgs e)
    {
        _focused = true;
        await OnFocus.InvokeAsync(e);
    }

    private async Task FocusInputAsync()
    {
        if (Disabled || Readonly)
        {
            return;
        }

        try
        {
            await _input.FocusAsync();
        }
        catch (JSDisconnectedException)
        {
            // SSR estático / circuito desconectado
        }
    }

    private bool IsSeparator(string key)
        => key == "," || (SeparatorKeys is not null && SeparatorKeys.Contains(key));
}
