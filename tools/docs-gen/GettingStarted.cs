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
            using OpBlazorUI;
            using OpBlazorUI.Globalization;
            // using OpBlazorUI.Charts; // opcional: pacote OpBlazorUI.Charts

            builder.Services.AddOpBlazorUI(
                theme =>
                {
                    theme.Preset = "aura";
                    theme.Primary = "noir";
                    theme.Surface = "slate";
                    theme.PersistTheme = true;
                },
                OpTranslations.PtBr); // opcional; padrão en-US

            // Opcional: gráficos (pacote OpBlazorUI.Charts)
            // builder.Services.AddOpBlazorCharts();
            """,
            """
            @using OpBlazorUI
            @using OpBlazorUI.Components.Button
            @using OpBlazorUI.Services
            @using OpBlazorUI.Globalization
            """,
            """
            <head>
                <base href="/" />
                <script src="_content/OpBlazorUI/op-theme.js" data-preset="aura" data-primary="noir" data-surface="slate"></script>
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
            "O pacote é publicado no NuGet.org, a fonte padrão do dotnet: o restore não exige feed nem credencial extra. A versão atual está no NuGet.org.",
            "O tema é aplicado por JS antes da primeira pintura; os atributos data-preset/data-primary/data-surface repetem o tema registrado no AddOpBlazorUI.",
            "Funciona em Blazor Web App (Server, WebAssembly e Auto), WebAssembly standalone, .NET MAUI Blazor Hybrid e em SSR estático: o estado é sempre scoped, nunca static. Componentes interativos exigem um render mode interativo.",
            "AddOpBlazorUI registra tema, tradução, mensagens, confirmação, filtro e dialog dinâmico; o segundo parâmetro opcional define a tradução (padrão en-US). O using OpBlazorUI é necessário para a extensão.",
            "Numa Blazor Web App com WebAssembly ou Auto, chame AddOpBlazorUI nos dois Program.cs (servidor e .Client). No Auto, o servidor registra AddInteractiveServerComponents e AddInteractiveWebAssemblyComponents e mapeia os dois render modes; use InteractiveAuto no HeadOutlet e Routes.",
            "Para Toast, ConfirmDialog e DynamicDialog, monte uma vez no layout: <OpToast Key=\"app\" />, <OpConfirmDialog Key=\"app\" /> e <OpDynamicDialogHost />.",
            "Em WebAssembly standalone, o op-theme.js fica no wwwroot/index.html e o <OpBlazorUiSetup /> no App.razor ou no layout; registre o HeadOutlet em builder.RootComponents.",
            "Em SSR estático, sem parte interativa, inclua fonts/inter/inter.css, openng-icons.css e optimus-base.css manualmente no <head>; o tema inicial vem do op-theme.js.",
            "No MAUI Blazor Hybrid não há HeadOutlet: carregue fonts/inter/inter.css, openng-icons.css, optimus-base.css, quill.snow.css e op-theme.js no wwwroot/index.html; use <OpBlazorUiSetup /> no Routes.razor/layout e não use @rendermode.",
            "A lib empacota Inter Variable 4.1 e usa 14px como base do documento. Para trocar a tipografia, sobrescreva --op-font-family; para o tamanho, html { font-size: ... }.",
            "Os componentes renderizam as classes p-* do Optimus UI/PrimeNG. Utilitários de layout (flex, grid, espaçamento) são responsabilidade da aplicação.",
            "Ícones usam a fonte nativa da lib: <i class=\"pi pi-*\"></i>."
        ]);
}
