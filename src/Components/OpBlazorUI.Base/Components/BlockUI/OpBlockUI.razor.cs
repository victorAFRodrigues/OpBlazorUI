using System.Globalization;
using Microsoft.AspNetCore.Components;

namespace OpBlazorUI.Base.Components.BlockUI;

public partial class OpBlockUI : ComponentBase
{
    // Mesma camada "modal" usada pelo ZIndexUtils do PrimeNG/Optimus.
    private const int ModalZIndex = 1100;

    [Parameter] public bool Blocked { get; set; }

    [Parameter] public bool AutoZIndex { get; set; } = true;

    [Parameter] public int BaseZIndex { get; set; }

    [Parameter] public string? StyleClass { get; set; }

    [Parameter] public string? Style { get; set; }

    [Parameter] public RenderFragment? ChildContent { get; set; }

    /// <summary>Conteúdo exibido sobre a máscara (ex.: um loader). Sem ele, o BlockUI apenas bloqueia.</summary>
    [Parameter] public RenderFragment? ContentTemplate { get; set; }

    [Parameter(CaptureUnmatchedValues = true)]
    public Dictionary<string, object>? AdditionalAttributes { get; set; }

    private string RootClass => OpCss.BuildClass("p-blockui p-component", StyleClass);

    // Enquanto bloqueado, o root isola o empilhamento: o z-index alto da máscara cobre qualquer
    // filho, mas não escapa do BlockUI (não passa por cima de topbars ou overlays da página).
    private string? RootStyle => Blocked
        ? string.IsNullOrWhiteSpace(Style) ? "isolation: isolate;" : $"{Style.TrimEnd().TrimEnd(';')}; isolation: isolate;"
        : Style;

    private string MaskClass => OpCss.BuildClass(
        "p-blockui-mask",
        "p-overlay-mask",
        "p-overlay-mask-enter-active");

    private string? MaskStyle
    {
        get
        {
            var zIndex = AutoZIndex ? BaseZIndex + ModalZIndex : BaseZIndex;
            return zIndex > 0 ? $"z-index: {zIndex.ToString(CultureInfo.InvariantCulture)};" : null;
        }
    }
}
