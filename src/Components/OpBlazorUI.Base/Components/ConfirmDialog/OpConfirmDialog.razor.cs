using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using OpBlazorUI.Base.Components.Common;
using OpBlazorUI.Base.Components.ConfirmDialog.Models;
using OpBlazorUI.Base.Services;

namespace OpBlazorUI.Base.Components.ConfirmDialog;

public partial class OpConfirmDialog : OpModalBase
{
    [Inject] private OpConfirmationService ConfirmationService { get; set; } = default!;

    [Parameter] public string? Key { get; set; }
    [Parameter] public string? Header { get; set; }
    [Parameter] public string? Message { get; set; }
    [Parameter] public string? Icon { get; set; }
    [Parameter] public string AcceptLabel { get; set; } = "Sim";
    [Parameter] public string RejectLabel { get; set; } = "Não";
    [Parameter] public string? AcceptIcon { get; set; }
    [Parameter] public string? RejectIcon { get; set; }
    [Parameter] public string? AcceptButtonStyleClass { get; set; }
    [Parameter] public string? RejectButtonStyleClass { get; set; }
    [Parameter] public Dictionary<string, object>? AcceptButtonProps { get; set; }
    [Parameter] public Dictionary<string, object>? RejectButtonProps { get; set; }
    [Parameter] public bool AcceptVisible { get; set; } = true;
    [Parameter] public bool RejectVisible { get; set; } = true;
    [Parameter] public bool Modal { get; set; } = true;
    [Parameter] public bool Closable { get; set; } = true;
    [Parameter] public bool CloseOnEscape { get; set; } = true;
    [Parameter] public bool DismissableMask { get; set; } = false;
    [Parameter] public string Position { get; set; } = "center";
    [Parameter] public string? Style { get; set; }
    [Parameter] public string? StyleClass { get; set; }
    [Parameter] public string? MaskStyleClass { get; set; }
    [Parameter] public string CloseIcon { get; set; } = "pi pi-times";
    [Parameter] public string CloseAriaLabel { get; set; } = "Fechar";

    // Vazio por padrão: sem aria-label, o nome acessível do botão é o próprio Label visível
    // (WCAG 2.5.3 - Label in Name). Defina apenas se o rótulo não for descritivo.
    [Parameter] public string? AcceptAriaLabel { get; set; }
    [Parameter] public string? RejectAriaLabel { get; set; }
    [Parameter] public bool AutoZIndex { get; set; } = true;
    [Parameter] public int BaseZIndex { get; set; }
    [Parameter] public bool FocusTrap { get; set; } = true;
    [Parameter] public string DefaultFocus { get; set; } = "accept";

    /// <summary>
    /// "Badge" aplica o layout de confirmação com badge circular sobreposto ao topo do diálogo.
    /// É uma extensão do OpBlazorUI, não existe no OptimusUI/PrimeNG.
    /// </summary>
    [Parameter] public string? Appearance { get; set; }

    [Parameter] public RenderFragment<ConfirmMessageContext>? MessageTemplate { get; set; }
    [Parameter] public RenderFragment? HeaderTemplate { get; set; }
    [Parameter] public RenderFragment? FooterTemplate { get; set; }
    [Parameter] public RenderFragment? IconTemplate { get; set; }
    [Parameter] public RenderFragment<ConfirmHeadlessContext>? HeadlessTemplate { get; set; }

    [Parameter] public EventCallback<ConfirmEvent> OnHide { get; set; }

    [Parameter(CaptureUnmatchedValues = true)]
    public Dictionary<string, object>? AdditionalAttributes { get; set; }

    private bool _visible;
    private ElementReference _root;
    private string _headerId = "";
    private string _messageId = "";
    private bool _lastRenderedVisible;
    private ConfirmOptions? _resolved;

    protected override void OnInitialized()
    {
        _headerId = $"op-confirmdialog-header-{Guid.NewGuid():N}";
        _messageId = $"op-confirmdialog-message-{Guid.NewGuid():N}";
        ConfirmationService.Subscribe(OnConfirmRequested);
    }

    private void OnConfirmRequested()
    {
        var options = ConfirmationService.CurrentOptions;
        if (options == null) return;

        if (!string.Equals(options.Key, Key, StringComparison.Ordinal))
        {
            return;
        }

        _resolved = ResolveOptions(options);
        _visible = true;
        _ = InvokeAsync(StateHasChanged);
    }

