# Optimus UI (Blazor)

Port do [Optimus UI](https://optimus.openng.org) para Blazor. Uma biblioteca de componentes
(`Razor Class Library`) com os design tokens e o CSS base do tema Aura, mais os componentes
já portados para Blazor.

- Showcase online: https://victorafrodrigues.github.io/OpBlazorUI
- Projeto original (Angular): https://github.com/openng-org/optimus-ui

## Requisitos

- .NET SDK **10.0** ou superior
- Blazor Web App (Server, WebAssembly ou Auto) ou Blazor WebAssembly standalone

## Instalação

O guia completo, com setup, render modes e opções de tema, está em
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

Alternativamente, baixe o `.nupkg` na página de [Releases](https://github.com/victorAFRodrigues/OpBlazorUI/releases) e use uma
referência de projeto/pasta local.

## Configuração

`Program.cs`:

```csharp
using OpBlazorUI.Globalization;

builder.Services.AddOpBlazorUI(
    theme =>
    {
        theme.Preset = "aura";          // aura, lara ou nora
        theme.Primary = "noir";         // noir ou uma das paletas
        theme.Surface = "slate";        // opcional
        theme.PersistTheme = true;      // opcional
    },
    OpTranslations.PtBr);               // opcional; padrão en-US
```

`_Imports.razor` — cada componente vive em `OpBlazorUI.Components.<Nome>`; modelos ficam em
`OpBlazorUI.Models`:

```razor
@using OpBlazorUI
@using OpBlazorUI.Components.Button
@using OpBlazorUI.Services
@using OpBlazorUI.Globalization
```

`App.razor` — o `op-theme.js` carrega o tema antes da primeira pintura (os `data-*` repetem o tema
do `AddOpBlazorUI`):

```razor
<head>
    <base href="/" />
    <script src="_content/OpBlazorUI/op-theme.js" data-preset="aura" data-primary="noir" data-surface="slate"></script>
    ...
    <HeadOutlet @rendermode="InteractiveAuto" />
</head>
<body>
    <Routes @rendermode="InteractiveAuto" />
</body>
```

`App.razor` (ou `MainLayout.razor`) — o `<OpBlazorUiSetup />` precisa ficar **dentro da parte
interativa** (ícones, CSS base e trocas de tema em runtime):

```razor
<OpBlazorUiSetup />
@Body
```

Em WebAssembly standalone, o `op-theme.js` fica no `wwwroot/index.html` e o `<OpBlazorUiSetup />`
no `App.razor` (ou no layout); o setup não pode ficar no `index.html`.

Os componentes funcionam em Interactive Server, WebAssembly e Auto. Com interatividade por página,
use no `<OpBlazorUiSetup>` o mesmo `@rendermode` das páginas que trocam o tema; numa Blazor Web App
com WebAssembly/Auto, chame `AddOpBlazorUI` nos dois `Program.cs` (servidor e `.Client`).

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

## Temas

O tema combina um **preset** (`aura`, `lara`, `nora`), uma **primária** e uma **superfície**.
O padrão é Aura com primária `noir`:

```razor
@inject OpThemeService Theme

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

Modo escuro e RTL: `Theme.SetDarkMode(true)` / `Theme.ToggleDarkMode()` e `Theme.SetRtl(true)`. O
`OpThemeService` é scoped (um estado por usuário no Server) e, com `PersistTheme`, salva as escolhas no
`localStorage`.

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
  Components/OpBlazorUI/     biblioteca de componentes (RCL)
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
Release com os `.nupkg` anexados. A versão vem da tag; o exemplo usa a versão atual (2.0.2):

```bash
git tag v2.0.2
git push origin v2.0.2
```

## Licença

MIT — veja [LICENSE](LICENSE). Inclui atribuição ao PrimeTek, conforme a licença da versão
community do PrimeNG, da qual o CSS base e os temas derivam.
