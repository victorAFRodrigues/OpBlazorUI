using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using OpBlazorUI.Base.Components.Common;

namespace OpBlazorUI.Base.Components.VirtualScroller;

public sealed record OpVirtualScrollerLazyLoadEvent(int First, int Last);

public sealed record OpVirtualScrollerScrollEvent(double Top, double Left);

public sealed record OpVirtualScrollerIndexEvent(int First, int Last);

public sealed record OpVirtualScrollerItemContext<TItem>(TItem Item, int Index, bool Even, bool Odd);

public sealed record OpVirtualScrollerContentContext<TItem>(
    IReadOnlyList<TItem> Items,
    int First,
    int Last,
    Func<int, string, Task> ScrollToIndex);

public partial class OpVirtualScroller<TItem> : OpComponentBase
{
    private ElementReference _root;
    private DotNetObjectReference<OpVirtualScroller<TItem>>? _selfRef;
    private bool _initialized;
    private bool _disposed;

    private int _first;
    private int _last;
    private double _scrollTop;
    private double _scrollLeft;
    private double _viewportWidth;
    private double _viewportHeight;
    private int _columns = 1;
    private bool _initialLazyRequested;

    // ---------------------------------------------------------------- params
    [Parameter] public IReadOnlyList<TItem>? Items { get; set; }

    [Parameter] public int ItemSize { get; set; }

    [Parameter] public int[]? ItemSizeGrid { get; set; }

    [Parameter] public string Orientation { get; set; } = "vertical";

    [Parameter] public string? ScrollHeight { get; set; }

    [Parameter] public string? ScrollWidth { get; set; }

    [Parameter] public int Delay { get; set; }

    [Parameter] public int ResizeDelay { get; set; }

    [Parameter] public int Step { get; set; }

    [Parameter] public bool Lazy { get; set; }

    [Parameter] public bool ShowLoader { get; set; }

    [Parameter] public bool Loading { get; set; }

    [Parameter] public int? NumToleratedItems { get; set; }

    [Parameter] public bool Disabled { get; set; }

    [Parameter] public bool AppendOnly { get; set; }

    [Parameter] public bool Inline { get; set; }

    [Parameter] public bool AutoSize { get; set; }

    [Parameter] public string? Style { get; set; }

    [Parameter] public int TabIndex { get; set; }

    // events
    [Parameter] public EventCallback<OpVirtualScrollerLazyLoadEvent> OnLazyLoad { get; set; }

    [Parameter] public EventCallback<OpVirtualScrollerScrollEvent> OnScroll { get; set; }

    [Parameter] public EventCallback<OpVirtualScrollerIndexEvent> OnScrollIndexChange { get; set; }

    // templates
    [Parameter] public RenderFragment<OpVirtualScrollerContentContext<TItem>>? ContentTemplate { get; set; }

    [Parameter] public RenderFragment<OpVirtualScrollerItemContext<TItem>>? ItemTemplate { get; set; }

    [Parameter] public RenderFragment? LoaderTemplate { get; set; }

    [Parameter] public RenderFragment? LoaderIconTemplate { get; set; }

    // ------------------------------------------------------------ computed
    private int Count => Items?.Count ?? 0;

    private int ItemHeight => Orientation == "both" && ItemSizeGrid is { Length: >= 2 }
        ? ItemSizeGrid[0]
        : ItemSize;

    private int ItemWidth => Orientation == "both" && ItemSizeGrid is { Length: >= 2 }
        ? ItemSizeGrid[1]
        : ItemSize;

    private int TotalRows
    {
        get
        {
            if (Orientation != "both") return Count;
            var cols = Math.Max(1, _columns);
            return (int)Math.Ceiling((double)Count / cols);
        }
    }

    private double ViewportSize => Orientation == "horizontal" ? _viewportWidth : _viewportHeight;

    private int PrimaryItemSize => Orientation == "horizontal" ? ItemWidth : ItemHeight;

