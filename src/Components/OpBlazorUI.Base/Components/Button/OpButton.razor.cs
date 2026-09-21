using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;

namespace OpBlazorUI.Base.Components.Button;

public partial class OpButton : ComponentBase
{
    [Parameter] public string? Label { get; set; }
    [Parameter] public string? Icon { get; set; }
    [Parameter] public string IconPos { get; set; } = "left";
    [Parameter] public string? Severity { get; set; }
    [Parameter] public string? Variant { get; set; }
    [Parameter] public bool Raised { get; set; }
    [Parameter] public bool Rounded { get; set; }
    [Parameter] public bool Text { get; set; }
    [Parameter] public bool Outlined { get; set; }
    [Parameter] public bool Link { get; set; }
    [Parameter] public bool Plain { get; set; }
    [Parameter] public bool Loading { get; set; }
    [Parameter] public bool Disabled { get; set; }
    [Parameter] public string? Size { get; set; }
    [Parameter] public bool Fluid { get; set; }
    [Parameter] public string Type { get; set; } = "button";
    [Parameter] public string? AriaLabel { get; set; }
    [Parameter] public int TabIndex { get; set; }
    [Parameter] public string? StyleClass { get; set; }
    [Parameter] public string? LoadingIcon { get; set; }
    [Parameter] public string? Badge { get; set; }
    [Parameter] public string? BadgeSeverity { get; set; }
    [Parameter] public string? BadgeSize { get; set; }
    [Parameter] public string? BadgeClass { get; set; }

    [Parameter] public EventCallback<MouseEventArgs> OnClick { get; set; }
    [Parameter] public EventCallback<FocusEventArgs> OnFocus { get; set; }
    [Parameter] public EventCallback<FocusEventArgs> OnBlur { get; set; }

    [Parameter] public RenderFragment? ChildContent { get; set; }
    [Parameter] public RenderFragment? IconTemplate { get; set; }
    [Parameter] public RenderFragment? LoadingIconTemplate { get; set; }

    [Parameter(CaptureUnmatchedValues = true)]
    public Dictionary<string, object>? AdditionalAttributes { get; set; }

    private bool HasIcon => !string.IsNullOrEmpty(Icon) || IconTemplate != null;
    private bool HasLabel => !string.IsNullOrEmpty(Label);
    private bool HasBadge => !string.IsNullOrEmpty(Badge);
    private bool IconOnly => HasIcon && !HasLabel && ChildContent == null;

    private string RootClass => BuildClass(
        "p-button p-component",
        IconOnly ? "p-button-icon-only" : null,
        (IconPos is "top" or "bottom") && HasLabel ? "p-button-vertical" : null,
        Loading ? "p-button-loading" : null,
        Link || Variant == "link" ? "p-button-link" : null,
        Severity is null ? null : $"p-button-{Severity}",
        Raised || Variant == "raised" ? "p-button-raised" : null,
        Rounded ? "p-button-rounded" : null,
        Text || Variant == "text" ? "p-button-text" : null,
        Outlined || Variant == "outlined" ? "p-button-outlined" : null,
        Size == "small" ? "p-button-sm" : null,
        Size == "large" ? "p-button-lg" : null,
        Plain ? "p-button-plain" : null,
        Fluid ? "p-button-fluid" : null,
        StyleClass);

    private static string BuildClass(params string?[] classes)
        => string.Join(' ', classes.Where(c => !string.IsNullOrWhiteSpace(c)));
}