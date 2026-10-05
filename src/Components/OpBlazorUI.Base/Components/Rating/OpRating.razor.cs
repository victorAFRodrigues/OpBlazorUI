using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using OpBlazorUI.Base.Components.Forms;

namespace OpBlazorUI.Base.Components.Rating;

public partial class OpRating : OpInputBase<int>
{
    private string _id = "";

    [Parameter] public int Stars { get; set; } = 5;
    [Parameter] public bool Cancel { get; set; } = true;
    [Parameter] public bool Readonly { get; set; }
    [Parameter] public bool Required { get; set; }
    [Parameter] public string? Name { get; set; }
    [Parameter] public string? AriaLabel { get; set; }
    [Parameter] public string? InputId { get; set; }
    [Parameter] public string? OnIconClass { get; set; } = "pi pi-star-fill";
    [Parameter] public string? OffIconClass { get; set; } = "pi pi-star";

    [Parameter] public EventCallback<int> OnRate { get; set; }
    [Parameter] public EventCallback<int> OnChange { get; set; }
    [Parameter] public EventCallback<FocusEventArgs> OnFocus { get; set; }
    [Parameter] public EventCallback<FocusEventArgs> OnBlur { get; set; }

    [Parameter] public RenderFragment? OnIconTemplate { get; set; }
    [Parameter] public RenderFragment? OffIconTemplate { get; set; }
    [Parameter] public RenderFragment? CancelIconTemplate { get; set; }

    protected override void OnInitialized()
    {
        _id = InputId ?? $"op-rating-{Guid.NewGuid():N}";
    }

    private string RootClass => OpCss.BuildClass(
        "p-rating p-component",
        Disabled ? "p-disabled" : null,
        Readonly ? "p-readonly" : null,
        StyleClass);

    private string OptionClass(int star) => OpCss.BuildClass(
        "p-rating-option",
        star <= CurrentValue && CurrentValue > 0 ? "p-rating-option-active" : null);

    private string IconClass(bool active) => OpCss.BuildClass(
        "p-rating-icon",
        active ? OnIconClass : OffIconClass,
        IsInvalid ? "p-invalid" : null);

    private async Task Rate(int star)
    {
        if (Disabled || Readonly) return;
        CurrentValue = star;
        await OnRate.InvokeAsync(star);
        await OnChange.InvokeAsync(star);
    }
}