    private string RootClass => OpCss.BuildClass(
        "p-virtualscroller p-component",
        Inline ? "p-virtualscroller-inline" : null,
        Orientation == "horizontal" ? "p-virtualscroller-horizontal p-horizontal-scroll" : null,
        Orientation == "both" ? "p-virtualscroller-both p-both-scroll" : null,
        StyleClass);

    private string RootStyle
    {
        get
        {
            var height = ScrollHeight;
            var width = ScrollWidth;
            return OpCss.BuildClass(
                height is not null ? $"height:{height};" : null,
                width is not null ? $"width:{width};" : null,
                Style);
        }
    }

    private string SpacerStyle
    {
        get
        {
            if (Orientation == "both")
            {
                return $"width:1px;height:{TotalRows * ItemHeight}px;";
            }

            if (Orientation == "horizontal")
            {
                return $"height:1px;width:{Count * ItemWidth}px;";
            }

            return $"width:1px;height:{Count * ItemHeight}px;";
        }
    }

    private string ContentStyle
    {
        get
        {
            if (Orientation == "both")
            {
                var cols = Math.Max(1, _columns);
                var firstRow = _columns > 0 ? _first / cols : 0;
                return $"display:flex;flex-wrap:wrap;transform:translateY({firstRow * ItemHeight}px);";
            }

            if (Orientation == "horizontal")
            {
                return $"transform:translateX({_first * ItemWidth}px);";
            }

            return $"transform:translateY({_first * ItemHeight}px);";
        }
    }

