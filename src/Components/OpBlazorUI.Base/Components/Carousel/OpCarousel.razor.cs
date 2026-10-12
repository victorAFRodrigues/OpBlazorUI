using System.Globalization;
using Microsoft.AspNetCore.Components;
using OpBlazorUI.Base.Components.Common;

namespace OpBlazorUI.Base.Components.Carousel;

public partial class OpCarousel<TItem> : OpComponentBase
{
    private int _activeIndex;
    private CancellationTokenSource? _autoplayCts;

    [Parameter] public IReadOnlyList<TItem> Value { get; set; } = Array.Empty<TItem>();

    [Parameter] public RenderFragment<TItem>? ItemTemplate { get; set; }
    [Parameter] public RenderFragment? HeaderTemplate { get; set; }
    [Parameter] public RenderFragment? FooterTemplate { get; set; }

    [Parameter] public int NumVisible { get; set; } = 1;
    [Parameter] public int NumScroll { get; set; } = 1;
    [Parameter] public int Page { get; set; }
    [Parameter] public EventCallback<int> PageChanged { get; set; }
    [Parameter] public bool Circular { get; set; }

    /// <summary><c>horizontal</c> (padrão) ou <c>vertical</c>.</summary>
    [Parameter] public string Orientation { get; set; } = "horizontal";

    [Parameter] public string VerticalViewPortHeight { get; set; } = "300px";

    /// <summary>Intervalo do autoplay em ms. <c>0</c> desliga.</summary>
    [Parameter] public int AutoPlayInterval { get; set; }

    /// <summary>Opções responsivas — aceitas por compatibilidade; ainda não aplicadas (ver Problemas Conhecidos).</summary>
    [Parameter] public object? ResponsiveOptions { get; set; }

    [Parameter] public string? ContentClass { get; set; }

    private int Count => Value.Count;

    private int Visible => Math.Max(1, NumVisible);

    private int MaxIndex => Math.Max(0, Count - Visible);

    private string RootClass => Class(
        "p-carousel p-component",
        Orientation == "vertical" ? "p-carousel-vertical" : null,
        StyleClass);

    private string ViewportStyle => Orientation == "vertical" ? $"height:{VerticalViewPortHeight}" : "";

    private string ItemListStyle
    {
        get
        {
            var shift = (_activeIndex * (100.0 / Visible)).ToString("0.####", CultureInfo.InvariantCulture);
            var axis = Orientation == "vertical" ? $"0,-{shift}%" : $"-{shift}%,0";
            return $"transform:translate3d({axis},0);transition:transform 500ms ease 0s";
        }
    }

    private string ItemStyle
    {
        get
        {
            var basis = (100.0 / Visible).ToString("0.####", CultureInfo.InvariantCulture);
            return Orientation == "vertical" ? $"flex:0 0 {basis}%;max-height:{basis}%" : $"flex:0 0 {basis}%;max-width:{basis}%";
        }
    }

    protected override void OnParametersSet()
    {
        _activeIndex = Math.Clamp(Page, 0, MaxIndex);
        RestartAutoPlay();
    }

    private void RestartAutoPlay()
    {
        StopAutoPlay();

        if (AutoPlayInterval <= 0 || Count <= Visible)
        {
            return;
        }

        _autoplayCts = new CancellationTokenSource();
        var token = _autoplayCts.Token;

        _ = Task.Run(async () =>
        {
            try
            {
                using var timer = new PeriodicTimer(TimeSpan.FromMilliseconds(Math.Max(500, AutoPlayInterval)));
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

    private async Task GoToAsync(int index)
    {
        var next = Math.Clamp(index, 0, MaxIndex);
        if (next == _activeIndex)
        {
            return;
        }

        _activeIndex = next;
        await PageChanged.InvokeAsync(next);
    }

    private async Task NextAsync()
    {
        if (Count <= Visible)
        {
            return;
        }

        var next = _activeIndex + Math.Max(1, NumScroll);
        if (next > MaxIndex)
        {
            next = Circular ? 0 : MaxIndex;
        }

        await GoToAsync(next);
    }

    private async Task PrevAsync()
    {
        if (Count <= Visible)
        {
            return;
        }

        var prev = _activeIndex - Math.Max(1, NumScroll);
        if (prev < 0)
        {
            prev = Circular ? MaxIndex : 0;
        }

        await GoToAsync(prev);
    }

    private int PageCount => Count <= Visible ? 1 : (int)Math.Floor((double)(Count - Visible) / Math.Max(1, NumScroll)) + 1;

    private int CurrentPage => _activeIndex / Math.Max(1, NumScroll);

    private bool IsItemActive(int index) => index >= _activeIndex && index < _activeIndex + Visible;

    private string ContentWrapperClass => Class("p-carousel-content", ContentClass);

    private string PrevButtonClass => Class(
        "p-carousel-prev-button",
        !Circular && _activeIndex <= 0 ? "p-disabled" : null);

    private string NextButtonClass => Class(
        "p-carousel-next-button",
        !Circular && _activeIndex >= MaxIndex ? "p-disabled" : null);

    private string ItemClass(int index) => Class(
        "p-carousel-item",
        IsItemActive(index) ? "p-carousel-item-active" : null);

    private string IndicatorClass(int page) => Class(
        "p-carousel-indicator",
        page == CurrentPage ? "p-carousel-indicator-active" : null);
}