    private ConfirmOptions ResolveOptions(ConfirmOptions options) => new()
    {
        Key = options.Key,
        Message = options.Message ?? Message,
        Header = options.Header ?? Header,
        Icon = options.Icon ?? Icon,
        AcceptLabel = options.AcceptLabel ?? AcceptLabel,
        RejectLabel = options.RejectLabel ?? RejectLabel,
        AcceptIcon = options.AcceptIcon ?? AcceptIcon,
        RejectIcon = options.RejectIcon ?? RejectIcon,
        AcceptButtonStyleClass = options.AcceptButtonStyleClass ?? AcceptButtonStyleClass,
        RejectButtonStyleClass = options.RejectButtonStyleClass ?? RejectButtonStyleClass,
        AcceptButtonProps = options.AcceptButtonProps ?? AcceptButtonProps,
        RejectButtonProps = options.RejectButtonProps ?? RejectButtonProps,
        AcceptVisible = options.AcceptVisible,
        RejectVisible = options.RejectVisible,
        Modal = options.Modal,
        Closable = options.Closable,
        CloseOnEscape = options.CloseOnEscape,
        DismissableMask = options.DismissableMask,
        FocusTrap = options.FocusTrap,
        AutoZIndex = options.AutoZIndex,
        DefaultFocus = options.DefaultFocus ?? DefaultFocus,
        Position = options.Position ?? Position,
        Style = options.Style ?? Style,
        StyleClass = options.StyleClass ?? StyleClass,
        MaskStyleClass = options.MaskStyleClass ?? MaskStyleClass,
        CloseIcon = options.CloseIcon ?? CloseIcon,
        CloseAriaLabel = options.CloseAriaLabel ?? CloseAriaLabel,
        AcceptAriaLabel = options.AcceptAriaLabel ?? AcceptAriaLabel,
        RejectAriaLabel = options.RejectAriaLabel ?? RejectAriaLabel,
        BaseZIndex = options.BaseZIndex != 0 ? options.BaseZIndex : BaseZIndex,
        Accept = options.Accept,
        Reject = options.Reject,
        OnClose = options.OnClose,
        MessageTemplate = options.MessageTemplate ?? MessageTemplate,
        HeaderTemplate = options.HeaderTemplate ?? HeaderTemplate,
        FooterTemplate = options.FooterTemplate ?? FooterTemplate,
        IconTemplate = options.IconTemplate ?? IconTemplate,
        HeadlessTemplate = options.HeadlessTemplate ?? HeadlessTemplate,
        Appearance = options.Appearance ?? Appearance
    };

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (_visible && !_lastRenderedVisible)
        {
            if (FocusTrap)
            {
                try
                {
                    // Trap + foco inicial no botão indicado por DefaultFocus (Accept/Reject/Close).
                    // "none" (ou valor desconhecido) não foca nada; o usuário entra pelo Tab.
                    await FocusTrapInitAsync(_root, DefaultFocusSelector(CurrentOptions?.DefaultFocus ?? DefaultFocus));
                }
                catch
                {
                }
            }
        }
        _lastRenderedVisible = _visible;
    }

    private static string? DefaultFocusSelector(string? mode) => mode switch
    {
        "accept" => "[data-pc-focus=\"accept\"]",
        "reject" => "[data-pc-focus=\"reject\"]",
        "close" => "[data-pc-focus=\"close\"]",
        _ => null
    };

    public void ShowDeclarative()
    {
        var options = new ConfirmOptions
        {
            Key = Key,
            Header = Header,
            Message = Message,
            Icon = Icon,
            AcceptLabel = AcceptLabel,
            RejectLabel = RejectLabel,
            AcceptIcon = AcceptIcon,
            RejectIcon = RejectIcon,
            AcceptButtonStyleClass = AcceptButtonStyleClass,
            RejectButtonStyleClass = RejectButtonStyleClass,
            AcceptButtonProps = AcceptButtonProps,
            RejectButtonProps = RejectButtonProps,
            AcceptVisible = AcceptVisible,
            RejectVisible = RejectVisible,
            Modal = Modal,
            Closable = Closable,
            CloseOnEscape = CloseOnEscape,
            DismissableMask = DismissableMask,
            Position = Position,
            Style = Style,
            StyleClass = StyleClass,
            MaskStyleClass = MaskStyleClass,
            CloseIcon = CloseIcon,
            CloseAriaLabel = CloseAriaLabel,
            AcceptAriaLabel = AcceptAriaLabel,
            RejectAriaLabel = RejectAriaLabel,
            AutoZIndex = AutoZIndex,
            BaseZIndex = BaseZIndex,
            FocusTrap = FocusTrap,
            DefaultFocus = DefaultFocus,
            MessageTemplate = MessageTemplate,
            HeaderTemplate = HeaderTemplate,
            FooterTemplate = FooterTemplate,
            IconTemplate = IconTemplate,
            HeadlessTemplate = HeadlessTemplate,
            Appearance = Appearance,
        };
        ConfirmationService.ConfirmDeclarative(options);
    }

    private void OnAcceptClick()
    {
        ConfirmationService.Accept();
        Close();
        _ = OnHide.InvokeAsync(new ConfirmEvent(true));
        StateHasChanged();
    }

    private void OnRejectClick()
    {
        ConfirmationService.Reject();
        Close();
        _ = OnHide.InvokeAsync(new ConfirmEvent(false));
        StateHasChanged();
    }

    private void OnCloseClick()
    {
        ConfirmationService.Close(false);
        Close();
        _ = OnHide.InvokeAsync(new ConfirmEvent(false));
        StateHasChanged();
    }

    private void Close()
    {
        // Dispara a restauração do foco antes de o diálogo sair do DOM (o elemento precisa
        // ainda estar anexado para ser serializado ao JS).
        if (FocusTrap && _visible)
        {
            _ = FocusTrapDisposeAsync(_root);
        }

        _visible = false;
        _resolved = null;
    }

    private void OnMaskClick()
    {
        if (DismissableMask)
        {
            OnCloseClick();
        }
    }

    private void OnKeydown(KeyboardEventArgs e)
    {
        if (CloseOnEscape && e.Key == "Escape")
        {
            OnCloseClick();
        }
    }

    public override async ValueTask DisposeAsync()
    {
        ConfirmationService.Unsubscribe(OnConfirmRequested);
        await base.DisposeAsync();
    }

    private ConfirmOptions? CurrentOptions => _resolved;

    private string RootClass => OpCss.BuildClass(
        "p-confirmdialog p-component p-dialog",
        "p-dialog-enter-active",
        AppearanceClass,
        Position != "center" ? $"p-dialog-{Position}" : null,
        StyleClass);

    // "Badge" é extensão do OpBlazorUI: o CSS vive em optimus-base.css e usa tokens do tema.
    private string? AppearanceClass =>
        string.IsNullOrWhiteSpace(CurrentOptions?.Appearance ?? Appearance)
            ? null
            : $"p-confirmdialog-{(CurrentOptions?.Appearance ?? Appearance).ToLowerInvariant()}";

    private string MaskClass => OpCss.BuildClass(
        "p-dialog-mask p-confirmdialog-mask",
        Modal ? "p-overlay-mask p-overlay-mask-enter-active" : null,
        Position != "center" ? $"p-dialog-{Position}" : null,
        MaskStyleClass);

    private string MaskInlineStyle => $"pointer-events: {(Modal ? "auto" : "none")};";

    private string RootInlineStyle => $"pointer-events: auto; {Style}".TrimEnd();

    private string? MessageDescribedBy => MessageTemplate != null ? null : _messageId;

    private string? IconValue => CurrentOptions?.Icon ?? Icon;

    private string MessageValue => CurrentOptions?.Message ?? Message ?? string.Empty;

    // O tema estiliza o elemento pelo .p-confirmdialog-icon; o glyph entra na mesma classe.
    private string IconClass => OpCss.BuildClass("p-confirmdialog-icon", IconValue);

    private string AcceptButtonClass => OpCss.BuildClass(
        "p-confirmdialog-accept-button",
        CurrentOptions?.AcceptButtonStyleClass ?? AcceptButtonStyleClass);

    private string RejectButtonClass => OpCss.BuildClass(
        "p-confirmdialog-reject-button",
        CurrentOptions?.RejectButtonStyleClass ?? RejectButtonStyleClass);

    private string? GetSeverity(Dictionary<string, object>? props) =>
        props?.TryGetValue("severity", out var v) == true ? v?.ToString() : null;

    private bool GetOutlined(Dictionary<string, object>? props) =>
        props?.TryGetValue("outlined", out var v) == true && v is bool b && b;

    private bool GetText(Dictionary<string, object>? props) =>
        props?.TryGetValue("text", out var v) == true && v is bool b && b;

    private bool GetRaised(Dictionary<string, object>? props) =>
        props?.TryGetValue("raised", out var v) == true && v is bool b && b;

    private bool GetRounded(Dictionary<string, object>? props) =>
        props?.TryGetValue("rounded", out var v) == true && v is bool b && b;

    private string? GetVariant(Dictionary<string, object>? props) =>
        props?.TryGetValue("variant", out var v) == true ? v?.ToString() : null;
}