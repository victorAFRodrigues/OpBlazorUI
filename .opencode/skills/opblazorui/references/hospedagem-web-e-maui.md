# Hospedagem: Blazor Web App, WebAssembly, MAUI e SSR

Exemplos mínimos por modelo de hospedagem. Em todos os modelos interativos, importe os namespaces
do componente que for usar e registre os serviços com `AddOpBlazorUI()`. O padrão do tema é
**claro** (`OpDarkMode.Light`); o `data-dark` do script deve refletir essa escolha.

## Blazor Web App — Interactive Server

`Program.cs`:

```csharp
using OpBlazorUI;
using OpBlazorUI.Globalization;
using OpBlazorUI.Services;
using SeuApp.Components;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddOpBlazorUI(
    theme =>
    {
        theme.Preset = "aura";
        theme.Primary = "noir";
        theme.Surface = "slate";
        theme.DarkMode = OpDarkMode.Light;
        theme.PersistTheme = true;
    },
    OpTranslations.PtBr);

var app = builder.Build();

app.UseHttpsRedirection();
app.UseAntiforgery();
app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
```

`Components/App.razor`:

```razor
<!DOCTYPE html>
<html lang="pt-BR">
<head>
    <base href="/" />
    <script src="_content/OpBlazorUI/op-theme.js"
            data-preset="aura" data-primary="noir" data-surface="slate" data-dark="light" data-persist="true"></script>
    <HeadOutlet @rendermode="InteractiveServer" />
</head>
<body>
    <Routes @rendermode="InteractiveServer" />
    <script src="_framework/blazor.web.js"></script>
</body>
</html>
```

`Components/Layout/MainLayout.razor`:

```razor
@inherits LayoutComponentBase

<OpBlazorUiSetup />
<OpToast Key="app" />
<OpConfirmDialog Key="app" />
<OpDynamicDialogHost />

<main>
    @Body
</main>
```

## Blazor Web App — Interactive WebAssembly

No servidor:

```csharp
using OpBlazorUI;
using OpBlazorUI.Globalization;
using OpBlazorUI.Services;
using SeuApp.Components;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorComponents()
    .AddInteractiveWebAssemblyComponents();

// Também no servidor: o prerender usa estes serviços.
builder.Services.AddOpBlazorUI(
    theme =>
    {
        theme.Preset = "aura";
        theme.Primary = "noir";
        theme.Surface = "slate";
        theme.DarkMode = OpDarkMode.Light;
        theme.PersistTheme = true;
    },
    OpTranslations.PtBr);

var app = builder.Build();

app.UseHttpsRedirection();
app.UseAntiforgery();
app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveWebAssemblyRenderMode()
    .AddAdditionalAssemblies(typeof(SeuApp.Client._Imports).Assembly);

app.Run();
```

No projeto `.Client` (`Program.cs`):

```csharp
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using OpBlazorUI;
using OpBlazorUI.Globalization;
using OpBlazorUI.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

builder.Services.AddOpBlazorUI(
    theme =>
    {
        theme.Preset = "aura";
        theme.Primary = "noir";
        theme.Surface = "slate";
        theme.DarkMode = OpDarkMode.Light;
        theme.PersistTheme = true;
    },
    OpTranslations.PtBr);

await builder.Build().RunAsync();
```

`App.razor` do servidor:

```razor
<head>
    <base href="/" />
    <script src="_content/OpBlazorUI/op-theme.js"
            data-preset="aura" data-primary="noir" data-surface="slate" data-dark="light" data-persist="true"></script>
    <HeadOutlet @rendermode="InteractiveWebAssembly" />
</head>
<body>
    <Routes @rendermode="InteractiveWebAssembly" />
    <script src="_framework/blazor.web.js"></script>
</body>
```

`Components/Layout/MainLayout.razor` no projeto `.Client`:

```razor
@inherits LayoutComponentBase

<OpBlazorUiSetup />
<OpToast Key="app" />
<OpConfirmDialog Key="app" />
<OpDynamicDialogHost />

<main>
    @Body
</main>
```

O `AddOpBlazorUI()` precisa estar nos **dois** `Program.cs`, com as mesmas opções.

## Blazor Web App — Interactive Auto

No servidor, registre **os dois** modos e mapeie os dois render modes. No `.Client`, configure os
serviços com as mesmas opções:

```csharp
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using OpBlazorUI;
using OpBlazorUI.Globalization;
using OpBlazorUI.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

builder.Services.AddOpBlazorUI(
    theme =>
    {
        theme.Preset = "aura";
        theme.Primary = "noir";
        theme.Surface = "slate";
        theme.DarkMode = OpDarkMode.Light;
        theme.PersistTheme = true;
    },
    OpTranslations.PtBr);

await builder.Build().RunAsync();
```

