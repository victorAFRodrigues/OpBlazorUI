using Microsoft.AspNetCore.Components;
using OpBlazorUI.Base.Components.Common;

namespace OpBlazorUI.Base.Components.InlineMessage;

public partial class OpInlineMessage : OpComponentBase
{
    /// <summary>Severidade: <c>info</c>, <c>success</c>, <c>warn</c>, <c>error</c>, <c>secondary</c> ou <c>contrast</c>.</summary>
    [Parameter] public string Severity { get; set; } = "info";

    /// <summary>Texto exibido. Ignorado quando <see cref="ChildContent"/> é informado.</summary>
    [Parameter] public string? Text { get; set; }

    /// <summary>Ícone customizado; sem valor, usa o ícone-padrão da severidade.</summary>
    [Parameter] public string? Icon { get; set; }

    [Parameter] public bool ShowIcon { get; set; } = true;

    /// <summary>Quando <c>false</c>, o <see cref="Text"/> é renderizado como HTML.</summary>
    [Parameter] public bool Escape { get; set; } = true;

    [Parameter] public RenderFragment? ChildContent { get; set; }

    private bool HasContent => ChildContent is not null || !string.IsNullOrEmpty(Text);

    private string RootClass => Class(
        "p-inlinemessage p-component",
        $"p-inlinemessage-{Severity.ToLowerInvariant()}",
        !HasContent ? "p-inlinemessage-icon-only" : null,
        StyleClass);

    private string? EffectiveIcon => Icon ?? OpSeverityIcon.Get(Severity);
}
