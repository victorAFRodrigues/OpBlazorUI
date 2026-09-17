using Microsoft.Extensions.DependencyInjection;
using OpBlazorUI.Base.Services;

namespace OpBlazorUI.Base;

public static class OpServiceCollectionExtensions
{
    public static IServiceCollection AddOpBlazorUI(this IServiceCollection services)
    {
        services.AddScoped<OpMessageService>();
        return services;
    }
}
