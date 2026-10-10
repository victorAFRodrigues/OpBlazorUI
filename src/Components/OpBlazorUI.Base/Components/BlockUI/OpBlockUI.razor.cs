using System.Globalization;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace OpBlazorUI.Base.Components.BlockUI;

public partial class OpBlockUI : ComponentBase, IAsyncDisposable
{
    // Mesma camada "modal" usada pelo ZIndexUtils do PrimeNG/Optimus.
    private const int ModalZIndex = 1100;

    private IJSObjectReference? _module;
    private bool _scrollBlocked;

    [Inject] private IJSRuntime Js { get; set; } = default!;

    [Parameter] public bool Blocked { get; set; }

    /// <summary>Cobre a tela inteira (máscara fixa) e trava a rolagem do body enquanto bloqueado.</summary>
    [Parameter] public bool FullScreen { get; set; }

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
            var parts = new List<string>();
            if (FullScreen)
            {
                parts.Add("position: fixed");
                parts.Add("inset: 0");
            }

            if (zIndex > 0)
            {
                parts.Add($"z-index: {zIndex.ToString(CultureInfo.InvariantCulture)}");
            }

            return parts.Count > 0 ? string.Join("; ", parts) + ";" : null;
        }
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        var shouldBlock = FullScreen && Blocked;

        if (shouldBlock && !_scrollBlocked)
        {
            _scrollBlocked = true;
            _module ??= await Js.InvokeAsync<IJSObjectReference>("import", OpInterop.OptimusInterop);
            await _module.InvokeVoidAsync("blockScroll");
        }
        else if (!shouldBlock && _scrollBlocked)
        {
            _scrollBlocked = false;
            if (_module is not null)
            {
                await _module.InvokeVoidAsync("unblockScroll");
            }
        }
    }

    public async ValueTask DisposeAsync()
    {
        if (_scrollBlocked && _module is not null)
        {
            _scrollBlocked = false;
            try
            {
                await _module.InvokeVoidAsync("unblockScroll");
            }
            catch
            {
                // circuito encerrado
            }
        }

        if (_module is not null)
        {
            try
            {
                await _module.DisposeAsync();
            }
            catch
            {
                // ignore
            }
        }
    }
}
