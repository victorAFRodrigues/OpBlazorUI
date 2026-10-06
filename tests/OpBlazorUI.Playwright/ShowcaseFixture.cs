using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using Microsoft.Playwright;
using Xunit;

namespace OpBlazorUI.Playwright;

/// <summary>
/// Sobe o Showcase (WASM) e o Chromium uma vez por coleção. Requer os navegadores do
/// Playwright instalados: <c>pwsh bin/Debug/net10.0/playwright.ps1 install chromium</c>
/// (ou <c>node .playwright/package/cli.js install chromium</c>).
/// Usa uma porta livre por execução para não conflitar com instâncias anteriores.
/// </summary>
public sealed class ShowcaseFixture : IAsyncLifetime
{
    private Process? _server;
    private IPlaywright? _playwright;
    private IBrowser? _browser;

    public string BaseUrl { get; private set; } = "";

    public IPlaywright Playwright => _playwright!;
    public IBrowser Browser => _browser!;

    public async Task InitializeAsync()
    {
        var port = GetFreePort();
        BaseUrl = $"http://localhost:{port}";

        _server = StartServer(BaseUrl);
        await WaitForServerAsync(TimeSpan.FromSeconds(180));

        _playwright = await Microsoft.Playwright.Playwright.CreateAsync();
        _browser = await _playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions { Headless = true });
    }

    private static int GetFreePort()
    {
        var listener = new TcpListener(IPAddress.Loopback, 0);
        listener.Start();
        var port = ((IPEndPoint)listener.LocalEndpoint).Port;
        listener.Stop();
        return port;
    }

    private static Process StartServer(string baseUrl)
    {
        var root = FindRepositoryRoot();
        var csproj = Path.Combine(root, "src", "OpBlazorUI.Showcase", "OpBlazorUI.Showcase.csproj");

        var psi = new ProcessStartInfo("dotnet", $"run --project \"{csproj}\" --no-launch-profile")
        {
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
        };
        psi.Environment["ASPNETCORE_URLS"] = baseUrl;
        psi.Environment["DOTNET_CLI_TELEMETRY_OPTOUT"] = "1";

        var process = Process.Start(psi) ?? throw new InvalidOperationException("Falha ao iniciar o Showcase.");

        // Drena os streams: sem isso o processo trava ao encher o buffer do pipe.
        process.OutputDataReceived += (_, _) => { };
        process.ErrorDataReceived += (_, _) => { };
        process.BeginOutputReadLine();
        process.BeginErrorReadLine();

        return process;
    }

    private static string FindRepositoryRoot()
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir is not null && !File.Exists(Path.Combine(dir.FullName, "OpBlazorUI.slnx")))
        {
            dir = dir.Parent;
        }

        return dir?.FullName ?? throw new InvalidOperationException("Raiz do repositório não encontrada.");
    }

    private async Task WaitForServerAsync(TimeSpan timeout)
    {
        using var client = new HttpClient { Timeout = TimeSpan.FromSeconds(10) };
        var deadline = DateTime.UtcNow + timeout;

        while (DateTime.UtcNow < deadline)
        {
            try
            {
                // Só está pronto quando o _framework é servido (WASM montado).
                using var response = await client.GetAsync($"{BaseUrl}/_framework/dotnet.js");
                if (response.IsSuccessStatusCode)
                {
                    return;
                }
            }
            catch
            {
                // servidor ainda subindo
            }

            await Task.Delay(1000);
        }

        throw new TimeoutException($"O Showcase não ficou pronto em {BaseUrl} a tempo.");
    }

    public async Task DisposeAsync()
    {
        if (_browser is not null)
        {
            await _browser.CloseAsync();
        }

        _playwright?.Dispose();

        if (_server is { HasExited: false })
        {
            try
            {
                _server.Kill(entireProcessTree: true);
                _server.WaitForExit(10000);
            }
            catch
            {
                // ignore
            }
        }

        _server?.Dispose();
    }
}

[CollectionDefinition("showcase")]
public sealed class ShowcaseCollection : ICollectionFixture<ShowcaseFixture>
{
}
