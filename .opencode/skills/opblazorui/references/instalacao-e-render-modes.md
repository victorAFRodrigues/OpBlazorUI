# Instalação e render modes

Como instalar o pacote, registrar os serviços, configurar o tema e escolher o render mode.

## 1. Adicionar o pacote

O pacote está no NuGet.org, a fonte padrão do `dotnet`; não é preciso configurar feed nem
credencial. O comando instala a versão estável mais recente; para fixar, use `--version <versão>`.

```bash
dotnet add package OpBlazorUI
# opcional: gráficos (Blazor-ApexCharts)
dotnet add package OpBlazorUI.Charts
```

## 2. Registrar os serviços

```csharp
// Program.cs
using OpBlazorUI;
using OpBlazorUI.Globalization;
// using OpBlazorUI.Charts;    // opcional: pacote OpBlazorUI.Charts

builder.Services.AddOpBlazorUI(
    theme =>
    {
        theme.Preset = "aura";          // aura (padrão), lara ou nora
        theme.Primary = "noir";         // paleta primária
        theme.Surface = "slate";        // paleta de superfícies (opcional)
        theme.Rtl = false;
        theme.PersistTheme = true;      // salva as escolhas no localStorage
    },
    OpTranslations.PtBr);               // opcional; padrão en-US

// Opcional: gráficos (pacote OpBlazorUI.Charts)
// builder.Services.AddOpBlazorCharts();
```

Para gráficos, instale `OpBlazorUI.Charts`, descomente `using OpBlazorUI.Charts;` e
`AddOpBlazorCharts()`. `AddOpBlazorUI()` registra `OpThemeService`, `OpTranslationService`,
`OpMessageService`, `OpConfirmationService`, `OpFilterService` e `OpDialogService`. Numa
Blazor Web App com WebAssembly ou Auto, chame nos dois `Program.cs` (servidor e `.Client`).

## 3. `_Imports.razor`

```razor
@using OpBlazorUI
@using OpBlazorUI.Components.Button
@using OpBlazorUI.Components.InputText
@using OpBlazorUI.Services
@using OpBlazorUI.Globalization
```

Cada componente vive no namespace `OpBlazorUI.Components.<Nome>`; importe o que usar. Modelos
como `OpToastMessage` ficam em `OpBlazorUI.Models`.

## 4. Tema e render mode

```razor
<head>
    <base href="/" />
    <script src="_content/OpBlazorUI/op-theme.js" data-preset="aura" data-primary="noir" data-surface="slate"></script>
    <HeadOutlet @rendermode="InteractiveAuto" />
</head>
<body>
    <Routes @rendermode="InteractiveAuto" />
</body>
```

O `op-theme.js` aplica o tema antes da primeira pintura, sem flash; os atributos `data-*`
repetem os padrões registrados no `AddOpBlazorUI`. Em WebAssembly standalone, o script fica no
`wwwroot/index.html`.

## 5. Setup no layout

```razor
@* Precisa estar dentro da parte interativa da aplicação. *@
<OpBlazorUiSetup />
```

Adicione uma única vez, no `App.razor` ou no `MainLayout`. Em WebAssembly standalone, o setup
fica no `App.razor` (ou no layout), nunca no `index.html`. O setup carrega a fonte Inter, os
ícones e o CSS base, e aplica as trocas de tema em runtime.

No MAUI Blazor Hybrid, o template não tem `HeadOutlet`: carregue no `wwwroot/index.html` o
`op-theme.js` e os CSS fixos (`fonts/inter/inter.css`, `openng-icons.css`, `optimus-base.css` e
`quill.snow.css`), monte o setup no `Routes.razor`/layout e não use `@rendermode`. Os exemplos
completos estão em [Hospedagem](hospedagem-web-e-maui.md).

Em SSR estático, sem parte interativa, inclua os mesmos links de CSS no `<head>`; o setup não roda
e o tema inicial fica por conta do `op-theme.js`.

## Tipografia

A lib empacota Inter Variable 4.1 e usa 14px como base do documento. Para trocar a família,
sobrescreva `--op-font-family` depois do CSS da lib; para mudar o tamanho, use
`html { font-size: ... }`. A fonte é SIL OFL 1.1 e funciona offline.

## 6. Overlays (opcional)

Toast, ConfirmDialog e DynamicDialog precisam de um host montado uma única vez no layout:

```razor
<OpToast Key="app" />
<OpConfirmDialog Key="app" />
<OpDynamicDialogHost />
```

O `Key` liga a chamada do serviço ao host.

## Render modes

- **InteractiveServer, InteractiveWebAssembly e InteractiveAuto**: funcionam sem mudanças; o
  estado da lib é sempre *scoped* (nunca `static`).
- **SSR estático**: a marcação renderiza, mas componentes com interação (overlays, foco,
  JS interop) precisam de uma parte interativa para funcionar. O gráfico também exige um
  render mode interativo.
- Não use estado `static` na sua aplicação para guardar dados de tela: isso vaza entre
  circuitos do Server e entre usuários no WASM.

## Primeiro componente

```razor
<OpButton Label="Salvar" Icon="pi pi-check" OnClick="Save" />
<OpInputText @bind-Value="_name" Placeholder="Nome" Fluid="true" />

@code {
    private string? _name;

    private void Save()
    {
        // ...
    }
}
```

## Checklist

- [ ] `dotnet add package OpBlazorUI` no projeto.
- [ ] `using OpBlazorUI;` e `AddOpBlazorUI()` registrados; `AddOpBlazorCharts()` se usar gráficos.
- [ ] Tradução definida (`OpTranslations.PtBr` é opcional).
- [ ] `op-theme.js` no `<head>`, depois do `<base href>` (ou no `index.html` standalone/MAUI).
- [ ] `<OpBlazorUiSetup />` na parte interativa do layout.
- [ ] Hosts de overlay (`OpToast`, `OpConfirmDialog`, `OpDynamicDialogHost`) se usar overlays.
- [ ] Import do namespace do componente que for usar.
- [ ] No MAUI, CSS fixo no `wwwroot/index.html` e setup no `Routes.razor`/layout, sem `@rendermode`.
