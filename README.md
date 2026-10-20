# Optimus UI (Blazor)

Port do [Optimus UI](https://optimus.openng.org) para Blazor. Uma biblioteca de componentes
(`Razor Class Library`) com os design tokens e o CSS base do tema Aura, mais os componentes
já portados para Blazor.

- Showcase online: https://victorafrodrigues.github.io/OpBlazorUI
- Projeto original (Angular): https://github.com/openng-org/optimus-ui

## Requisitos

- .NET SDK **10.0** ou superior
- Blazor Web App (Server, WebAssembly ou Auto), Blazor WebAssembly standalone, .NET MAUI Blazor Hybrid
  ou SSR estático (somente componentes de exibição)
- Para MAUI, o workload do .NET MAUI (`dotnet workload install maui`)

## Instalação

O guia completo, com o código de cada arquivo e seções autossuficientes por modelo, está em
[Installation](https://victorafrodrigues.github.io/OpBlazorUI/installation).

O pacote é publicado no [NuGet.org](https://www.nuget.org/packages/OpBlazorUI), a fonte padrão
do `dotnet` — o restore não exige feed nem credencial extra:

```bash
dotnet add package OpBlazorUI
```

Para os gráficos, use a mesma versão do pacote principal:

```bash
dotnet add package OpBlazorUI.Charts
```

Os comandos instalam a versão estável mais recente. Para fixar uma versão, use
`dotnet add package OpBlazorUI --version <versão>` (a versão atual está no NuGet).

### Escolha o modelo

| Modelo | `Program.cs` | `op-theme.js` | `<OpBlazorUiSetup />` |
| --- | --- | --- | --- |
| Blazor Web App: Server | servidor | `App.razor` | `MainLayout` |
| Blazor Web App: WebAssembly | servidor e `.Client` | `App.razor` do servidor | `MainLayout` do `.Client` |
| Blazor Web App: Auto | servidor e `.Client` | `App.razor` do servidor | `MainLayout` do `.Client` |
| WebAssembly standalone | projeto único | `wwwroot/index.html` | `App.razor` |
| MAUI Blazor Hybrid | `MauiProgram.cs` | `wwwroot/index.html` | `Routes.razor` ou layout |
| SSR estático | servidor | `App.razor` | não usa |

### Blazor Web App: Server

- Registre os serviços no `Program.cs` do servidor e mapeie `AddInteractiveServerComponents`.
- No `App.razor`, coloque o script do tema depois de `<base href="/" />` e use
  `InteractiveServer` no `HeadOutlet` e no `Routes`.
- Monte o `<OpBlazorUiSetup />`, o Toast, o ConfirmDialog e o DynamicDialog no `MainLayout`.

### Blazor Web App: WebAssembly

- Registre `AddOpBlazorUI` no `Program.cs` do servidor **e** no `Program.cs` do projeto `.Client`.
- Coloque o script no `App.razor` do servidor e use `InteractiveWebAssembly` no `HeadOutlet` e no
  `Routes`.
- O setup e os hosts ficam no `MainLayout` do projeto `.Client`.

### Blazor Web App: Auto

- Registre os serviços nos dois `Program.cs` e mapeie `AddInteractiveServerRenderMode` +
  `AddInteractiveWebAssemblyRenderMode` no servidor.
- No `App.razor`, use `InteractiveAuto` no `HeadOutlet` e no `Routes`.
- O setup fica no `MainLayout` do `.Client`. Com `PersistTheme`, o tema é restaurado quando a
  aplicação passa de Server para WebAssembly.

### WebAssembly standalone

- Registre os serviços no `Program.cs`, adicione `HeadOutlet` em `builder.RootComponents` e monte o
  `<OpBlazorUiSetup />` no `App.razor`, antes do `Router`.
- Coloque o script no `wwwroot/index.html`, depois de `<base href="/" />`.
- Não há render mode nem projeto servidor.

### MAUI Blazor Hybrid

- Registre `AddOpBlazorUI` no `MauiProgram.cs`.
- O template não tem `HeadOutlet`: carregue o `op-theme.js` e os CSS fixos
  (`fonts/inter/inter.css`, `openng-icons.css`, `optimus-base.css` e `quill.snow.css`) no
  `wwwroot/index.html`.
- Monte o `<OpBlazorUiSetup />` no `Routes.razor` ou no layout e não use `@rendermode`.

### SSR estático

- Sem parte interativa, não use `<OpBlazorUiSetup />` nem `@rendermode`.
- Inclua manualmente no `<head>` os links de `fonts/inter/inter.css`, `openng-icons.css` e
  `optimus-base.css`, além do `op-theme.js`.
- Componentes de exibição funcionam; componentes que dependem de eventos ou JavaScript não respondem.
  O tema é fixo e definido pelo `data-dark`.

## Configuração

`Program.cs` (ou `MauiProgram.cs` no MAUI):

```csharp
using OpBlazorUI;
using OpBlazorUI.Services;
using OpBlazorUI.Globalization;

builder.Services.AddOpBlazorUI(
    theme =>
    {
        theme.Preset = "aura";             // aura, lara ou nora
        theme.Primary = "noir";            // noir ou uma das paletas
        theme.Surface = "slate";           // opcional
        theme.DarkMode = OpDarkMode.Light; // padrão; Dark ou System
        theme.PersistTheme = true;         // opcional
    },
    OpTranslations.PtBr);                  // opcional; padrão en-US
```

`op-theme.js` — depois de `<base href="/" />`, no `App.razor` ou no `wwwroot/index.html`. Os
`data-*` repetem as opções do `AddOpBlazorUI`:

```html
<script src="_content/OpBlazorUI/op-theme.js"
        data-preset="aura" data-primary="noir" data-surface="slate"
        data-dark="light" data-persist="true"></script>
```

O `<OpBlazorUiSetup />` fica **dentro da parte interativa**, uma única vez:

```razor
<OpBlazorUiSetup />
@Body
```

`_Imports.razor` — cada componente vive em `OpBlazorUI.Components.<Nome>`; modelos ficam em
`OpBlazorUI.Models`:

```razor
@using OpBlazorUI
@using OpBlazorUI.Components.Button
@using OpBlazorUI.Services
@using OpBlazorUI.Globalization
```

Com interatividade por página, use no `<OpBlazorUiSetup>` o mesmo `@rendermode` das páginas que
trocam o tema. Os componentes funcionam em Interactive Server, WebAssembly, Auto e MAUI.

Para Toast, ConfirmDialog e DynamicDialog, monte os hosts uma vez no layout:

```razor
<OpToast Key="app" />
<OpConfirmDialog Key="app" />
<OpDynamicDialogHost />
```

Importe os namespaces correspondentes (`OpBlazorUI.Components.Toast`,
`OpBlazorUI.Components.ConfirmDialog` e `OpBlazorUI.Components.DynamicDialog`).

Uso:

```razor
<OpButton Label="Salvar" Icon="pi pi-save" />
```

## Tipografia

A biblioteca empacota a **Inter Variable 4.1** e usa **14px** como tamanho base do documento. O
`<OpBlazorUiSetup />` carrega `_content/OpBlazorUI/fonts/inter/inter.css`; a fonte é licenciada sob
SIL OFL 1.1 e funciona offline, sem CDN.

Para usar outra família, sobrescreva a variável depois do CSS da lib:

```css
:root {
    --op-font-family: "Segoe UI", system-ui, sans-serif;
}
```

O tamanho base pode ser ajustado em `html` (`font-size: 16px`, por exemplo). A variável
`--op-font-mono` define a pilha monoespaçada para conteúdo técnico.

## Temas

O tema combina um **preset** (`aura`, `lara`, `nora`), uma **primária** e uma **superfície**.
O padrão é Aura com primária `noir` e **modo claro**:

```csharp
// Em um componente com @inject OpThemeService Theme
Theme.SetPreset("lara");       // themes/lara.css
Theme.SetPrimary("blue");      // noir, emerald, green, lime, orange, amber, yellow, teal, cyan,
                               // sky, blue, indigo, violet, purple, fuchsia, pink, rose
Theme.SetSurface("zinc");      // slate, gray, zinc, neutral, stone, soho, viva, ocean (null = do preset)
```

Primária e superfície vêm de `themes/palettes.css`, aplicadas pelos atributos `data-op-primary` e
`data-op-surface` no `<html>`; trocar a cor não carrega outro arquivo.

Os temas são gerados a partir dos presets do Optimus UI (`@openng/optimus-ui-themes`, MIT):

```bash
cd tools/theme-gen
npm install
npm run build     # regenera wwwroot/themes/{aura,lara,nora}.css e palettes.css
```

Não edite os arquivos gerados à mão; ajustes do OpBlazorUI ficam em `optimus-base.css`.

### Modo escuro e persistência

`OpThemeOptions.DarkMode` aceita `OpDarkMode.Light` (padrão), `Dark` ou `System`. O `System` segue
o `prefers-color-scheme` e reage à troca do tema do sistema com a página aberta. Sem configuração
e sem escolha salva, a aplicação começa clara.

O `op-theme.js` aplica o modo antes da primeira pintura; o `data-dark` deve refletir o
`Program.cs`:

```html
<script src="_content/OpBlazorUI/op-theme.js"
        data-preset="aura" data-primary="noir" data-surface="slate"
        data-dark="light" data-persist="true"></script>
```

Com `PersistTheme = true`, a escolha do usuário no `localStorage` (chave `op-theme`) tem prioridade
sobre o padrão. Para **forçar** claro ou escuro sem switcher, use `DarkMode = Light` (ou `Dark`),
`data-dark` com o mesmo valor, `PersistTheme = false` e `data-persist="false"`. Assim um estado
escuro salvo anteriormente não vence a configuração.

`Theme.SetDarkMode(true)` / `Theme.ToggleDarkMode()` alternam em runtime e passam a ser uma escolha
explícita; `Theme.SetRtl(true)` ativa o RTL. O `OpThemeService` é scoped (um estado por usuário no
Server). Veja [Configuration](https://victorafrodrigues.github.io/OpBlazorUI/configuration).

## Componentes portados

Accordion, AutoComplete, Avatar, Badge, BlockUI, Breadcrumb, Button, Card, CascadeSelect,
Checkbox, Chip, ColorPicker, ConfirmDialog, ConfirmPopup, DataTable, DataView, DatePicker,
Dialog, Divider, Drawer, DynamicDialog, Editor, Fieldset, FileUpload, FloatLabel, FocusTrap,
IconField, IftaLabel, InlineMessage, Inplace, InputChips, InputGroup, InputIcon, InputNumber,
InputOtp, InputSwitch, InputText, Listbox, Menu, Message, MeterGroup, MultiSelect, OrderList,
Overlay, Paginator, Password, PickList, Popover, ProgressBar, ProgressSpinner, RadioButton,
Rating, ScrollTop, Select, SelectButton, Skeleton, Slider, SpeedDial, SplitButton, Stepper,
Steps, Tabs, Tag, Textarea, ThemeSwitcher, Timeline, Toast, ToggleButton, Toolbar, Tooltip,
Tree, TreeSelect, TreeTable, VirtualScroller.

### Gráficos

O `Chart` fica em um projeto separado, `OpBlazorUI.Charts`, sobre o Blazor-ApexCharts.
Instale o pacote e registre o serviço:

```bash
dotnet add package OpBlazorUI.Charts
```

```csharp
using OpBlazorUI.Charts;

builder.Services.AddOpBlazorCharts();
```

No Razor, importe `ApexCharts` e `OpBlazorUI.Charts`. O gráfico depende de JavaScript e só
renderiza em um render mode interativo; veja [Chart](https://victorafrodrigues.github.io/OpBlazorUI/chart).

## Estrutura do repositório

```
src/
  Components/OpBlazorUI/           biblioteca de componentes (RCL)
  OpBlazorUI.Showcase/             showcase/documentação, Blazor WebAssembly standalone (publicado no GitHub Pages)
```

## Desenvolvimento

```bash
dotnet build OpBlazorUI.slnx
dotnet run --project src/OpBlazorUI.Showcase
```

Não é necessário Node.js/npm. O Showcase usa um bundle estático do Tailwind
(`src/OpBlazorUI.Showcase/wwwroot/css/tailwind.css`, ~5 KB) versionado no repositório.
O tema e os tokens são definidos por CSS em `OpBlazorUI/wwwroot/themes/`.
Se precisar regenerar o bundle ao adicionar uma classe nova, veja o cabeçalho de
`wwwroot/css/tailwind.input.css`.

## Publicação

O workflow `.github/workflows/release.yml` publica `OpBlazorUI` e `OpBlazorUI.Charts` no
NuGet.org (autenticação por Trusted Publishing via OIDC, sem API key armazenada) e cria uma
Release com os `.nupkg` anexados. A versão vem da tag; o exemplo usa a versão atual (2.1.0):

```bash
git tag v2.1.0
git push origin v2.1.0
```

## Licença

MIT — veja [LICENSE](LICENSE). Inclui atribuição ao PrimeTek, conforme a licença da versão
community do PrimeNG, da qual o CSS base e os temas derivam.

A fonte **Inter Variable** empacotada em `wwwroot/fonts/inter/` é licenciada sob a
[SIL Open Font License 1.1](src/Components/OpBlazorUI/wwwroot/fonts/inter/OFL.txt); o arquivo de
licença acompanha a fonte no pacote.
