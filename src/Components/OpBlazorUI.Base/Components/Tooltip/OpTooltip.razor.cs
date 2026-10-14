using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;

namespace OpBlazorUI.Base.Components.Tooltip;

public partial class OpTooltip : ComponentBase
{
    private ElementReference _wrapperRef;
    private string _id = "";
    private bool _visible;
    private bool _hovering;
    private int _enterSequence;
    private int _leaveSequence;
    private int _lifeSequence;

    [Parameter] public string? Content { get; set; }
    [Parameter] public string Position { get; set; } = "right";
    [Parameter] public bool Disabled { get; set; }
    [Parameter] public int ShowDelay { get; set; }
    [Parameter] public int HideDelay { get; set; }

    /// <summary>Evento que exibe o tooltip: <c>hover</c>, <c>focus</c> ou <c>both</c>.</summary>
    [Parameter] public string TooltipEvent { get; set; } = "hover";

    /// <summary>Se falso, o tooltip só some ao sair do hover/foco (sem tempo máximo).</summary>
    [Parameter] public bool AutoHide { get; set; } = true;

    /// <summary>Tempo (ms) que o tooltip permanece visível antes de sumir sozinho. 0 desativa.</summary>
    [Parameter] public int Life { get; set; }

    [Parameter] public string? StyleClass { get; set; }

    [Parameter] public RenderFragment? ChildContent { get; set; }

    [Parameter(CaptureUnmatchedValues = true)]
    public Dictionary<string, object>? AdditionalAttributes { get; set; }

    private bool HoverEnabled => TooltipEvent is "hover" or "both";
    private bool FocusEnabled => TooltipEvent is "focus" or "both";

    protected override void OnInitialized()
    {
        _id = $"op-tt-{Guid.NewGuid():N}";
    }

    private string RootClass => BuildClass(
        "p-tooltip p-component",
        $"p-tooltip-{Position}",
        StyleClass);

    private string WrapperStyle => "display: inline-block; position: relative;";

    private Task OnMouseEnter() => HoverEnabled ? ShowAsync() : Task.CompletedTask;

    private Task OnMouseLeave() => HoverEnabled ? HideAsync() : Task.CompletedTask;

    private Task OnFocusIn() => FocusEnabled ? ShowAsync() : Task.CompletedTask;

    private Task OnFocusOut() => FocusEnabled ? HideAsync() : Task.CompletedTask;

    private async Task ShowAsync()
    {
        if (Disabled || string.IsNullOrEmpty(Content)) return;
        _hovering = true;
        var seq = ++_enterSequence;
        if (ShowDelay > 0) await Task.Delay(ShowDelay);
        if (!_hovering || seq != _enterSequence) return;
        _visible = true;
        StateHasChanged();

        if (AutoHide && Life > 0)
        {
            var life = ++_lifeSequence;
            _ = HideAfterLifeAsync(life);
        }
    }

    private async Task HideAfterLifeAsync(int life)
    {
        await Task.Delay(Life);
        if (life != _lifeSequence || !_visible) return;
        _visible = false;
        await InvokeAsync(StateHasChanged);
    }

    private async Task HideAsync()
    {
        _hovering = false;
        _lifeSequence++;
        var seq = ++_leaveSequence;
        if (!_visible) return;
        if (HideDelay > 0) await Task.Delay(HideDelay);
        if (_hovering || seq != _leaveSequence) return;
        _visible = false;
        StateHasChanged();
    }

    // WCAG 1.4.13: Escape fecha o tooltip sem mover o foco/hover.
    private void OnKeyDown(KeyboardEventArgs e)
    {
        if (_visible && e.Key == "Escape")
        {
            _visible = false;
            _hovering = false;
            _enterSequence++;
            StateHasChanged();
        }
    }

    private static string BuildClass(params string?[] classes)
        => string.Join(' ', classes.Where(c => !string.IsNullOrWhiteSpace(c)));
}
