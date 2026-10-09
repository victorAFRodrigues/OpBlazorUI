using Microsoft.AspNetCore.Components;
using OpBlazorUI.Base.Components.Common;

namespace OpBlazorUI.Base.Components.Popover;

public partial class OpPopover : OpComponentBase
{
    private ElementReference _root;
    private ElementReference _target;
    private bool _visible;
    private bool _opening;

    [Parameter] public string Position { get; set; } = "bottom";

    /// <summary>Fecha com clique fora do painel e do alvo.</summary>
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
    // .p-popover-flipped direto no DOM (e reposiciona no scroll/resize). Clique fora e
    // Escape também chegam por ele.
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
                // painel removido antes do foco ou circuito encerrado
            }
        }
    }
}