    // ------------------------------------------------------------ lifecycle
    protected override void OnParametersSet()
    {
        // Recompute the visible window whenever the data set changes.
        Recalculate();
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender && !_initialized && !_disposed)
        {
            _initialized = true;
            _selfRef = DotNetObjectReference.Create(this);
            try
            {
                await Interop.InvokeVoidAsync(OpInterop.VirtualScrollerInterop, "init", _selfRef, _root);
            }
            catch
            {
                // ignore
            }
        }
    }

    // ------------------------------------------------------------ range
    private void Recalculate()
    {
        var count = Count;
        if (count == 0 || PrimaryItemSize <= 0)
        {
            _first = 0;
            _last = 0;

            // Lazy com dados iniciais vazios: pede o primeiro lote uma única vez.
            if (count == 0 && Lazy && Step > 0 && !_initialLazyRequested)
            {
                _initialLazyRequested = true;
                _ = OnLazyLoad.InvokeAsync(new OpVirtualScrollerLazyLoadEvent(0, Step));
            }

            return;
        }

        var viewport = ResolveViewportSize();
        var tolerated = NumToleratedItems ?? Math.Max(1, (int)Math.Ceiling(viewport / PrimaryItemSize) / 2);

        int first;
        int last;

        if (Orientation == "both")
        {
            var cols = Math.Max(1, _columns);
            var firstRow = Math.Clamp((int)Math.Floor(_scrollTop / ItemHeight) - tolerated, 0, TotalRows);
            var visibleRows = (int)Math.Ceiling(viewport / ItemHeight) + 2 * tolerated;
            var lastRow = Math.Min(firstRow + visibleRows, TotalRows);
            first = firstRow * cols;
            last = Math.Min(lastRow * cols, count);
        }
        else
        {
            var offset = Orientation == "horizontal" ? _scrollLeft : _scrollTop;
            first = Math.Clamp((int)Math.Floor(offset / PrimaryItemSize) - tolerated, 0, count);
            var visibleCount = (int)Math.Ceiling(viewport / PrimaryItemSize) + 2 * tolerated;
            last = Math.Min(first + visibleCount, count);
        }

        var changed = first != _first || last != _last;

        if (AppendOnly && first < _first)
        {
            first = _first;
        }

        _first = first;
        _last = last;

        if (changed)
        {
            _ = OnScrollIndexChange.InvokeAsync(new OpVirtualScrollerIndexEvent(_first, _last));

            if (Lazy && Step > 0 && _last >= Count)
            {
                _ = OnLazyLoad.InvokeAsync(new OpVirtualScrollerLazyLoadEvent(Count, Count + Step));
            }
        }
    }

    private double ResolveViewportSize()
    {
        if (ViewportSize > 0) return ViewportSize;
        return ParseLength(Orientation == "horizontal" ? ScrollWidth : ScrollHeight, 0);
    }

    private static double ParseLength(string? value, double fallback)
    {
        if (string.IsNullOrWhiteSpace(value)) return fallback;
        var digits = new string(value.Where(char.IsDigit).ToArray());
        return double.TryParse(digits, out var parsed) ? parsed : fallback;
    }

    // ------------------------------------------------------------ interop callbacks
    [JSInvokable]
    public async Task NotifyScroll(double top, double left)
    {
        _scrollTop = top;
        _scrollLeft = left;

        var oldFirst = _first;
        var oldLast = _last;
        Recalculate();

        await OnScroll.InvokeAsync(new OpVirtualScrollerScrollEvent(top, left));

        if (_first != oldFirst || _last != oldLast)
        {
            StateHasChanged();
        }
    }

    [JSInvokable]
    public async Task NotifyResize(double width, double height)
    {
        _viewportWidth = width;
        _viewportHeight = height;

        if (Orientation == "both" && ItemWidth > 0)
        {
            _columns = Math.Max(1, (int)Math.Floor(width / ItemWidth));
        }

        var oldFirst = _first;
        var oldLast = _last;
        Recalculate();

        if (_first != oldFirst || _last != oldLast)
        {
            StateHasChanged();
        }

        await Task.CompletedTask;
    }

    // ------------------------------------------------------------ public api
    public async Task ScrollToIndexAsync(int index, string behavior = "auto")
    {
        if (index < 0) index = 0;
        if (index >= Count) index = Math.Max(0, Count - 1);

        double top;
        double left;

        if (Orientation == "both")
        {
            var cols = Math.Max(1, _columns);
            top = (index / cols) * ItemHeight;
            left = 0;
        }
        else if (Orientation == "horizontal")
        {
            top = 0;
            left = index * ItemWidth;
        }
        else
        {
            top = index * ItemHeight;
            left = 0;
        }

        try
        {
            await Interop.InvokeVoidAsync(OpInterop.VirtualScrollerInterop, "scrollTo", _root, top, left, behavior);
        }
        catch
        {
            // ignore
        }
    }

    // ------------------------------------------------------------ render helpers
    private RenderFragment? RenderContent()
    {
        var count = Count;
        if (count == 0) return null;

        // Desabilitado: sem virtualização, renderiza tudo.
        var first = Disabled ? 0 : _first;
        var last = Disabled ? count : _last;

        if (ItemTemplate is not null)
        {
            return builder =>
            {
                for (var i = first; i < last && i < count; i++)
                {
                    var index = i;
                    builder.AddContent(0, ItemTemplate(new OpVirtualScrollerItemContext<TItem>(Items![index], index, index % 2 == 0, index % 2 != 0)));
                }
            };
        }

        if (ContentTemplate is not null)
        {
            var visible = new List<TItem>(Math.Max(0, last - first));
            for (var i = first; i < last && i < count; i++)
            {
                visible.Add(Items![i]);
            }

            var ctx = new OpVirtualScrollerContentContext<TItem>(visible, first, last, ScrollToIndexAsync);
            return builder => builder.AddContent(0, ContentTemplate(ctx));
        }

        // default: render raw items
        return builder =>
        {
            for (var i = first; i < last && i < count; i++)
            {
                builder.AddContent(0, Items![i]);
            }
        };
    }

    public override async ValueTask DisposeAsync()
    {
        if (_disposed) return;
        _disposed = true;

        if (_initialized)
        {
            try
            {
                await Interop.InvokeVoidAsync(OpInterop.VirtualScrollerInterop, "dispose", _root);
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
