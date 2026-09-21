using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;

namespace OpBlazorUI.Base.Components.Popover;

public partial class OpPopover : ComponentBase, IAsyncDisposable
{
    private const string JsModule = "./_content/OpBlazorUI.Base/optimus.interop.js";

    private ElementReference _root;
    private ElementReference _target;
    private Lazy<Task<IJSObjectReference>>? _module;
    private DotNetObjectReference<OpPopover>? _selfRef;
    private bool _disposed;
    private bool _visible;
    private bool _aligning;
    private bool _aligned;
    private bool _listening;
    private double _top;
    private double _left;
    private bool _flipped;

    [Parameter] public string Position { get; set; } = "bottom";

    [Parameter] public bool Dismissable { get; set; } = true;

    [Parameter] public bool FocusOnShow { get; set; } = true;

    [Parameter] public string? Style { get; set; }

    [Parameter] public string? StyleClass { get; set; }

    [Parameter] public string? ContentStyle { get; set; }

    [Parameter] public string? ContentStyleClass { get; set; }

    [Parameter] public string? AriaLabel { get; set; }

    [Parameter] public string? AriaLabelledBy { get; set; }

    [Parameter] public EventCallback OnShow { get; set; }

    [Parameter] public EventCallback OnHide { get; set; }

    [Parameter] public RenderFragment? ChildContent { get; set; }

    [Parameter(CaptureUnmatchedValues = true)]
    public Dictionary<string, object>? AdditionalAttributes { get; set; }

    [Inject] private IJSRuntime Js { get; set; } = default!;

    private string RootClass => OpCss.BuildClass(
        "p-popover p-component",
        _flipped ? "p-popover-flipped" : null,
        StyleClass);

    private string ContentClass => OpCss.BuildClass("p-popover-content", ContentStyleClass);

    private string RootStyle => OpCss.BuildClass(
        _aligned
            ? $"position:fixed;top:{_top.ToString(System.Globalization.CultureInfo.InvariantCulture)}px;left:{_left.ToString(System.Globalization.CultureInfo.InvariantCulture)}px;margin:0;z-index:1000;"
            : "position:fixed;visibility:hidden;margin:0;",
        Style);

    public async Task Show(ElementReference target)
    {
        if (_visible) return;
        _target = target;
        _visible = true;
        _aligned = false;
        _aligning = true;
        await OnShow.InvokeAsync();
        StateHasChanged();
    }

    public async Task Hide()
    {
        if (!_visible) return;
        _visible = false;
        _aligned = false;
        _aligning = false;
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
        if (_aligning)
        {
            _aligning = false;
            var module = await GetModuleAsync();
            if (module is not null)
            {
                try
                {
                    var result = await module.InvokeAsync<OpOverlayAlignResult>("alignOverlay", _root, _target, Position);
                    _top = result.Top;
                    _left = result.Left;
                    _flipped = result.Flipped;
                }
                catch
                {
                    // keep default positioning
                }
            }

            _aligned = true;
            StateHasChanged();

            if (FocusOnShow)
            {
                try
                {
                    await _root.FocusAsync();
                }
                catch
                {
                    // ignore
                }
            }

            if (Dismissable && module is not null)
            {
                await AddOutsideListenerAsync(module);
            }
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

    private async Task AddOutsideListenerAsync(IJSObjectReference module)
    {
        if (_listening) return;
        _listening = true;
        _selfRef ??= DotNetObjectReference.Create(this);
        try
        {
            await module.InvokeVoidAsync("addOutsideClickListener", _root, _target, _selfRef);
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
        var module = await GetModuleAsync();
        if (module is not null)
        {
            try
            {
                await module.InvokeVoidAsync("removeOutsideClickListener", _root);
            }
            catch
            {
                // ignore
            }
        }
    }

    private async Task<IJSObjectReference?> GetModuleAsync()
    {
        if (_module is null)
        {
            _module = new Lazy<Task<IJSObjectReference>>(() =>
                Js.InvokeAsync<IJSObjectReference>("import", JsModule).AsTask());
        }

        try
        {
            return await _module.Value;
        }
        catch
        {
            return null;
        }
    }

    public async ValueTask DisposeAsync()
    {
        if (_disposed) return;
        _disposed = true;

        if (_listening)
        {
            var module = await GetModuleAsync();
            if (module is not null)
            {
                try
                {
                    await module.InvokeVoidAsync("removeOutsideClickListener", _root);
                }
                catch
                {
                    // ignore
                }
            }
        }

        _selfRef?.Dispose();

        if (_module is { IsValueCreated: true })
        {
            try
            {
                var module = await _module.Value;
                await module.DisposeAsync();
            }
            catch
            {
                // ignore
            }
        }
    }
}

internal sealed record OpOverlayAlignResult(
    [property: JsonPropertyName("top")] double Top,
    [property: JsonPropertyName("left")] double Left,
    [property: JsonPropertyName("flipped")] bool Flipped);
