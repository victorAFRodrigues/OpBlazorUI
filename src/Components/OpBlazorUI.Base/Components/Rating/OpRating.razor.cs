using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;

namespace OpBlazorUI.Base.Components.Rating;

public partial class OpRating : ComponentBase
{
    private string _id = "";

    [Parameter] public int Value { get; set; }
    [Parameter] public EventCallback<int> ValueChanged { get; set; }
    [Parameter] public int Stars { get; set; } = 5;
    [Parameter] public bool Cancel { get; set; } = true;
    [Parameter] public bool Disabled { get; set; }
    [Parameter] public bool Readonly { get; set; }
    [Parameter] public bool Invalid { get; set; }
    [Parameter] public bool Required { get; set; }
    [Parameter] public string? Name { get; set; }
    [Parameter] public string? AriaLabel { get; set; }
    [Parameter] public string? InputId { get; set; }
    [Parameter] public string? StyleClass { get; set; }
    [Parameter] public string? OnIconClass { get; set; } = "pi pi-star-fill";
    [Parameter] public string? OffIconClass { get; set; } = "pi pi-star";

    [Parameter] public EventCallback<int> OnRate { get; set; }
    [Parameter] public EventCallback<int> OnChange { get; set; }
    [Parameter] public EventCallback<FocusEventArgs> OnFocus { get; set; }
    [Parameter] public EventCallback<FocusEventArgs> OnBlur { get; set; }

    [Parameter] public RenderFragment? OnIconTemplate { get; set; }
    [Parameter] public RenderFragment? OffIconTemplate { get; set; }
    [Parameter] public RenderFragment? CancelIconTemplate { get; set; }

    [Parameter(CaptureUnmatchedValues = true)]
    public Dictionary<string, object>? AdditionalAttributes { get; set; }

    protected override void OnInitialized()
    {
        _id = InputId ?? $"op-rating-{Guid.NewGuid():N}";
    }

    private string RootClass => BuildClass(
        "p-rating p-component",
        Disabled ? "p-disabled" : null,
        Readonly ? "p-readonly" : null,
        StyleClass);

    private string OptionClass(int star) => BuildClass(
        "p-rating-option",
        star <= Value && Value > 0 ? "p-rating-option-active" : null);

    private string IconClass(bool active) => BuildClass(
        "p-rating-icon",
        active ? OnIconClass : OffIconClass,
        Invalid ? "p-invalid" : null);

    private async Task Rate(int star)
    {
        if (Disabled || Readonly) return;
        Value = star;
        await ValueChanged.InvokeAsync(star);
        await OnRate.InvokeAsync(star);
        await OnChange.InvokeAsync(star);
    }

    private static string BuildClass(params string?[] classes)
        => string.Join(' ', classes.Where(c => !string.IsNullOrWhiteSpace(c)));
}
