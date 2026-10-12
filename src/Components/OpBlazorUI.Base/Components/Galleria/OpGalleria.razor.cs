using Microsoft.AspNetCore.Components;
using OpBlazorUI.Base.Components.Common;

namespace OpBlazorUI.Base.Components.Galleria;

public partial class OpGalleria<TItem> : OpComponentBase
{
    private int _activeIndex;
    private int _thumbStart;
    private bool _visible;
    private CancellationTokenSource? _autoplayCts;

    [Parameter] public IReadOnlyList<TItem> Value { get; set; } = Array.Empty<TItem>();

    [Parameter] public int ActiveIndex { get; set; }
    [Parameter] public EventCallback<int> ActiveIndexChanged { get; set; }

    /// <summary>Visibilidade no modo fullscreen (two-way).</summary>
    [Parameter] public bool Visible { get; set; }
    [Parameter] public EventCallback<bool> VisibleChanged { get; set; }

    [Parameter] public bool FullScreen { get; set; }
    [Parameter] public int NumVisible { get; set; } = 3;
    [Parameter] public bool Circular { get; set; }
    [Parameter] public bool AutoPlay { get; set; }
    [Parameter] public int TransitionInterval { get; set; } = 4000;

    [Parameter] public bool ShowIndicators { get; set; }
    [Parameter] public bool ShowIndicatorsOnItem { get; set; }
    [Parameter] public string IndicatorsPosition { get; set; } = "bottom";

    [Parameter] public bool ShowThumbnails { get; set; } = true;
    [Parameter] public string ThumbnailsPosition { get; set; } = "bottom";

    [Parameter] public bool ShowItemNavigators { get; set; }
    [Parameter] public bool ShowItemNavigatorsOnHover { get; set; }

    /// <summary>Opções responsivas — aceitas por compatibilidade; ainda não aplicadas (ver Problemas Conhecidos).</summary>
    [Parameter] public object? ResponsiveOptions { get; set; }

    [Parameter] public string? ContainerStyle { get; set; }

    [Parameter] public RenderFragment<TItem>? ItemTemplate { get; set; }
    [Parameter] public RenderFragment<TItem>? ThumbnailTemplate { get; set; }
    [Parameter] public RenderFragment<TItem>? CaptionTemplate { get; set; }
    [Parameter] public RenderFragment<int>? IndicatorTemplate { get; set; }
    [Parameter] public RenderFragment? HeaderTemplate { get; set; }
    [Parameter] public RenderFragment? FooterTemplate { get; set; }

    private int Count => Value.Count;

    private int Current => Count == 0 ? 0 : Math.Clamp(_activeIndex, 0, Count - 1);

    private string RootClass => Class(
        "p-galleria p-component",
        FullScreen ? "p-galleria-fullscreen" : null,
        ShowIndicatorsOnItem ? "p-galleria-inset-indicators" : null,
        ShowItemNavigators && ShowItemNavigatorsOnHover ? "p-galleria-hover-navigators" : null,
        ShowThumbnails ? $"p-galleria-thumbnails-{ThumbnailsPosition}" : null,
        ShowIndicators ? $"p-galleria-indicators-{IndicatorsPosition}" : null,
        StyleClass);

    protected override void OnParametersSet()
    {
        _activeIndex = ActiveIndex;
        _visible = Visible;
        _thumbStart = Math.Clamp(_thumbStart, 0, Math.Max(0, Count - Math.Max(1, NumVisible)));
        RestartAutoPlay();
    }

    private void RestartAutoPlay()
    {
        StopAutoPlay();

        if (!AutoPlay || Count <= 1)
        {
            return;
        }

        _autoplayCts = new CancellationTokenSource();
        var token = _autoplayCts.Token;

        _ = Task.Run(async () =>
        {
            try
            {
                using var timer = new PeriodicTimer(TimeSpan.FromMilliseconds(Math.Max(500, TransitionInterval)));
                while (await timer.WaitForNextTickAsync(token))
                {
                    await InvokeAsync(async () =>
                    {
                        await NextAsync();
                        StateHasChanged();
                    });
                }
            }
            catch (OperationCanceledException)
            {
                // descartado / parado
            }
        });
    }

    private void StopAutoPlay()
    {
        _autoplayCts?.Cancel();
        _autoplayCts?.Dispose();
        _autoplayCts = null;
    }

    public override async ValueTask DisposeAsync()
    {
        StopAutoPlay();
        await base.DisposeAsync();
    }

    private async Task SetIndexAsync(int index)
    {
        if (Count == 0 || index == Current)
        {
            return;
        }

        _activeIndex = index;
        EnsureThumbVisible(index);
        await ActiveIndexChanged.InvokeAsync(index);
    }

    private async Task NextAsync()
    {
        if (Count == 0)
        {
            return;
        }

        var next = Current + 1;
        if (next >= Count)
        {
            if (!Circular)
            {
                return;
            }

            next = 0;
        }

        await SetIndexAsync(next);
    }

    private async Task PrevAsync()
    {
        if (Count == 0)
        {
            return;
        }

        var prev = Current - 1;
        if (prev < 0)
        {
            if (!Circular)
            {
                return;
            }

            prev = Count - 1;
        }

        await SetIndexAsync(prev);
    }

    private void EnsureThumbVisible(int index)
    {
        var visible = Math.Max(1, NumVisible);
        if (index < _thumbStart)
        {
            _thumbStart = index;
        }
        else if (index >= _thumbStart + visible)
        {
            _thumbStart = index - visible + 1;
        }
    }

    private void ThumbNext()
    {
        var max = Math.Max(0, Count - Math.Max(1, NumVisible));
        _thumbStart = Math.Min(max, _thumbStart + 1);
    }

    private void ThumbPrev() => _thumbStart = Math.Max(0, _thumbStart - 1);

    private string PrevNavClass => Class(
        "p-galleria-prev-button",
        "p-galleria-nav-button",
        !Circular && Current <= 0 ? "p-disabled" : null);

    private string NextNavClass => Class(
        "p-galleria-next-button",
        "p-galleria-nav-button",
        !Circular && Current >= Count - 1 ? "p-disabled" : null);

    private async Task CloseAsync()
    {
        _visible = false;
        await VisibleChanged.InvokeAsync(false);
    }

    private string IndicatorClass(int index) => Class(
        "p-galleria-indicator",
        index == Current ? "p-galleria-indicator-active" : null);

    private string ThumbItemClass(int index) => Class(
        "p-galleria-thumbnail-item",
        index == Current ? "p-galleria-thumbnail-item-current" : null,
        index == Current ? "p-galleria-thumbnail-item-active" : null,
        index == _thumbStart ? "p-galleria-thumbnail-item-start" : null);
}
