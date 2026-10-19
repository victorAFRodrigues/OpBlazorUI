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
            "O pacote é publicado no NuGet.org, a fonte padrão do dotnet: o restore não exige feed nem credencial extra.",
            "O tema é aplicado por JS antes da primeira pintura; os atributos data-preset/data-primary repetem o tema registrado no AddOpBlazorUI.",
            "Funciona em Blazor Web App (Server, WebAssembly e Auto) e em SSR estático: o estado é sempre scoped, nunca static.",
            "Os componentes renderizam as classes p-* do Optimus UI/PrimeNG. Utilitários de layout (flex, grid, espaçamento) são responsabilidade da aplicação.",
            "Ícones usam a fonte nativa da lib: <i class=\"pi pi-*\"></i>."
        ]);
}
