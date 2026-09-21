using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using OpBlazorUI.Base.Models;

namespace OpBlazorUI.Base.Components.SplitButton;

public partial class OpSplitButton : ComponentBase
{
    private string _id = "";
    private bool _overlayVisible;
    private bool _panelRendered;
    private bool _panelClosing;
    private string? _panelAnimationClass;
    private bool _focusInside;
    private DateTime _lastPanelPointerDown = DateTime.MinValue;
    private readonly HashSet<OpMenuItem> _openPath = new();

    // ---------------------------------------------------------------- params
    [Parameter] public IReadOnlyList<OpMenuItem>? Model { get; set; }
    [Parameter] public string? Label { get; set; }
    [Parameter] public string? Icon { get; set; }
    [Parameter] public string IconPos { get; set; } = "left";
    [Parameter] public string? Severity { get; set; }
    [Parameter] public bool Raised { get; set; }
    [Parameter] public bool Rounded { get; set; }
    [Parameter] public bool Text { get; set; }
    [Parameter] public bool Outlined { get; set; }
    [Parameter] public bool Plain { get; set; }
    [Parameter] public string? Size { get; set; }
    [Parameter] public bool Disabled { get; set; }
    [Parameter] public bool ButtonDisabled { get; set; }
    [Parameter] public bool MenuButtonDisabled { get; set; }
    [Parameter] public string? DropdownIcon { get; set; } = "pi pi-chevron-down";
    [Parameter] public string? ExpandAriaLabel { get; set; }
    [Parameter] public string? MenuStyleClass { get; set; }
    [Parameter] public string? StyleClass { get; set; }
    [Parameter] public int TabIndex { get; set; }
    [Parameter] public bool Autofocus { get; set; }

    // events
    [Parameter] public EventCallback<MouseEventArgs> OnClick { get; set; }
    [Parameter] public EventCallback OnMenuShow { get; set; }
    [Parameter] public EventCallback OnMenuHide { get; set; }
    [Parameter] public EventCallback<MouseEventArgs> OnDropdownClick { get; set; }

    // templates
    [Parameter] public RenderFragment? ContentTemplate { get; set; }
    [Parameter] public RenderFragment? DropdownIconTemplate { get; set; }

    [Parameter(CaptureUnmatchedValues = true)]
    public Dictionary<string, object>? AdditionalAttributes { get; set; }

    // ------------------------------------------------------------ lifecycle
    protected override void OnInitialized()
    {
        _id = $"op-splitbutton-{Guid.NewGuid():N}";
    }

    // ------------------------------------------------------------ computed
    private IReadOnlyList<OpMenuItem> Items => Model ?? Array.Empty<OpMenuItem>();

    private string RootClass => BuildClass(
        "p-splitbutton p-component",
        Rounded ? "p-splitbutton-rounded" : null,
        Raised ? "p-splitbutton-raised" : null,
        StyleClass);

    private string OverlayClass => BuildClass(
        "p-tieredmenu p-tieredmenu-overlay p-component",
        MenuStyleClass,
        _panelAnimationClass);

    // ------------------------------------------------------------ overlay
    private async Task OnDropdownButtonClick(MouseEventArgs e)
    {
        await OnDropdownClick.InvokeAsync(e);
        if (Disabled || MenuButtonDisabled) return;
        if (_overlayVisible)
        {
            await CloseAsync();
        }
        else
        {
            await OpenAsync();
        }
    }

    private async Task OpenAsync()
    {
        if (_overlayVisible) return;
        _overlayVisible = true;
        _panelRendered = true;
        _panelClosing = false;
        _panelAnimationClass = "p-anchored-overlay-enter-active";
        _openPath.Clear();
        await OnMenuShow.InvokeAsync();
        StateHasChanged();
    }

    private async Task CloseAsync()
    {
        if (!_overlayVisible) return;
        _overlayVisible = false;
        _panelClosing = true;
        _panelAnimationClass = "p-anchored-overlay-leave-active";
        StateHasChanged();
        await OnMenuHide.InvokeAsync();
        _ = ForceRemovePanelAsync();
    }

    private async Task ForceRemovePanelAsync()
    {
        await Task.Delay(400);
        if (_panelClosing)
        {
            _panelRendered = false;
            _panelClosing = false;
            _panelAnimationClass = null;
            await InvokeAsync(StateHasChanged);
        }
    }

    private async Task OnPanelAnimationEnd()
    {
        if (_panelClosing)
        {
            _panelRendered = false;
            _panelClosing = false;
        }

        _panelAnimationClass = null;
        await InvokeAsync(StateHasChanged);
    }

    private void OnRootFocusIn(FocusEventArgs e)
    {
        _focusInside = true;
    }

    private void OnPanelMouseDown()
    {
        _lastPanelPointerDown = DateTime.UtcNow;
        _focusInside = true;
    }

    private async Task OnRootFocusOut(FocusEventArgs e)
    {
        if (!_overlayVisible || _panelClosing) return;

        _focusInside = false;
        await Task.Delay(10);

        var interactedWithPanel = (DateTime.UtcNow - _lastPanelPointerDown).TotalMilliseconds < 250;
        if (!_focusInside && !interactedWithPanel)
        {
            await CloseAsync();
        }
    }

    private async Task OnRootKeydown(KeyboardEventArgs e)
    {
        if (e.Key == "Escape" && _overlayVisible)
        {
            await CloseAsync();
        }
    }

    // ------------------------------------------------------------ menu
    private async Task OnItemClick(OpMenuItem item)
    {
        if (item.Disabled) return;
        item.Command?.Invoke();
        await CloseAsync();
    }

    private void OpenItem(OpMenuItem item)
    {
        if (item.Disabled) return;
        _openPath.Clear();
        var path = new List<OpMenuItem>();
        foreach (var root in Items)
        {
            if (FindPath(root, item, path)) break;
        }

        foreach (var n in path)
        {
            if (n.HasChildren) _openPath.Add(n);
        }

        StateHasChanged();
    }

    private void CloseSubmenus()
    {
        if (_openPath.Count == 0) return;
        _openPath.Clear();
        StateHasChanged();
    }

    private static bool FindPath(OpMenuItem current, OpMenuItem target, List<OpMenuItem> path)
    {
        path.Add(current);
        if (ReferenceEquals(current, target)) return true;
        if (current.Items is not null)
        {
            foreach (var child in current.Items)
            {
                if (FindPath(child, target, path)) return true;
            }
        }

        path.RemoveAt(path.Count - 1);
        return false;
    }

    private static string BuildClass(params string?[] classes)
        => string.Join(' ', classes.Where(c => !string.IsNullOrWhiteSpace(c)));
}
