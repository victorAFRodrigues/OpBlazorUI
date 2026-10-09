using System.Globalization;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;
using OpBlazorUI.Base.Components.Common;

namespace OpBlazorUI.Base.Components.Splitter;

public partial class OpSplitter : OpComponentBase
{
    private readonly List<OpSplitterPanel> _panels = new();
    private readonly List<double> _sizes = new();
    private ElementReference _root;
    private OpSplitterInterop? _interop;
    private bool _initialized;

    /// <summary>Orientação dos painéis: <c>horizontal</c> (padrão) ou <c>vertical</c>.</summary>
    [Parameter] public string Layout { get; set; } = "horizontal";

    /// <summary>Tamanho do divisor em pixels.</summary>
    [Parameter] public double GutterSize { get; set; } = 4;

    /// <summary>Passo (em %) movido pelas setas do teclado.</summary>
    [Parameter] public double Step { get; set; } = 5;

    /// <summary>Tamanho mínimo de cada painel (%).</summary>
    [Parameter] public IReadOnlyList<double> MinSizes { get; set; } = Array.Empty<double>();

    /// <summary>Tamanhos iniciais de cada painel (%).</summary>
    [Parameter] public IReadOnlyList<double> PanelSizes { get; set; } = Array.Empty<double>();

    [Parameter] public string? PanelStyle { get; set; }
    [Parameter] public string? PanelStyleClass { get; set; }

    [Parameter] public RenderFragment? ChildContent { get; set; }

    [Parameter] public EventCallback<double[]> OnResizeStart { get; set; }
    [Parameter] public EventCallback<double[]> OnResizeEnd { get; set; }

    internal string Orientation => Layout == "vertical" ? "vertical" : "horizontal";

    private string RootClass => Class(
        "p-splitter p-component",
        $"p-splitter-{Orientation}",
        StyleClass);

    internal void Register(OpSplitterPanel panel)
    {
        if (_panels.Contains(panel))
        {
            return;
        }

        panel.Index = _panels.Count;
        _panels.Add(panel);
        _ = InvokeAsync(StateHasChanged);
    }

    internal void Unregister(OpSplitterPanel panel) => _panels.Remove(panel);

    internal int PanelCount => _panels.Count;

    internal double GetSize(int index)
    {
        if (index >= 0 && index < PanelSizes.Count)
        {
            return PanelSizes[index];
        }

        if (index >= 0 && index < _sizes.Count)
        {
            return _sizes[index];
        }

        var count = Math.Max(1, _panels.Count);
        return 100.0 / count;
    }

    internal string GetPanelStyle(int index)
    {
        var size = GetSize(index).ToString("0.####", CultureInfo.InvariantCulture);
        var gutters = Math.Max(0, _panels.Count - 1) * GutterSize;
        var basis = $"flex-basis:calc({size}% - {gutters.ToString("0.####", CultureInfo.InvariantCulture)}px)";

        return string.IsNullOrEmpty(PanelStyle) ? basis : $"{basis};{PanelStyle}";
    }

    internal string GutterStyle => $"flex:0 0 auto;{(Orientation == "vertical" ? "height" : "width")}:{GutterSize.ToString("0.####", CultureInfo.InvariantCulture)}px";

    internal string AriaValueNow(int gutterIndex) => GetSize(gutterIndex).ToString("0.####", CultureInfo.InvariantCulture);

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (_initialized || _panels.Count == 0)
        {
            return;
        }

        _interop ??= new OpSplitterInterop(HandleResizeStartAsync, HandleResizeEndAsync);

        var options = new
        {
            orientation = Orientation,
            gutterSize = GutterSize,
            step = Step,
            minSizes = MinSizes,
        };

        try
        {
            await Interop.InvokeVoidAsync(OpInterop.SplitterInterop, "init", _interop.Reference, _root, options);
            _initialized = true;
        }
        catch (JSDisconnectedException)
        {
            // SSR estático / circuito desconectado
        }
    }

    public override async ValueTask DisposeAsync()
    {
        try
        {
            await Interop.InvokeVoidAsync(OpInterop.SplitterInterop, "dispose", _root);
        }
        catch (JSDisconnectedException)
        {
            // ignore
        }

        _interop?.Dispose();
        await base.DisposeAsync();
    }

    private async Task HandleResizeStartAsync(double[] sizes)
    {
        await OnResizeStart.InvokeAsync(sizes);
    }

    private async Task HandleResizeEndAsync(double[] sizes)
    {
        _sizes.Clear();
        _sizes.AddRange(sizes);
        await OnResizeEnd.InvokeAsync(sizes);
        StateHasChanged();
    }
}
