using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using OpBlazorUI.Base.Models;

namespace OpBlazorUI.Base.Components.Menu;

public partial class OpMenu : ComponentBase
{
    private string _id = "";
    private int _focusedItemIndex = -1;

    private bool _overlayVisible;
    private bool _panelRendered;
    private bool _panelClosing;
    private string? _panelAnimationClass;

    private bool _focusInside;
    private DateTime _lastPanelPointerDown = DateTime.MinValue;
    private ElementReference _listRef;

    [Parameter] public IReadOnlyList<OpMenuItem>? Items { get; set; }
    [Parameter] public IReadOnlyList<OpMenuGroup>? Groups { get; set; }
    [Parameter] public bool Popup { get; set; }
    [Parameter] public string? StyleClass { get; set; }
    [Parameter] public string? Id { get; set; }
    [Parameter] public string? AriaLabel { get; set; }

    [Parameter] public EventCallback<OpMenuItem> OnItemClick { get; set; }
    [Parameter] public EventCallback OnShow { get; set; }
    [Parameter] public EventCallback OnHide { get; set; }

    [Parameter] public RenderFragment<OpMenuItem>? ItemTemplate { get; set; }
    [Parameter] public RenderFragment? StartTemplate { get; set; }
    [Parameter] public RenderFragment? EndTemplate { get; set; }

    [Parameter(CaptureUnmatchedValues = true)]
    public Dictionary<string, object>? AdditionalAttributes { get; set; }

    protected override void OnInitialized()
    {
        _id = Id ?? $"op-menu-{Guid.NewGuid():N}";
    }

    private string RootClass => BuildClass(
        "p-menu p-component",
        Popup ? "p-menu-overlay" : null,
        StyleClass);

    private IReadOnlyList<OpMenuItem> FlatItems => Items ?? Array.Empty<OpMenuItem>();

    private IReadOnlyList<OpMenuGroup> GroupItems => Groups ?? Array.Empty<OpMenuGroup>();

    private List<OpMenuItem> AllItems => Groups is { Count: > 0 }
        ? GroupItems.SelectMany(g => g.Items).ToList()
        : FlatItems.ToList();

    private string ItemId(int index) => $"{_id}_{index}";

    private string ItemClass(OpMenuItem item, int index) => BuildClass(
        "p-menu-item",
        _focusedItemIndex == index ? "p-focus" : null,
        item.Disabled ? "p-disabled" : null,
        item.StyleClass);

    // ------------------------------------------------------------ popup
    public async Task ShowAsync()
    {
        if (_overlayVisible || !Popup) return;
        _overlayVisible = true;
        _panelRendered = true;
        _panelClosing = false;
        _panelAnimationClass = "p-anchored-overlay-enter-active";
        _focusedItemIndex = -1;
        await OnShow.InvokeAsync();
        StateHasChanged();
    }

    public async Task HideAsync()
    {
        if (!_overlayVisible || !Popup) return;
        _overlayVisible = false;
        _panelClosing = true;
        _panelAnimationClass = "p-anchored-overlay-leave-active";
        StateHasChanged();
        await OnHide.InvokeAsync();
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

    /// <summary>
    /// Marca a interação com o painel. Itens sem <c>Url</c> não são focáveis, então
    /// clicar neles dispara <c>focusout</c> e fecharia o popup antes de o item
    /// receber o clique.
    /// </summary>
    private void OnPanelMouseDown()
    {
        _lastPanelPointerDown = DateTime.UtcNow;
        _focusInside = true;
    }

    private async Task OnRootFocusOut(FocusEventArgs e)
    {
        if (!_overlayVisible || _panelClosing || !Popup) return;

        _focusInside = false;

        await Task.Delay(10);

        var interactedWithPanel = (DateTime.UtcNow - _lastPanelPointerDown).TotalMilliseconds < 250;

        if (!_focusInside && !interactedWithPanel)
        {
            await HideAsync();
        }
    }

    private async Task OnRootKeydown(KeyboardEventArgs e)
    {
        if (!Popup) return;
        if (e.Key == "Escape" && _overlayVisible)
        {
            await HideAsync();
        }
    }

    // ------------------------------------------------------------ interaction
    private async Task OnItemClicked(OpMenuItem item)
    {
        if (item.Disabled) return;
        await OnItemClick.InvokeAsync(item);
        if (Popup)
        {
            await HideAsync();
        }
    }

    private async Task OnListKeydown(KeyboardEventArgs e)
    {
        switch (e.Key)
        {
            case "ArrowDown":
                _focusedItemIndex = FindNextEnabled(_focusedItemIndex);
                StateHasChanged();
                break;
            case "ArrowUp":
                _focusedItemIndex = FindNextEnabled(_focusedItemIndex, backward: true);
                StateHasChanged();
                break;
            case "Home":
                _focusedItemIndex = FindFirstEnabled();
                StateHasChanged();
                break;
            case "End":
                _focusedItemIndex = FindLastEnabled();
                StateHasChanged();
                break;
            case "Enter":
            case " ":
                if (_focusedItemIndex >= 0 && _focusedItemIndex < AllItems.Count)
                {
                    await OnItemClicked(AllItems[_focusedItemIndex]);
                }

                break;
        }
    }

    private int FindFirstEnabled()
    {
        for (var i = 0; i < AllItems.Count; i++)
        {
            if (!AllItems[i].Disabled && !AllItems[i].Separator) return i;
        }

        return -1;
    }

    private int FindLastEnabled()
    {
        for (var i = AllItems.Count - 1; i >= 0; i--)
        {
            if (!AllItems[i].Disabled && !AllItems[i].Separator) return i;
        }

        return -1;
    }

    private int FindNextEnabled(int from, bool backward = false)
    {
        if (AllItems.Count == 0) return -1;
        var step = backward ? -1 : 1;
        var i = from;
        for (var n = 0; n < AllItems.Count; n++)
        {
            i = (i + step + AllItems.Count) % AllItems.Count;
            if (!AllItems[i].Disabled && !AllItems[i].Separator) return i;
        }

        return -1;
    }

    private static string BuildClass(params string?[] classes)
        => string.Join(' ', classes.Where(c => !string.IsNullOrWhiteSpace(c)));
}