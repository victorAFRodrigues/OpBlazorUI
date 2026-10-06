using Microsoft.AspNetCore.Components;

namespace OpBlazorUI.Base.Components.ConfirmDialog.Models;

public sealed class ConfirmOptions
{
    public string? Key { get; set; }

    public string? Message { get; set; }
    public string? Header { get; set; }
    public string? Icon { get; set; }

    public string? AcceptLabel { get; set; }
    public string? RejectLabel { get; set; }
    public string? AcceptIcon { get; set; }
    public string? RejectIcon { get; set; }
    public string? AcceptButtonStyleClass { get; set; }
    public string? RejectButtonStyleClass { get; set; }
    public Dictionary<string, object>? AcceptButtonProps { get; set; }
    public Dictionary<string, object>? RejectButtonProps { get; set; }
    public bool AcceptVisible { get; set; } = true;
    public bool RejectVisible { get; set; } = true;

    public bool Modal { get; set; } = true;
    public bool Closable { get; set; } = true;
    public bool CloseOnEscape { get; set; } = true;
    public bool DismissableMask { get; set; } = false;
    public bool BlockScroll { get; set; } = true;
    public bool FocusTrap { get; set; } = true;
    public string? DefaultFocus { get; set; }

    public string? Position { get; set; }

    public string? AcceptAriaLabel { get; set; }
    public string? RejectAriaLabel { get; set; }
    public string? CloseAriaLabel { get; set; }
    public string? CloseIcon { get; set; }

    public string? Style { get; set; }
    public string? StyleClass { get; set; }
    public string? MaskStyleClass { get; set; }

    /// <summary>
    /// "Badge" aplica o layout de confirmação com badge circular sobreposto ao topo do diálogo.
    /// É uma extensão do OpBlazorUI, não existe no OptimusUI/PrimeNG.
    /// </summary>
    public string? Appearance { get; set; }

    public bool AutoZIndex { get; set; } = true;
    public int BaseZIndex { get; set; }

    public string TransitionOptions { get; set; } = "150ms cubic-bezier(0, 0, 0.2, 1)";

    public Action? Accept { get; set; }
    public Action? Reject { get; set; }
    public Action<bool>? OnClose { get; set; }

    public RenderFragment<ConfirmMessageContext>? MessageTemplate { get; set; }
    public RenderFragment? HeaderTemplate { get; set; }
    public RenderFragment? FooterTemplate { get; set; }
    public RenderFragment? IconTemplate { get; set; }
    public RenderFragment<ConfirmHeadlessContext>? HeadlessTemplate { get; set; }

    public ElementReference? Target { get; set; }
}

public sealed class ConfirmMessageContext
{
    public ConfirmOptions Options { get; set; } = new();
}

public sealed class ConfirmHeadlessContext
{
    public ConfirmOptions Options { get; set; } = new();
    public Action OnAccept { get; set; } = () => { };
    public Action OnReject { get; set; } = () => { };
}

public sealed class ConfirmEvent
{
    public bool Accepted { get; }

    public ConfirmEvent(bool accepted)
    {
        Accepted = accepted;
    }
}