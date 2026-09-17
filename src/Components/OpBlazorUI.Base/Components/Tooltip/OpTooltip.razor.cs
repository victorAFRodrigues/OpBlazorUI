using Microsoft.AspNetCore.Components;

namespace OpBlazorUI.Base.Components.Tooltip;

public partial class OpTooltip : ComponentBase
{
    private string _id = "";
    private bool _visible;
    private bool _hovering;
    private int _enterSequence;
    private int _leaveSequence;

    [Parameter] public string? Content { get; set; }
    [Parameter] public string Position { get; set; } = "right";
    [Parameter] public bool Disabled { get; set; }
    [Parameter] public int ShowDelay { get; set; }
    [Parameter] public int HideDelay { get; set; }
    [Parameter] public string? StyleClass { get; set; }

    [Parameter] public RenderFragment? ChildContent { get; set; }

    [Parameter(CaptureUnmatchedValues = true)]
    public Dictionary<string, object>? AdditionalAttributes { get; set; }

    protected override void OnInitialized()
    {
        _id = $"op-tt-{Guid.NewGuid():N}";
    }

    private string RootClass => BuildClass(
        "p-tooltip p-component",
        $"p-tooltip-{Position}",
        StyleClass);

    private string WrapperStyle => "display: inline-block; position: relative;";

    private async Task OnEnter()
    {
        if (Disabled || string.IsNullOrEmpty(Content)) return;
        _hovering = true;
        var seq = ++_enterSequence;
        if (ShowDelay > 0) await Task.Delay(ShowDelay);
        if (!_hovering || seq != _enterSequence) return;
        _visible = true;
        StateHasChanged();
    }

    private async Task OnLeave()
    {
        _hovering = false;
        var seq = ++_leaveSequence;
        if (!_visible) return;
        if (HideDelay > 0) await Task.Delay(HideDelay);
        if (_hovering || seq != _leaveSequence) return;
        _visible = false;
        StateHasChanged();
    }

    private static string BuildClass(params string?[] classes)
        => string.Join(' ', classes.Where(c => !string.IsNullOrWhiteSpace(c)));
}