```csharp
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents()
    .AddInteractiveWebAssemblyComponents();

// ...

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode()
    .AddInteractiveWebAssemblyRenderMode()
    .AddAdditionalAssemblies(typeof(SeuApp.Client._Imports).Assembly);
```

```razor
<head>
    <base href="/" />
    <script src="_content/OpBlazorUI/op-theme.js"
            data-preset="aura" data-primary="noir" data-surface="slate" data-dark="light" data-persist="true"></script>
    <HeadOutlet @rendermode="InteractiveAuto" />
</head>
<body>
    <Routes @rendermode="InteractiveAuto" />
    <script src="_framework/blazor.web.js"></script>
</body>
```

A primeira visita roda em Server e as seguintes em WebAssembly. O estado do serviço não passa de
um runtime para o outro; `PersistTheme = true` restaura o tema salvo nos dois.

`Components/Layout/MainLayout.razor` no projeto `.Client`:

```razor
@inherits LayoutComponentBase

<OpBlazorUiSetup />
<OpToast Key="app" />
<OpConfirmDialog Key="app" />
<OpDynamicDialogHost />

<main>
    @Body
</main>
```

## WebAssembly standalone

`Program.cs`:

```csharp
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using OpBlazorUI;
using OpBlazorUI.Globalization;
using OpBlazorUI.Services;
using SeuApp;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddOpBlazorUI(
    theme =>
    {
        theme.Preset = "aura";
        theme.Primary = "noir";
        theme.Surface = "slate";
        theme.DarkMode = OpDarkMode.Light;
        theme.PersistTheme = true;
    },
    OpTranslations.PtBr);

await builder.Build().RunAsync();
```

`wwwroot/index.html`:

```html
<!DOCTYPE html>
<html lang="pt-BR">
<head>
    <meta charset="utf-8" />
    <meta name="viewport" content="width=device-width, initial-scale=1.0" />
    <title>SeuApp</title>
    <base href="/" />
    <script src="_content/OpBlazorUI/op-theme.js"
            data-preset="aura" data-primary="noir" data-surface="slate" data-dark="light" data-persist="true"></script>
</head>
<body>
    <div id="app">Carregando...</div>
    <script src="_framework/blazor.webassembly.js"></script>
</body>
</html>
```

`App.razor`:

```razor
<OpBlazorUiSetup />

<Router AppAssembly="typeof(Program).Assembly">
    <Found Context="routeData">
        <RouteView RouteData="routeData" DefaultLayout="typeof(Layout.MainLayout)" />
        <FocusOnNavigate RouteData="routeData" Selector="h1" />
    </Found>
</Router>
```

O `HeadOutlet` em `builder.RootComponents` é o que permite ao setup injetar os `<link>` de CSS no
`<head>`. Não coloque o setup no `index.html`.

## MAUI Blazor Hybrid

O template MAUI **não inclui `HeadOutlet`**. Por isso, os CSS fixos da lib e o `op-theme.js` ficam
no `wwwroot/index.html`; o `<OpBlazorUiSetup />` fica no `Routes.razor` ou no layout e cuida do tema
em runtime e do interop. Não use `@rendermode` no MAUI.

`MauiProgram.cs`:

```csharp
using Microsoft.Extensions.Logging;
using OpBlazorUI;
using OpBlazorUI.Globalization;
using OpBlazorUI.Services;

namespace SeuApp;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
            });

        builder.Services.AddMauiBlazorWebView();
        builder.Services.AddOpBlazorUI(
            theme =>
            {
                theme.Preset = "aura";
                theme.Primary = "noir";
                theme.Surface = "slate";
                theme.DarkMode = OpDarkMode.Light;
                theme.PersistTheme = true;
            },
            OpTranslations.PtBr);

#if DEBUG
        builder.Services.AddBlazorWebViewDeveloperTools();
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }
}
```

`wwwroot/index.html`:

```html
<!DOCTYPE html>
<html lang="pt-BR">
<head>
    <meta charset="utf-8" />
    <meta name="viewport" content="width=device-width, initial-scale=1.0, maximum-scale=1.0, user-scalable=no, viewport-fit=cover" />
    <base href="/" />

    <!-- O script entra antes dos CSS para o tema ser criado antes do CSS base. -->
    <script src="_content/OpBlazorUI/op-theme.js"
            data-preset="aura" data-primary="noir" data-surface="slate" data-dark="light" data-persist="true"></script>

    <!-- CSS fixo da lib: o template MAUI não tem HeadOutlet. -->
    <link rel="stylesheet" href="_content/OpBlazorUI/fonts/inter/inter.css" />
    <link rel="stylesheet" href="_content/OpBlazorUI/openng-icons.css" />
    <link rel="stylesheet" href="_content/OpBlazorUI/optimus-base.css" />
    <link rel="stylesheet" href="_content/OpBlazorUI/quill.snow.css" />
    <link rel="stylesheet" href="app.css" />
</head>
<body>
    <div id="app">Carregando...</div>
    <script src="_framework/blazor.webview.js" autostart="false"></script>
</body>
</html>
```

