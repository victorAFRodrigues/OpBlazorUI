using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;
using OpBlazorUI.Base.Components.Common;
using OpBlazorUI.Base.Components.ConfirmDialog.Models;

namespace OpBlazorUI.Base.Components.ConfirmPopup;

/// <summary>
/// Confirmação exibida em um painel ancorado a um elemento alvo, equivalente ao ConfirmPopup
/// do Optimus UI/PrimeNG. O painel só é renderizado quando visível e o posicionamento/âncora
/// são delegados ao <c>OpOverlayAttach</c>.
/// </summary>
public partial class OpConfirmPopup : OpComponentBase
{
    private ElementReference _root;
    private ElementReference? _anchor;
    private DotNetObjectReference<OpConfirmPopup>? _selfRef;
    private bool _visible;
    private bool _opening;
    private bool _listening;
    private bool _focusTrapped;
    private bool _disposed;
    private string _messageId = "";

    [Parameter] public string? Message { get; set; }
    [Parameter] public string Icon { get; set; } = "pi pi-exclamation-triangle";
    [Parameter] public string AcceptLabel { get; set; } = "Sim";
    [Parameter] public string RejectLabel { get; set; } = "Não";
    [Parameter] public string? AcceptIcon { get; set; }
    [Parameter] public string? RejectIcon { get; set; }
    [Parameter] public string? AcceptButtonStyleClass { get; set; }
    [Parameter] public string? RejectButtonStyleClass { get; set; }
    [Parameter] public bool AcceptVisible { get; set; } = true;
    [Parameter] public bool RejectVisible { get; set; } = true;
    [Parameter] public bool CloseOnEscape { get; set; } = true;
    [Parameter] public bool DismissableMask { get; set; }

    /// <summary>Botão que recebe o foco ao abrir: <c>accept</c>, <c>reject</c> ou <c>none</c>.</summary>
    [Parameter] public string DefaultFocus { get; set; } = "accept";

    /// <summary>Elemento âncora usado quando <see cref="ShowAsync"/> é chamado sem alvo.</summary>
    [Parameter] public ElementReference? Target { get; set; }

    [Parameter] public string Placement { get; set; } = "bottom";

    [Parameter] public EventCallback<ConfirmEvent> OnHide { get; set; }
    [Parameter] public EventCallback Accept { get; set; }
    [Parameter] public EventCallback Reject { get; set; }

    protected override void OnInitialized()
    {
        _messageId = $"op-confirmpopup-message-{Guid.NewGuid():N}";
    }

    private string RootClass => Class("p-confirmpopup p-component", StyleClass);

    private string IconClass => Class("p-confirmpopup-icon", Icon);

    private string AcceptButtonClass => Class("p-confirmpopup-accept-button", AcceptButtonStyleClass);

    private string RejectButtonClass => Class("p-confirmpopup-reject-button", RejectButtonStyleClass);

    /// <summary>Exibe o painel ancorado ao elemento informado (ou a <see cref="Target"/>).</summary>
    public Task ShowAsync(ElementReference? target = null)
    {
        _anchor = target ?? Target;
        _visible = true;
        _opening = true;
        StateHasChanged();
        return Task.CompletedTask;
    }

    /// <summary>Oculta o painel sem aceitar nem rejeitar.</summary>
    public Task HideAsync() => CloseAsync(null);

    private async Task OnAcceptClick()
    {
        await Accept.InvokeAsync();
        await CloseAsync(true);
    }

    private async Task OnRejectClick()
    {
        await Reject.InvokeAsync();
        await CloseAsync(false);
    }

    private async Task OnKeydown(KeyboardEventArgs e)
    {
        if (CloseOnEscape && e.Key == "Escape")
        {
            // Mesmo comportamento do upstream: Escape dispara o reject.
            await OnRejectClick();
        }
    }

    [JSInvokable]
    public Task OnOutsideClick() => CloseAsync(false);

    private async Task CloseAsync(bool? accepted)
    {
        if (!_visible) return;

        await RemoveOutsideListenerAsync();

        // Solta o trap antes de o painel sair do DOM (o elemento precisa estar anexado).
        if (_focusTrapped)
        {
            _focusTrapped = false;
            try
            {
                await Interop.InvokeVoidAsync(OpInterop.OptimusInterop, "focusTrapDispose", _root);
            }
            catch
            {
                // ignore
            }
        }

        _visible = false;
        _anchor = null;
        _opening = false;
        await OnHide.InvokeAsync(new ConfirmEvent(accepted ?? false));
        StateHasChanged();
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (!_opening) return;
        _opening = false;

        if (DismissableMask)
        {
            await AddOutsideListenerAsync();
        }

        // O upstream usa pFocusTrap no popup: o trap também faz o foco inicial no botão
        // indicado por DefaultFocus ("none" ou valor desconhecido não foca nada).
        try
        {
            await Interop.InvokeVoidAsync(
                OpInterop.OptimusInterop, "focusTrapInit", _root, DefaultFocusSelector(DefaultFocus), true);
            _focusTrapped = true;
        }
        catch
        {
            // ignore
        }
    }

    private static string? DefaultFocusSelector(string? mode) => mode switch
    {
        "accept" => "[data-pc-focus=\"accept\"]",
        "reject" => "[data-pc-focus=\"reject\"]",
        _ => null
    };

    private async Task AddOutsideListenerAsync()
    {
        if (_listening) return;
        _listening = true;
        _selfRef ??= DotNetObjectReference.Create(this);
        try
        {
            await Interop.InvokeVoidAsync(
                OpInterop.OptimusInterop, "addOutsideClickListener", _root, _anchor, _selfRef);
        }
        catch
        {
            _listening = false;
        }
    }

    private async Task RemoveOutsideListenerAsync()
    {
        if (!_listening) return;
        _listening = false;
        try
        {
            await Interop.InvokeVoidAsync(OpInterop.OptimusInterop, "removeOutsideClickListener", _root);
        }
        catch
        {
            // ignore
        }
    }

    public override async ValueTask DisposeAsync()
    {
        if (_disposed) return;
        _disposed = true;

        if (_listening)
        {
            try
            {
                await Interop.InvokeVoidAsync(OpInterop.OptimusInterop, "removeOutsideClickListener", _root);
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
