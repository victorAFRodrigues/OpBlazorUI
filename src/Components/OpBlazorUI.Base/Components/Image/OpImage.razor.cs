using System.Globalization;
using Microsoft.AspNetCore.Components;
using OpBlazorUI.Base.Components.Common;

namespace OpBlazorUI.Base.Components.Image;

public partial class OpImage : OpComponentBase
{
    private const double ZoomStep = 0.1;
    private const double MinScale = 0.5;
    private const double MaxScale = 1.5;

    private bool _previewVisible;
    private int _rotate;
    private double _scale = 1;

    [Parameter] public string? Src { get; set; }
    [Parameter] public string? SrcSet { get; set; }
    [Parameter] public string? Sizes { get; set; }
    [Parameter] public string? Alt { get; set; }
    [Parameter] public string? Width { get; set; }
    [Parameter] public string? Height { get; set; }
    [Parameter] public string? Loading { get; set; }

    [Parameter] public bool Preview { get; set; }
    [Parameter] public string? PreviewImageSrc { get; set; }
    [Parameter] public string? PreviewImageSrcSet { get; set; }
    [Parameter] public string? PreviewImageSizes { get; set; }

    [Parameter] public string? ImageClass { get; set; }
    [Parameter] public string? ImageStyle { get; set; }
    [Parameter] public string? AriaZoomImage { get; set; } = "Visualizar imagem";

    [Parameter] public RenderFragment? IndicatorTemplate { get; set; }
    [Parameter] public RenderFragment? ImageTemplate { get; set; }

    [Parameter] public EventCallback OnShow { get; set; }
    [Parameter] public EventCallback OnHide { get; set; }
    [Parameter] public EventCallback OnImageError { get; set; }

    private string RootClass => Class(
        "p-image p-component",
        Preview ? "p-image-preview" : null,
        StyleClass);

    private string? PreviewSource => PreviewImageSrc ?? Src;

    private string ZoomOutButtonClass => Class("p-image-action", "p-image-zoom-out-button", _scale <= MinScale ? "p-disabled" : null);

    private string ZoomInButtonClass => Class("p-image-action", "p-image-zoom-in-button", _scale >= MaxScale ? "p-disabled" : null);

    private string PreviewImageStyle =>
        $"transform: rotate({_rotate.ToString(CultureInfo.InvariantCulture)}deg) scale({_scale.ToString("0.##", CultureInfo.InvariantCulture)})";

    private async Task OpenPreview()
    {
        _previewVisible = true;
        _rotate = 0;
        _scale = 1;
        await OnShow.InvokeAsync();
    }

    private async Task ClosePreviewAsync()
    {
        if (!_previewVisible)
        {
            return;
        }

        _previewVisible = false;
        await OnHide.InvokeAsync();
    }

    private void RotateRight() => _rotate = (_rotate + 90) % 360;

    private void RotateLeft() => _rotate = (_rotate - 90 + 360) % 360;

    private void ZoomIn() => _scale = Math.Min(MaxScale, _scale + ZoomStep);

    private void ZoomOut() => _scale = Math.Max(MinScale, _scale - ZoomStep);

    private Task HandleImageError() => OnImageError.InvokeAsync();
}
