using Microsoft.Extensions.DependencyInjection;
using OpBlazorUI.Globalization;
using OpBlazorUI.Services;

namespace OpBlazorUI;

public static class OpServiceCollectionExtensions
{
    /// <summary>
    /// Registra os serviços do OpBlazorUI. O tema e a tradução iniciais podem ser configurados:
    /// <code>
    /// builder.Services.AddOpBlazorUI(
    ///     theme => { theme.Preset = "lara"; theme.Primary = "blue"; },
    ///     OpTranslations.PtBr);
    /// </code>
    /// Sem tradução informada, o padrão é o inglês (<see cref="OpTranslations.EnUs"/>).
    /// </summary>
    public static IServiceCollection AddOpBlazorUI(
        this IServiceCollection services,
        Action<OpThemeOptions>? configureTheme = null,
        OpTranslation? translation = null)
    {
        var options = new OpThemeOptions();
        configureTheme?.Invoke(options);
        services.AddSingleton(options);
        services.AddSingleton(translation ?? OpTranslations.EnUs);
        services.AddScoped<OpThemeService>();
        services.AddScoped<OpTranslationService>();
        services.AddScoped<OpMessageService>();
        services.AddScoped<OpConfirmationService>();
        services.AddScoped<OpFilterService>();
        services.AddScoped<OpDialogService>();
        return services;
    }
}
