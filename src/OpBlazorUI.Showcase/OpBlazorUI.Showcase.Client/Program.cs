using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using OpBlazorUI.Base;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

builder.Services.AddOpBlazorUI();

await builder.Build().RunAsync();