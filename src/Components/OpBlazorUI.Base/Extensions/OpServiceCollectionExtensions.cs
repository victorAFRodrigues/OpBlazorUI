using Microsoft.Extensions.DependencyInjection;
using OpBlazorUI.Base.Services;

namespace OpBlazorUI.Base;

public static class OpServiceCollectionExtensions
{
    /// <summary>
    /// Registra os serviços do OpBlazorUI. O tema inicial pode ser configurado:
    /// <code>builder.Services.AddOpBlazorUI(o => { o.Preset = "lara"; o.Primary = "blue"; });</code>
    /// </summary>
    public static IServiceCollection AddOpBlazorUI(this IServiceCollection services, Action<OpThemeOptions>? configureTheme = null)
    {
        var options = new OpThemeOptions();
        configureTheme?.Invoke(options);
        services.AddSingleton(options);
        services.AddScoped<OpThemeService>();
        services.AddScoped<OpMessageService>();
        services.AddScoped<OpConfirmationService>();
        services.AddScoped<OpFilterService>();
        return services;
    }
}
