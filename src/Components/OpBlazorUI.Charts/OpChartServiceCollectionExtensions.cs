using ApexCharts;
using Microsoft.Extensions.DependencyInjection;

namespace OpBlazorUI.Charts;

public static class OpChartServiceCollectionExtensions
{
    /// <summary>
    /// Registra o serviço de gráficos (Blazor-ApexCharts). As cores padrão são lidas dos tokens do
    /// tema pelo <c>OpChart</c> em tempo de execução.
    /// </summary>
    public static IServiceCollection AddOpBlazorCharts(this IServiceCollection services)
    {
        services.AddApexCharts();
        return services;
    }
}
