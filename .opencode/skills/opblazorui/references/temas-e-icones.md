# Temas e ícones

O tema define os design tokens (`--p-*`), as classes `p-*` dos componentes e o modo escuro.
Há três presets: **aura** (padrão), **lara** e **nora**.

## Configuração inicial

```csharp
// Program.cs
builder.Services.AddOpBlazorUI(theme =>
{
    theme.Preset = "lara";      // aura, lara ou nora
    theme.Primary = "blue";     // paleta primária
    theme.Surface = "zinc";     // paleta de superfícies
    theme.Rtl = false;          // direção do documento
    theme.PersistTheme = true;  // guarda as escolhas no localStorage (chave "op-theme")
});
```

```razor
@* no <head>, depois do <base href> *@
<script src="_content/OpBlazorUI/op-theme.js"
        data-preset="aura" data-primary="noir" data-surface=""></script>
```

Os atributos `data-*` repetem os padrões do `AddOpBlazorUI` e evitam o flash de tema errado
antes de o Blazor iniciar. O `<OpBlazorUiSetup />` (no layout, na parte interativa) mantém o
documento sincronizado com o `OpThemeService`.

## Trocando o tema em tempo de execução

```razor
@inject OpThemeService Theme

<OpButton Label="Lara" OnClick="() => Theme.SetPreset("lara")" />
<OpButton Label="Azul" OnClick="() => Theme.SetPrimary("blue")" />
<OpButton Label="Escuro" OnClick="Theme.ToggleDarkMode" />
<OpButton Label="RTL" OnClick="() => Theme.SetRtl(!Theme.Rtl)" />
```

Métodos do `OpThemeService`: `SetPreset`, `SetPrimary`, `SetSurface`, `SetDarkMode(bool)`,
`ToggleDarkMode()` e `SetRtl(bool)`. As propriedades (`Preset`, `Primary`, `Surface`,
`DarkMode`, `Rtl`, `Persist`, `ThemeUrl`) refletem o estado atual.

O `<OpThemeSwitcher />` já traz a paleta de primárias e o botão de modo escuro prontos:

```razor
<OpThemeSwitcher OnThemeChanged="OnTheme" OnDarkModeChanged="OnDarkMode" />

@code {
    private void OnTheme(string primary) { /* opcional */ }
    private void OnDarkMode(bool dark) { /* opcional */ }
}
```

## Modo escuro e RTL

- O modo escuro é a classe `app-dark` no `<html>`; use `SetDarkMode`/`ToggleDarkMode`, não
  manipule a classe na mão.
- O estado inicial pode vir do `prefers-color-scheme` aplicado pelo `op-theme.js`.
- RTL: `theme.Rtl = true` no registro ou `Theme.SetRtl(true)` em runtime; o atributo `dir`
  do `<html>` é atualizado e o CSS usa propriedades lógicas.

## Personalização

- Componentes renderizam as classes `p-*` (PrimeNG/Optimus UI). Para utilitários de layout
  (flex, grid, espaçamento), use a solução de CSS da sua aplicação.
- `StyleClass` adiciona classes ao elemento raiz de cada componente; `Style` aplica inline.
- Para ajustar aparência, prefira as variáveis CSS (`--p-primary-color`,
  `--p-surface-0`, `--p-content-border-color`, `--p-form-field-*`...) a sobrescrever as
  regras `p-*`.
- Não edite os arquivos `wwwroot/themes/*.css` da lib: eles são gerados a partir dos tokens
  dos presets.

## Ícones

A lib usa a fonte nativa; não há SVGs para copiar.

```razor
<OpButton Label="Buscar" Icon="pi pi-search" />
<OpButton Icon="pi pi-trash" Rounded="true" AriaLabel="Excluir" />
<i class="pi pi-check"></i>
```

- A lista completa de glyphs está em `llms/icons.md` (gerado no build).
- Em botões, prefira `Icon`/`IconPos` a montar o `<i>` na mão.
- Não crie ícones SVG à mão: se faltar um glyph, use uma biblioteca de referência (ex.:
  Tabler Icons) e registre como ícone customizado.
