# Instalação e render modes

Como instalar o pacote, registrar os serviços, configurar o tema e escolher o render mode.

## 1. Autenticação no GitHub Packages

O pacote é publicado no GitHub Packages e o restore exige um PAT com escopo `read:packages`,
mesmo para pacotes públicos. Crie ou atualize o `nuget.config` na raiz da solução:

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

## 2. Adicionar o pacote

```bash
dotnet add package OpBlazorUI
# opcional: gráficos (Chart.js)
dotnet add package OpBlazorUI.Charts
```

## 3. Registrar os serviços

```csharp
// Program.cs
builder.Services.AddOpBlazorUI();

// opcional: gráficos (pacote OpBlazorUI.Charts)
builder.Services.AddOpBlazorCharts();
```

Para definir o tema inicial:

```csharp
builder.Services.AddOpBlazorUI(theme =>
{
    theme.Preset = "lara";    // aura (padrão), lara ou nora
    theme.Primary = "blue";   // paleta primária
    theme.Surface = "zinc";   // paleta de superfícies
    theme.Rtl = false;
    theme.PersistTheme = true; // salva as escolhas no localStorage
});
```

## 4. `_Imports.razor`

```razor
@using OpBlazorUI
@using OpBlazorUI.Components.Button
```

Cada componente vive no namespace `OpBlazorUI.Components.<Nome>`; importe o que usar.

## 5. Tema e render mode

```razor
<head>
    <base href="/" />
    <script src="_content/OpBlazorUI/op-theme.js" data-preset="aura" data-primary="noir"></script>
    <HeadOutlet @rendermode="InteractiveAuto" />
</head>
<body>
    <Routes @rendermode="InteractiveAuto" />
</body>
```

O `op-theme.js` aplica o tema antes da primeira pintura, sem flash; os atributos `data-*`
repetem os padrões registrados no `AddOpBlazorUI`.

## 6. Setup no layout

```razor
@* Precisa estar dentro da parte interativa da aplicação. *@
<OpBlazorUiSetup />
```

Adicione uma única vez, no `MainLayout`.

## Render modes

- **InteractiveServer, InteractiveWebAssembly e InteractiveAuto**: funcionam sem mudanças; o
  estado da lib é sempre *scoped* (nunca `static`).
- **SSR estático**: a marcação renderiza, mas componentes com interação (overlays, foco,
  JS interop) precisam de uma parte interativa para funcionar.
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

- [ ] `nuget.config` com o PAT (`read:packages`).
- [ ] `AddOpBlazorUI()` registrado.
- [ ] `op-theme.js` no `<head>`, depois do `<base href>`.
- [ ] `<OpBlazorUiSetup />` na parte interativa do layout.
- [ ] Import do namespace do componente que for usar.