`Components/Routes.razor`:

```razor
<OpBlazorUiSetup />

<Router AppAssembly="typeof(MauiProgram).Assembly">
    <Found Context="routeData">
        <RouteView RouteData="routeData" DefaultLayout="typeof(Layout.MainLayout)" />
        <FocusOnNavigate RouteData="routeData" Selector="h1" />
    </Found>
</Router>
```

`Components/Layout/MainLayout.razor`:

```razor
@inherits LayoutComponentBase

<OpToast Key="app" />
<OpConfirmDialog Key="app" />
<OpDynamicDialogHost />

<main>
    @Body
</main>
```

No `MainPage.xaml` do template, o `RootComponent` já aponta para `Components.Routes`; não é preciso
adicionar `HeadOutlet`.

## SSR estático

Sem parte interativa, não use `<OpBlazorUiSetup />` nem `@rendermode`. O tema é fixo e vem do
`op-theme.js`; os CSS que o setup carregaria entram manualmente no `<head>`.

`Program.cs`:

```csharp
using OpBlazorUI;
using OpBlazorUI.Services;
using SeuApp.Components;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorComponents();
builder.Services.AddOpBlazorUI(theme =>
{
    theme.DarkMode = OpDarkMode.Light;
});

var app = builder.Build();

app.UseHttpsRedirection();
app.UseAntiforgery();
app.MapStaticAssets();
app.MapRazorComponents<App>();

app.Run();
```

`Components/App.razor`:

```razor
<!DOCTYPE html>
<html lang="pt-BR">
<head>
    <meta charset="utf-8" />
    <meta name="viewport" content="width=device-width, initial-scale=1.0" />
    <base href="/" />

    @* Sem setup interativo: o script aplica o tema e os CSS entram manualmente. *@
    <script src="_content/OpBlazorUI/op-theme.js"
            data-preset="aura" data-primary="noir" data-surface="slate"
            data-dark="light" data-persist="false"></script>
    <link rel="stylesheet" href="_content/OpBlazorUI/fonts/inter/inter.css" />
    <link rel="stylesheet" href="_content/OpBlazorUI/openng-icons.css" />
    <link rel="stylesheet" href="_content/OpBlazorUI/optimus-base.css" />
    <HeadOutlet />
</head>
<body>
    <Routes />
    <script src="_framework/blazor.web.js"></script>
</body>
</html>
```

Como não há setup para salvar escolhas, o exemplo usa `data-persist="false"` e ignora qualquer
estado anterior no `localStorage`.

## Tipografia

A lib empacota a **Inter Variable 4.1** e usa **14px** como base do documento. O
`<OpBlazorUiSetup />` carrega `_content/OpBlazorUI/fonts/inter/inter.css`; o CSS base aplica a fonte
em `html` e os componentes herdam. A fonte é SIL OFL 1.1 e funciona offline.

Para trocar a família, sobrescreva a variável depois do CSS da lib:

```css
:root {
    --op-font-family: "Segoe UI", system-ui, sans-serif;
}
```

Opcionalmente, ajuste o tamanho base (`html { font-size: 16px; }`). `--op-font-mono` define a pilha
monoespaçada.

## Checklist

- [ ] `AddOpBlazorUI()` no `Program.cs`/`MauiProgram.cs`; nos projetos WebAssembly/Auto, também no `.Client`.
- [ ] `theme.DarkMode` definido (`Light` é o padrão) e `data-dark` igual no script.
- [ ] `op-theme.js` no `<head>`, depois de `<base href>` (ou no `wwwroot/index.html` do MAUI/standalone).
- [ ] `<OpBlazorUiSetup />` dentro da parte interativa.
- [ ] `HeadOutlet` no `App.razor` (Blazor Web App/standalone) ou CSS fixo no `index.html` (MAUI).
- [ ] Hosts de overlay (`OpToast`, `OpConfirmDialog`, `OpDynamicDialogHost`) se usar overlays.
- [ ] No MAUI, sem `@rendermode`.
- [ ] No SSR estático, sem setup e com CSS manual.
