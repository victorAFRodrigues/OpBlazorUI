using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;
using OpBlazorUI.Base.Components.Common;

namespace OpBlazorUI.Base.Components.Popover;

public partial class OpPopover : OpComponentBase
{
    private ElementReference _root;
    private ElementReference _target;
    private DotNetObjectReference<OpPopover>? _selfRef;
    private bool _disposed;
    private bool _visible;
    private bool _opening;
    private bool _listening;

    [Parameter] public string Position { get; set; } = "bottom";

    [Parameter] public bool Dismissable { get; set; } = true;

    [Parameter] public bool FocusOnShow { get; set; } = true;

    [Parameter] public string? Style { get; set; }

    [Parameter] public string? ContentStyle { get; set; }

    [Parameter] public string? ContentStyleClass { get; set; }

    [Parameter] public string? AriaLabel { get; set; }

    [Parameter] public string? AriaLabelledBy { get; set; }

    [Parameter] public EventCallback OnShow { get; set; }

    [Parameter] public EventCallback OnHide { get; set; }

    [Parameter] public RenderFragment? ChildContent { get; set; }

    // Sem estado de posição aqui: o OpOverlayAttach aplica posição, z-index, a seta e
    // .p-popover-flipped direto no DOM (e reposiciona no scroll/resize).
    private string RootClass => OpCss.BuildClass("p-popover p-component", StyleClass);

    private string ContentClass => OpCss.BuildClass("p-popover-content", ContentStyleClass);

    public async Task Show(ElementReference target)
    {
        if (_visible) return;
        _target = target;
        _visible = true;
        _opening = true;
        await OnShow.InvokeAsync();
        StateHasChanged();
    }

    public async Task Hide()
    {
        if (!_visible) return;
        _visible = false;
        _opening = false;
        await RemoveOutsideListenerAsync();
        await OnHide.InvokeAsync();
        StateHasChanged();
    }

    public async Task Toggle(ElementReference target)
    {
        if (_visible)
        {
            await Hide();
        }
        else
        {
            await Show(target);
        }
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (!_opening) return;
        _opening = false;

        if (FocusOnShow)
        {
            try
            {
                await _root.FocusAsync(preventScroll: true);
            }
            catch
            {
                // ignore
            }
        }

        if (Dismissable)
        {
            await AddOutsideListenerAsync();
        }
    }

    private async Task OnKeydown(KeyboardEventArgs e)
    {
        if (e.Key == "Escape")
        {
            await Hide();
        }
    }

    [JSInvokable]
    public async Task OnOutsideClick()
    {
        if (_visible)
        {
            await Hide();
        }
    }

    private async Task AddOutsideListenerAsync()
    {
        if (_listening) return;
        _listening = true;
        _selfRef ??= DotNetObjectReference.Create(this);
        try
        {
            await Interop.InvokeVoidAsync(
                OpInterop.OptimusInterop, "addOutsideClickListener", _root, _target, _selfRef);
        }
        catch
        {
            _listening = false;
        }
    }

    private async Task RemoveOutsideListenerAsync()
    {
        if (!_listening) return;
        _listening = false;
        try
        {
            await Interop.InvokeVoidAsync(OpInterop.OptimusInterop, "removeOutsideClickListener", _root);
        }
        catch
        {
            // ignore
        }
    }

    public override async ValueTask DisposeAsync()
    {
        if (_disposed) return;
        _disposed = true;

        if (_listening)
        {
            try
            {
                await Interop.InvokeVoidAsync(OpInterop.OptimusInterop, "removeOutsideClickListener", _root);
            }
            catch
            {
                // ignore
            }
        }

        _selfRef?.Dispose();
        await base.DisposeAsync();
    }
}
