using OpBlazorUI.Base.Globalization;

namespace OpBlazorUI.Base.Services;

/// <summary>
/// Estado da tradução dos componentes (scoped: cada circuito do Blazor Server tem o seu), definido
/// no setup pelo <see cref="OpBlazorUI.Base.OpServiceCollectionExtensions.AddOpBlazorUI"/> ou trocado
/// em runtime com <see cref="SetTranslation"/>. Os componentes leem as strings daqui em vez de
/// dependerem da cultura/ICU da plataforma.
/// </summary>
public sealed class OpTranslationService
{
    public OpTranslationService(OpTranslation translation) => Translation = translation;

    /// <summary>Tradução corrente.</summary>
    public OpTranslation Translation { get; private set; }

    /// <summary>Disparado quando a tradução muda em runtime.</summary>
    public event Action? Changed;

    public void SetTranslation(OpTranslation translation)
    {
        Translation = translation;
        Changed?.Invoke();
    }
}
