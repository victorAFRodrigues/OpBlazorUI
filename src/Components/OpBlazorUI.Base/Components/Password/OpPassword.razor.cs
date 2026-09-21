using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;

namespace OpBlazorUI.Base.Components.Password;

public partial class OpPassword : ComponentBase
{
    private string _id = "";
    private bool _masked = true;
    private bool _overlayVisible;

    // ---------------------------------------------------------------- params
    [Parameter] public string? Value { get; set; }
    [Parameter] public EventCallback<string?> ValueChanged { get; set; }
    [Parameter] public bool Feedback { get; set; } = true;
    [Parameter] public bool ToggleMask { get; set; }
    [Parameter] public bool ShowClear { get; set; }
    [Parameter] public string? Placeholder { get; set; }
    [Parameter] public string Variant { get; set; } = "outlined";
    [Parameter] public string? Size { get; set; }
    [Parameter] public bool Invalid { get; set; }
    [Parameter] public bool Disabled { get; set; }
    [Parameter] public bool Readonly { get; set; }
    [Parameter] public bool Fluid { get; set; }
    [Parameter] public string? Autocomplete { get; set; }
    [Parameter] public string? InputId { get; set; }
    [Parameter] public string? AriaLabel { get; set; }
    [Parameter] public string? AriaLabelledBy { get; set; }
    [Parameter] public int? MaxLength { get; set; }

    [Parameter] public string? PromptLabel { get; set; }
    [Parameter] public string? WeakLabel { get; set; }
    [Parameter] public string? MediumLabel { get; set; }
    [Parameter] public string? StrongLabel { get; set; }
    [Parameter] public string MediumRegex { get; set; } =
        "^(((?=.*[a-z])(?=.*[A-Z]))|((?=.*[a-z])(?=.*[0-9]))|((?=.*[A-Z])(?=.*[0-9])))(?=.{6,})";
    [Parameter] public string StrongRegex { get; set; } = "^(?=.*[a-z])(?=.*[A-Z])(?=.*[0-9])(?=.{8,})";

    // events
    [Parameter] public EventCallback<FocusEventArgs> OnFocus { get; set; }
    [Parameter] public EventCallback<FocusEventArgs> OnBlur { get; set; }
    [Parameter] public EventCallback OnClear { get; set; }
    [Parameter] public EventCallback<string?> OnInput { get; set; }

    // templates
    [Parameter] public RenderFragment? HeaderTemplate { get; set; }
    [Parameter] public RenderFragment? ContentTemplate { get; set; }
    [Parameter] public RenderFragment? FooterTemplate { get; set; }
    [Parameter] public RenderFragment? ClearIconTemplate { get; set; }
    [Parameter] public RenderFragment? ShowIconTemplate { get; set; }
    [Parameter] public RenderFragment? HideIconTemplate { get; set; }

    [Parameter(CaptureUnmatchedValues = true)]
    public Dictionary<string, object>? AdditionalAttributes { get; set; }

    // ------------------------------------------------------------ lifecycle
    protected override void OnInitialized()
    {
        _id = InputId ?? $"op-password-{Guid.NewGuid():N}";
    }

    // ------------------------------------------------------------ computed
    private string RootClass => BuildClass(
        "p-password p-component",
        Disabled ? "p-disabled" : null,
        Invalid ? "p-invalid" : null,
        Fluid ? "p-password-fluid" : null);

    private bool HasValue => !string.IsNullOrEmpty(Value);

    private int StrengthLevel
    {
        get
        {
            if (string.IsNullOrEmpty(Value)) return 0;
            try
            {
                if (Regex.IsMatch(Value, StrongRegex)) return 3;
                if (Regex.IsMatch(Value, MediumRegex)) return 2;
            }
            catch (ArgumentException)
            {
                // regex inválido fornecido pelo consumidor — ignora
            }

            return 1;
        }
    }

    private string MeterLabelClass => StrengthLevel switch
    {
        1 => "p-password-meter-weak",
        2 => "p-password-meter-medium",
        3 => "p-password-meter-strong",
        _ => ""
    };

    private string MeterWidth => StrengthLevel switch
    {
        1 => "33.33%",
        2 => "66.66%",
        3 => "100%",
        _ => "0%"
    };

    private string MeterText => StrengthLevel switch
    {
        1 => WeakLabel ?? "Fraca",
        2 => MediumLabel ?? "Média",
        3 => StrongLabel ?? "Forte",
        _ => PromptLabel ?? ""
    };

    // ------------------------------------------------------------ handlers
    private async Task HandleInputAsync(string? text)
    {
        Value = text;
        await ValueChanged.InvokeAsync(text);
        await OnInput.InvokeAsync(text);
    }

    private async Task HandleFocusAsync(FocusEventArgs e)
    {
        if (Disabled) return;
        _overlayVisible = Feedback;
        await OnFocus.InvokeAsync(e);
    }

    private async Task HandleBlurAsync(FocusEventArgs e)
    {
        _overlayVisible = false;
        await OnBlur.InvokeAsync(e);
    }

    private async Task ToggleMaskClick()
    {
        _masked = !_masked;
        await Task.CompletedTask;
    }

    private async Task Clear()
    {
        Value = string.Empty;
        await ValueChanged.InvokeAsync(string.Empty);
        await OnClear.InvokeAsync();
    }

    private static string BuildClass(params string?[] classes)
        => string.Join(' ', classes.Where(c => !string.IsNullOrWhiteSpace(c)));
}
