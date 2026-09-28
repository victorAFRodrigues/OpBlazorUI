using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using OpBlazorUI.Base;
using OpBlazorUI.Showcase;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddOpBlazorUI();

// Showcase: primária noir (padrão da lib) com superfície slate também no modo escuro.
OpTheme.SetSurface(OpBlazorUI.Showcase.Components.Layout.OpConfig.DefaultSurface);

await builder.Build().RunAsync();