# Instalação e render modes

Como instalar o pacote, registrar os serviços, configurar o tema e escolher o modelo de projeto.
Os exemplos completos de cada arquivo estão em
[Hospedagem: Blazor Web App, WebAssembly e MAUI](hospedagem-web-e-maui.md).

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
// Program.cs (ou MauiProgram.cs no MAUI)
using OpBlazorUI;
using OpBlazorUI.Services;
using OpBlazorUI.Globalization;
// using OpBlazorUI.Charts;    // opcional: pacote OpBlazorUI.Charts

builder.Services.AddOpBlazorUI(
    theme =>
    {
        theme.Preset = "aura";             // aura (padrão), lara ou nora
        theme.Primary = "noir";            // paleta primária
        theme.Surface = "slate";           // paleta de superfícies (opcional)
        theme.DarkMode = OpDarkMode.Light; // padrão; Dark ou System
        theme.Rtl = false;
        theme.PersistTheme = true;         // salva as escolhas no localStorage
    },
    OpTranslations.PtBr);                  // opcional; padrão en-US

// Opcional: gráficos (pacote OpBlazorUI.Charts)
// builder.Services.AddOpBlazorCharts();
```

Para gráficos, instale `OpBlazorUI.Charts`, descomente `using OpBlazorUI.Charts;` e
`AddOpBlazorCharts()`. `AddOpBlazorUI()` registra `OpThemeService`, `OpTranslationService`,
`OpMessageService`, `OpConfirmationService`, `OpFilterService` e `OpDialogService`.

Numa Blazor Web App com WebAssembly ou Auto, chame `AddOpBlazorUI()` nos **dois** `Program.cs`
(servidor e `.Client`), com as mesmas opções.

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

## 4. Escolha o modelo

| Modelo | `Program.cs` | `op-theme.js` | `<OpBlazorUiSetup />` |
| --- | --- | --- | --- |
| Blazor Web App: Server | servidor | `App.razor` | `MainLayout` |
| Blazor Web App: WebAssembly | servidor e `.Client` | `App.razor` do servidor | `MainLayout` do `.Client` |
| Blazor Web App: Auto | servidor e `.Client` | `App.razor` do servidor | `MainLayout` do `.Client` |
| WebAssembly standalone | projeto único | `wwwroot/index.html` | `App.razor` |
| MAUI Blazor Hybrid | `MauiProgram.cs` | `wwwroot/index.html` | `Routes.razor` ou layout |
| SSR estático | servidor | `App.razor` | não usa |

### Server

- `Program.cs`: `AddInteractiveServerComponents` e `AddInteractiveServerRenderMode`.
- `App.razor`: script do tema + `HeadOutlet`/`Routes` com `InteractiveServer`.
- `MainLayout`: `<OpBlazorUiSetup />` e hosts de overlay.

### WebAssembly

- `Program.cs` do servidor e do `.Client`: os dois registram `AddOpBlazorUI()`.
- Servidor: `AddInteractiveWebAssemblyComponents` e `AddInteractiveWebAssemblyRenderMode`.
- `App.razor` do servidor: script + `HeadOutlet`/`Routes` com `InteractiveWebAssembly`.
- `MainLayout` do `.Client`: setup e hosts.

### Auto

- Servidor: registre Server + WebAssembly e mapeie os dois render modes.
- `.Client`: registre `AddOpBlazorUI()`.
- `App.razor`: script + `HeadOutlet`/`Routes` com `InteractiveAuto`.
- `MainLayout` do `.Client`: setup e hosts. O estado não passa de um runtime para o outro;
  `PersistTheme = true` restaura o tema salvo.

### WebAssembly standalone

- `Program.cs`: registre os serviços e adicione `HeadOutlet` em `builder.RootComponents`.
- `wwwroot/index.html`: script do tema.
- `App.razor`: setup antes do `Router`. Não há `@rendermode`.

### MAUI Blazor Hybrid

- `MauiProgram.cs`: registre os serviços.
- `wwwroot/index.html`: script + CSS fixo (`fonts/inter/inter.css`, `openng-icons.css`,
  `optimus-base.css`, `quill.snow.css`). O template não tem `HeadOutlet`.
- `Routes.razor`/layout: `<OpBlazorUiSetup />`; não use `@rendermode`.

### SSR estático

- Sem parte interativa: não use `<OpBlazorUiSetup />` nem `@rendermode`.
- `App.razor`: script + links manuais de `fonts/inter/inter.css`, `openng-icons.css` e
  `optimus-base.css`.
- Só componentes de exibição; o tema é fixo pelo `data-dark`.

## 5. Tema antes da primeira pintura

```html
<script src="_content/OpBlazorUI/op-theme.js"
        data-preset="aura" data-primary="noir" data-surface="slate"
        data-dark="light" data-persist="true"></script>
```

O `op-theme.js` aplica o tema antes de o Blazor iniciar, sem flash; os atributos `data-*`
repetem os padrões registrados no `AddOpBlazorUI`. O padrão é **claro**. `data-dark` aceita
`light`, `dark` ou `system` e deve refletir `OpDarkMode`; `data-persist="false"` ignora o
estado salvo no `localStorage`.

## 6. Setup no layout

```razor
@* Uma única vez, dentro da parte interativa. *@
<OpBlazorUiSetup />
```

O setup carrega a fonte Inter, os ícones e o CSS base, e aplica as trocas de tema em runtime.
No WebAssembly standalone ele fica no `App.razor`, nunca no `index.html`.

## Tipografia

A lib empacota Inter Variable 4.1 e usa 14px como base do documento. Para trocar a família,
sobrescreva `--op-font-family` depois do CSS da lib; para mudar o tamanho, use
`html { font-size: ... }`. A fonte é SIL OFL 1.1 e funciona offline.

## 7. Overlays (opcional)

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
- **Interatividade por página**: use no `<OpBlazorUiSetup>` o mesmo `@rendermode` das páginas
  que trocam o tema.
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
- [ ] `theme.DarkMode` definido (`Light` é o padrão) e `data-dark` igual no script.
- [ ] Tradução definida (`OpTranslations.PtBr` é opcional).
- [ ] `op-theme.js` no `<head>`, depois do `<base href>` (ou no `index.html` standalone/MAUI).
- [ ] `<OpBlazorUiSetup />` na parte interativa do layout.
- [ ] Hosts de overlay (`OpToast`, `OpConfirmDialog`, `OpDynamicDialogHost`) se usar overlays.
- [ ] Import do namespace do componente que for usar.
- [ ] No MAUI, CSS fixo no `wwwroot/index.html` e setup no `Routes.razor`/layout, sem `@rendermode`.
- [ ] No SSR estático, links de CSS manuais e nenhum setup interativo.
