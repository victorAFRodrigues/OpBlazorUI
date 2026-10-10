using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using OpBlazorUI.Base.Components.Common;
using OpBlazorUI.Base.Models;

namespace OpBlazorUI.Base.Components.Menu;

public partial class OpContextMenu : OpComponentBase
{
    private readonly HashSet<string> _open = new(StringComparer.Ordinal);
    private ElementReference _panel;
    private ElementReference _host;
    private ElementReference? _anchor;
    private (double X, double Y)? _coordinate;
    private bool _visible;
    private DotNetObjectReference<OpContextMenu>? _selfRef;

    [Parameter] public IReadOnlyList<OpMenuItem> Model { get; set; } = Array.Empty<OpMenuItem>();
    [Parameter] public EventCallback<OpMenuItem> OnItemClick { get; set; }
    [Parameter] public EventCallback OnShow { get; set; }
    [Parameter] public EventCallback OnHide { get; set; }

    /// <summary>Seletor CSS cujos elementos abrem o menu no clique direito.</summary>
    [Parameter] public string? Target { get; set; }

    /// <summary>Abre o menu no clique direito em qualquer ponto do documento.</summary>
    [Parameter] public bool Global { get; set; }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender && (Global || !string.IsNullOrEmpty(Target)))
        {
            _selfRef ??= DotNetObjectReference.Create(this);
            await Interop.InvokeVoidAsync(
                OpInterop.MenuInterop, "initContextMenu", _host, Global ? null : Target, _selfRef);
        }
    }

    [JSInvokable]
    public Task OnContextMenuAt(double x, double y) => ShowAsync(x, y);

    public override async ValueTask DisposeAsync()
    {
        if (_selfRef is not null)
        {
            await Interop.InvokeVoidAsync(OpInterop.MenuInterop, "disposeContextMenu", _host);
            _selfRef.Dispose();
        }

        await base.DisposeAsync();
    }

    private string RootClass => Class("p-contextmenu p-component", StyleClass);

    private bool IsOpen(string path) => _open.Contains(path);

    private Task ToggleSubmenuAsync(string path)
    {
        OpMenuPaths.Toggle(_open, path);
        return Task.CompletedTask;
    }

    /// <summary>Exibe o menu ancorado ao elemento informado (ou ao elemento anterior).</summary>
    public Task ShowAsync(ElementReference? anchor = null)
    {
        _anchor = anchor;
        _coordinate = null;
        return OpenAsync();
    }

    /// <summary>Exibe o menu nas coordenadas do viewport (ex.: <c>MouseEventArgs.ClientX/ClientY</c>).</summary>
    public Task ShowAsync(double x, double y)
    {
        _anchor = null;
        _coordinate = (x, y);
        return OpenAsync();
    }

    private async Task OpenAsync()
    {
        _visible = true;
        _open.Clear();
        await OnShow.InvokeAsync();
        StateHasChanged();
    }

    public async Task HideAsync()
    {
        if (!_visible)
        {
            return;
        }

        _visible = false;
        _open.Clear();
        await OnHide.InvokeAsync();
        StateHasChanged();
    }

    public async Task ToggleAsync(ElementReference? anchor = null)
    {
        if (_visible)
        {
            await HideAsync();
        }
        else
        {
            await ShowAsync(anchor);
        }
    }

    private async Task HandleItemClickAsync(OpMenuItem item)
    {
        await OnItemClick.InvokeAsync(item);
        await HideAsync();
    }
}
