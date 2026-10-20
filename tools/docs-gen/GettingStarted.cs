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
            using OpBlazorUI.Services;
            using OpBlazorUI.Globalization;
            // using OpBlazorUI.Charts; // opcional: pacote OpBlazorUI.Charts

            builder.Services.AddOpBlazorUI(
                theme =>
                {
                    theme.Preset = "aura";
                    theme.Primary = "noir";
                    theme.Surface = "slate";
                    theme.DarkMode = OpDarkMode.Light; // padrão; Dark ou System
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
            @* Exemplo de Blazor Web App. Troque InteractiveAuto pelo render mode do seu modelo. *@
            <head>
                <base href="/" />
                <script src="_content/OpBlazorUI/op-theme.js"
                        data-preset="aura" data-primary="noir" data-surface="slate"
                        data-dark="light" data-persist="true"></script>
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
            "O tema é aplicado por JS antes da primeira pintura; data-preset/data-primary/data-surface repetem o tema registrado no AddOpBlazorUI. O padrão é claro: data-dark aceita light, dark ou system e deve refletir OpDarkMode.",
            "Funciona em Blazor Web App (Server, WebAssembly e Auto), WebAssembly standalone, .NET MAUI Blazor Hybrid e em SSR estático: o estado é sempre scoped, nunca static. Componentes interativos exigem um render mode interativo.",
            "AddOpBlazorUI registra tema, tradução, mensagens, confirmação, filtro e dialog dinâmico; o segundo parâmetro opcional define a tradução (padrão en-US). O using OpBlazorUI é necessário para a extensão.",
            "Blazor Web App Server: registre AddOpBlazorUI no Program.cs do servidor, use InteractiveServer no HeadOutlet/Routes e monte o setup no MainLayout.",
            "Blazor Web App WebAssembly: registre AddOpBlazorUI nos dois Program.cs (servidor e .Client), use InteractiveWebAssembly no App.razor e monte o setup no MainLayout do .Client.",
            "Blazor Web App Auto: além dos dois Program.cs, o servidor registra AddInteractiveServerComponents + AddInteractiveWebAssemblyComponents e mapeia os dois render modes; use InteractiveAuto no HeadOutlet/Routes e o setup no MainLayout do .Client.",
            "Com PersistTheme = true, a escolha salva no localStorage vence o padrão. Para forçar claro/escuro sem switcher, use DarkMode + data-dark iguais, PersistTheme = false e data-persist=\"false\".",
            "Para Toast, ConfirmDialog e DynamicDialog, monte uma vez no layout: <OpToast Key=\"app\" />, <OpConfirmDialog Key=\"app\" /> e <OpDynamicDialogHost />.",
            "Em WebAssembly standalone, o op-theme.js fica no wwwroot/index.html e o <OpBlazorUiSetup /> no App.razor ou no layout; registre o HeadOutlet em builder.RootComponents.",
            "Em SSR estático, não use <OpBlazorUiSetup />: inclua fonts/inter/inter.css, openng-icons.css e optimus-base.css manualmente no <head>; o tema fixo vem do op-theme.js com data-persist=\"false\".",
            "No MAUI Blazor Hybrid não há HeadOutlet: carregue fonts/inter/inter.css, openng-icons.css, optimus-base.css, quill.snow.css e op-theme.js no wwwroot/index.html; use <OpBlazorUiSetup /> no Routes.razor/layout e não use @rendermode.",
            "A lib empacota Inter Variable 4.1 e usa 14px como base do documento. Para trocar a tipografia, sobrescreva --op-font-family; para o tamanho, html { font-size: ... }.",
            "Os componentes renderizam as classes p-* do Optimus UI/PrimeNG. Utilitários de layout (flex, grid, espaçamento) são responsabilidade da aplicação.",
            "Ícones usam a fonte nativa da lib: <i class=\"pi pi-*\"></i>."
        ]);
}
