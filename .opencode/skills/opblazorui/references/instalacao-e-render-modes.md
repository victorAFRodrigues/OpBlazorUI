# Instalação e render modes

Como instalar o pacote, registrar os serviços, configurar o tema e escolher o render mode.

## 1. Adicionar o pacote

O pacote está no NuGet.org, a fonte padrão do `dotnet`; não é preciso configurar feed nem
credencial.

```bash
dotnet add package OpBlazorUI
# opcional: gráficos (Chart.js)
dotnet add package OpBlazorUI.Charts
```

## 2. Registrar os serviços

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

## 3. `_Imports.razor`

```razor
@using OpBlazorUI
@using OpBlazorUI.Components.Button
```

Cada componente vive no namespace `OpBlazorUI.Components.<Nome>`; importe o que usar.

## 4. Tema e render mode

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

## 5. Setup no layout

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

- [ ] `dotnet add package OpBlazorUI` no projeto.
- [ ] `AddOpBlazorUI()` registrado.
- [ ] `op-theme.js` no `<head>`, depois do `<base href>`.
- [ ] `<OpBlazorUiSetup />` na parte interativa do layout.
- [ ] Import do namespace do componente que for usar.
