using Microsoft.AspNetCore.Components;
using OpBlazorUI.Base.Components.Common;

namespace OpBlazorUI.Base.Components.Directives;

/// <summary>
/// Equivalente ao <c>pStyleClass</c>: envolve um gatilho e, no clique, alterna classes
/// (<see cref="ToggleClass"/>) ou executa uma animação de enter/leave em um alvo.
/// O alvo é resolvido por um seletor CSS ou pelas palavras-chave <c>@next</c>, <c>prev</c>,
/// <c>parent</c> e <c>grandparent</c> (relativas ao wrapper).
/// Desvio consciente: o upstream aplica a diretiva ao próprio gatilho; aqui o componente
/// renderiza um <c>&lt;span style="display: contents"&gt;</c> em volta dele.
/// </summary>
public partial class OpStyleClass : OpComponentBase
{
    private ElementReference _host;

    /// <summary>Seletor do alvo (CSS ou <c>@next</c>/<c>prev</c>/<c>parent</c>/<c>grandparent</c>). Sem valor, o alvo é o próprio wrapper.</summary>
    [Parameter] public string? Target { get; set; }

    /// <summary>Modo simples: classes alternadas a cada clique.</summary>
    [Parameter] public string? ToggleClass { get; set; }

    [Parameter] public string? EnterFromClass { get; set; }

    [Parameter] public string? EnterActiveClass { get; set; }

    [Parameter] public string? EnterToClass { get; set; }

    [Parameter] public string? LeaveFromClass { get; set; }

    [Parameter] public string? LeaveActiveClass { get; set; }

    [Parameter] public string? LeaveToClass { get; set; }

    [Parameter] public bool HideOnOutsideClick { get; set; }

    [Parameter] public bool HideOnEscape { get; set; }

    [Parameter] public bool HideOnResize { get; set; }

    /// <summary><c>window</c> (padrão), <c>document</c> ou um seletor CSS do elemento observado.</summary>
    [Parameter] public string ResizeSelector { get; set; } = "window";

    [Parameter] public RenderFragment? ChildContent { get; set; }

    private string? _appliedSignature;

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        var signature = string.Join('\u001f',
            Target, ToggleClass, EnterFromClass, EnterActiveClass, EnterToClass,
            LeaveFromClass, LeaveActiveClass, LeaveToClass,
            HideOnOutsideClick.ToString(), HideOnEscape.ToString(), HideOnResize.ToString(), ResizeSelector);

        if (!firstRender && signature == _appliedSignature)
        {
            return;
        }

        if (_appliedSignature is not null)
        {
            await Interop.InvokeVoidAsync(OpInterop.DirectivesInterop, "styleClassDispose", _host);
        }

        await Interop.InvokeVoidAsync(OpInterop.DirectivesInterop, "styleClassInit", _host, new
        {
            target = Target,
            toggleClass = ToggleClass,
            enterFromClass = EnterFromClass,
            enterActiveClass = EnterActiveClass,
            enterToClass = EnterToClass,
            leaveFromClass = LeaveFromClass,
            leaveActiveClass = LeaveActiveClass,
            leaveToClass = LeaveToClass,
            hideOnOutsideClick = HideOnOutsideClick,
            hideOnEscape = HideOnEscape,
            hideOnResize = HideOnResize,
            resizeSelector = ResizeSelector
        });
        _appliedSignature = signature;
    }

    public override async ValueTask DisposeAsync()
    {
        try
        {
            await Interop.InvokeVoidAsync(OpInterop.DirectivesInterop, "styleClassDispose", _host);
        }
        catch
        {
            // ignore
        }

        await base.DisposeAsync();
    }
}
