namespace OpBlazorUI.DocsGen;

/// <summary>
/// Conteúdo curado de instalação/configuração — o que um agente precisa para
/// colocar a lib numa aplicação antes de usar qualquer componente.
/// </summary>
internal static class GettingStarted
{
    public static GettingStartedManifest Create() => new(
        new PackageManifest(
            "OpBlazorUI",
            "OpBlazorUI.Charts",
            "https://nuget.pkg.github.com/victorAFRodrigues/index.json",
            true,
            """
            <?xml version="1.0" encoding="utf-8"?>
            <configuration>
                <packageSources>
                    <add key="nuget.org" value="https://api.nuget.org/v3/index.json" />
                    <add key="github" value="https://nuget.pkg.github.com/victorAFRodrigues/index.json" />
                </packageSources>
                <packageSourceCredentials>
                    <github>
                        <add key="Username" value="SEU_USUARIO_GITHUB" />
                        <add key="ClearTextPassword" value="SEU_PAT" />
                    </github>
                </packageSourceCredentials>
                <packageSourceMapping>
                    <packageSource key="nuget.org">
                        <package pattern="*" />
                    </packageSource>
                    <packageSource key="github">
                        <package pattern="OpBlazorUI.*" />
                    </packageSource>
                </packageSourceMapping>
            </configuration>
            """,
            "dotnet add package OpBlazorUI"),
        new ConfigureManifest(
            """
            builder.Services.AddOpBlazorUI();

            // Opcional: gráficos (pacote OpBlazorUI.Charts)
            builder.Services.AddOpBlazorCharts();
            """,
            """
            @using OpBlazorUI
            @using OpBlazorUI.Components.Button
            """,
            """
            <head>
                <base href="/" />
                <script src="_content/OpBlazorUI/op-theme.js" data-preset="aura" data-primary="noir"></script>
                <HeadOutlet @rendermode="InteractiveAuto" />
            </head>
            <body>
                <Routes @rendermode="InteractiveAuto" />
            </body>
            """,
            """
            @* Precisa estar dentro da parte interativa da aplicação. *@
            <OpBlazorUiSetup />
            """),
        [
            "O pacote é publicado no GitHub Packages: o restore exige um PAT com escopo read:packages, mesmo para pacotes públicos.",
            "O tema é aplicado por JS antes da primeira pintura; os atributos data-preset/data-primary repetem o tema registrado no AddOpBlazorUI.",
            "Funciona em Blazor Web App (Server, WebAssembly e Auto) e em SSR estático: o estado é sempre scoped, nunca static.",
            "Os componentes renderizam as classes p-* do Optimus UI/PrimeNG. Utilitários de layout (flex, grid, espaçamento) são responsabilidade da aplicação.",
            "Ícones usam a fonte nativa da lib: <i class=\"pi pi-*\"></i>."
        ]);
}
