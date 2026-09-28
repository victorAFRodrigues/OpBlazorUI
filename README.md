# Optimus UI (Blazor)

Port do [Optimus UI](https://optimus.openng.org) para Blazor. Uma biblioteca de componentes
(`Razor Class Library`) com os design tokens e o CSS base do tema Aura, mais os componentes
já portados para Blazor.

- Showcase online: https://optimus.openng.org
- Projeto original (Angular): https://github.com/openng-org/optimus-ui

## Requisitos

- .NET SDK **10.0** ou superior
- Blazor Web App, Blazor Server ou Blazor WebAssembly

## Instalação

O pacote é publicado no GitHub Packages deste repositório. Como o registro do GitHub exige
autenticação mesmo para pacotes públicos, adicione um `nuget.config` na raiz do seu projeto:

```xml
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
```

O PAT precisa do escopo `read:packages`. O `packageSourceMapping` evita que uma falha de
autenticação do GitHub Packages derrube o restore de pacotes do nuget.org.

Depois:

```bash
dotnet add package OpBlazorUI.Base
```

Alternativamente, baixe o `.nupkg` na página de [Releases](../../releases) e use uma
referência de projeto/pasta local.

## Configuração

`Program.cs`:

```csharp
builder.Services.AddOpBlazorUI();
```

`_Imports.razor`:

```razor
@using OpBlazorUI.Base
@using OpBlazorUI.Base.Components.Button
```

`App.razor` — o `op-theme.js` carrega o tema antes da primeira pintura (os `data-*` repetem o tema
do `AddOpBlazorUI`):

```razor
<head>
    <base href="/" />
    <script src="_content/OpBlazorUI.Base/op-theme.js" data-preset="aura" data-primary="noir"></script>
    ...
    <HeadOutlet @rendermode="InteractiveAuto" />
</head>
<body>
    <Routes @rendermode="InteractiveAuto" />
</body>
```

`MainLayout.razor` — o `<OpBlazorUiSetup />` precisa ficar **dentro da parte interativa** (ícones,
CSS base e trocas de tema em runtime):

```razor
<OpBlazorUiSetup />
@Body
```

Os componentes funcionam em Interactive Server, WebAssembly e Auto. Com interatividade por página,
use no `<OpBlazorUiSetup>` o mesmo `@rendermode` das páginas que trocam o tema; numa Blazor Web App
com WebAssembly/Auto, chame `AddOpBlazorUI` nos dois `Program.cs` (servidor e `.Client`).

Uso:

```razor
<OpButton Label="Salvar" Icon="pi pi-save" />
```

## Temas

O tema combina um **preset** (`aura`, `lara`, `nora`), uma **primária** e uma **superfície**.
O padrão é Aura com primária `noir`:

```csharp
OpTheme.SetPreset("lara");     // themes/lara.css
OpTheme.SetPrimary("blue");    // noir, emerald, green, lime, orange, amber, yellow, teal, cyan,
                               // sky, blue, indigo, violet, purple, fuchsia, pink, rose
OpTheme.SetSurface("zinc");    // slate, gray, zinc, neutral, stone, soho, viva, ocean (null = do preset)
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

Modo escuro e RTL via `_content/OpBlazorUI.Base/optimus.interop.js` (`setDarkMode`, `setRtl`).

## Componentes portados

Button, CascadeSelect, Checkbox, DataTable, DatePicker, Editor, FloatLabel, IconField,
IftaLabel, InputGroup, InputMask, InputNumber, InputOtp, InputText, KeyFilter, Listbox,
Menu, Message, MultiSelect, Password, RadioButton, Rating, Select, SelectButton,
SpeedDial, SplitButton, ThemeSwitcher, Toast, ToggleButton, ToggleSwitch, Tooltip,
TreeSelect, Avatar, Badge, Chip, ProgressBar, ProgressSpinner, Skeleton, Tag,
Dialog, Drawer, FocusTrap, Overlay, Popover, BlockUI, Card, Divider, Paginator,
ScrollTop, VirtualScroller.

## Estrutura do repositório

```
src/
  Components/OpBlazorUI.Base/     biblioteca de componentes (RCL)
  OpBlazorUI.Showcase/             showcase/documentação, Blazor WebAssembly standalone (publicado no GitHub Pages)
```

## Desenvolvimento

```bash
dotnet build OpBlazorUI.slnx
dotnet run --project src/OpBlazorUI.Showcase
```

## Publicação

O workflow `.github/workflows/release.yml` publica no GitHub Packages e cria uma Release
com o `.nupkg` anexado. A versão vem da tag:

```bash
git tag v1.0.0
git push origin v1.0.0
```

## Licença

MIT — veja [LICENSE](LICENSE). Inclui atribuição ao PrimeTek, conforme a licença da versão
community do PrimeNG, da qual o CSS base e os temas derivam.
