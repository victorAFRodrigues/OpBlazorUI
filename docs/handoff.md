# Handoff — progresso da documentação (Fase B concluída)

> Checkpoint do trabalho de cobertura da documentação. Acompanhamento vivo do detalhe por
> componente: `docs/plano-placeholders.md`. Estado canônico dos componentes: `docs/roadmap.md`.

Última atualização: conclusão da Fase B e abertura do PR #16.

---

## Estado do repositório

| Item | Valor |
|---|---|
| Branch | `docs/placeholder-coverage` |
| Base | `main` |
| PR | [#16](https://github.com/victorAFRodrigues/OpBlazorUI/pull/16) `docs: placeholders dos componentes não portados (Fase B)` |
| Commits à frente do `main` | 12 |
| Diff | 51 arquivos, +10075 / −83 |
| Build | `dotnet build OpBlazorUI.slnx` → 0 erros |

PRs anteriores (#13 `feat/op-inputs`, #14 `feat/modal-focus`, #15 `chore/docs-state`) já foram
mergeados no `main`.

---

## Fundação (mergeada)

- Inputs derivam de `OpInputBase<TValue>` (`Invalid`/`Disabled`/`StyleClass`, `EditForm`/
  `DataAnnotations`); `OpCss.BuildClass` com dedupe; 16 inputs migrados (`Checked` → `Value`).
- Dialog/Drawer modais e `OpConfirmDialog` prendem o foco (`Tab`) e restauram o foco ao gatilho
  (`optimus.interop.js`: `focusTrapInit`/`focusTrapDispose`; `OpModalBase`).
- Testes em `tests/OpBlazorUI.Playwright` (2 passando).

---

## Fase B — placeholders (concluída)

46 páginas em `src/OpBlazorUI.Showcase/Components/Pages/`, todas com rota no `AppMenu.cs`
(sem `ComingSoon`). Textos e exemplos vêm da doc de referência (PrimeNG/Optimus UI v21 em Angular),
em pt-BR, com snippets via `CodeBlock`.

| Lote | Componentes | Páginas | Commit |
|---|---|---|---|
| B1 — P0 Formulário | Textarea, Fieldset, Slider, InputChips, Inplace, ColorPicker, FileUpload | 7 | `03e5a93` |
| B2 — P0 Navegação | Tabs, TabMenu, TabView, Accordion, Breadcrumb, Toolbar, ContextMenu, Menubar, TieredMenu, PanelMenu, MegaMenu, Stepper, Steps | 13 | `5a2bee0` |
| B3 — P1 Dados/feedback | Tree, TreeTable, ConfirmPopup, InlineMessage, DataView, OrderList, PickList, MeterGroup, Timeline | 9 | `8ead184` |
| B4 — P2 Layout/mídia | Panel, Splitter, ScrollPanel, Dock, Galleria, Carousel, Image, ImageCompare | 8 | `c50edbc` |
| B5 — P3 Baixa | Knob, Terminal, OrganizationChart, Ripple | 4 | `b7f8780` |
| B6 — Utilitários | DynamicDialog, StyleClass, Fluid, Chart, AnimateOnScroll | 5 | `a78cfdf` |

Commits de documentação do plano: `23dd70c`, `4d97df8`, `66a91f6`, `4ca0adb`, `ecfccf8`.

### Sem página no upstream (adições do Optimus UI)

**InputChips, TabMenu, TabView, InlineMessage** — placeholders com seções próprias e aviso em
`KnownIssues`.

### Convenções do placeholder

- `@page "/<slug>"`, `@namespace OpBlazorUI.Showcase.Components.Pages`,
  `@using OpBlazorUI.Showcase.Components.Doc`, `<PageTitle>`.
- `DocComponent` com `Description` incluindo o aviso "não portado"; uma `DocSection` por seção do
  upstream, na ordem oficial, com `Description` + `Code` (snippet Angular).
- Blocos `Api`/`Theming`/`Passthrough`/`KnownIssues` iguais ao modelo `Textarea.razor`
  (`Textarea.razor` é o modelo exato). `@@openng/optimus-ui-themes` no markup (dois `@`).
- Sem demos. Modelo ouro de página completa: `Button.razor`.

---

## Pendências / próximos passos

- **Merge do PR #16** quando aprovado.
- **Fase A — melhorias das páginas existentes** (ranking em `docs/plano-placeholders.md` §3),
  em PRs separados, começando por **A1** (InputText, Checkbox, RadioButton, ToggleSwitch).
- Branch local `feat/op-inputbase` pode ser apagada (coberta pelos PRs mergeados).

---

## Ambiente de verificação

```bash
dotnet build OpBlazorUI.slnx -v q --nologo          # "Compilação com êxito"
dotnet run --project src/OpBlazorUI.Showcase --urls http://localhost:5199
```

Verificação de renderização (Chromium headless, uma página por invocação):

```bash
CHROME="$HOME/Library/Caches/ms-playwright/chromium-1243/chrome-mac-arm64/Google Chrome for Testing.app/Contents/MacOS/Google Chrome for Testing"
"$CHROME" --headless=new --disable-gpu --no-sandbox --disable-dev-shm-usage \
  --virtual-time-budget=120000 --dump-dom "http://localhost:5199/<rota>"
```

- Shell de ~3,6 KB = app não bootou (repetir/aumentar o budget); páginas OK = 30–70 KB.
- Erros `CVDisplayLinkCreateWithCGDisplay` no stderr são ruído do macOS.
