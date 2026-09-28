using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using OpBlazorUI.Base;
using OpBlazorUI.Showcase;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

// Showcase: primária noir (padrão da lib), superfície slate também no modo escuro e escolhas
// do usuário salvas no localStorage (aplicadas antes do boot por op-theme.js, no index.html).
builder.Services.AddOpBlazorUI(theme =>
{
    theme.Surface = OpBlazorUI.Showcase.Components.Layout.OpConfig.DefaultSurface;
    theme.PersistTheme = true;
});

await builder.Build().RunAsync();