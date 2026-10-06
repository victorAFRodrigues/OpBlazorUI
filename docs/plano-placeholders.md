# Plano — Cobertura de placeholders e melhorias da documentação

> Objetivo: levar cada página do Showcase à paridade com a doc de referência
> (PrimeNG / Optimus UI v21). Este documento é o acompanhamento vivo do trabalho.
> Base metodológica: `docs/fase6-docs-playbook.md`; lacunas: `docs/doc-coverage-analysis.md`.

---

## Status geral

- **Fase B (placeholders):** **concluída** — 6 de 6 lotes, **46 de 46 páginas** criadas
  (41 componentes + 5 utilitários); mergeada no `main` (PR #16).
- **Fase A (melhorias):** em andamento — **A1 concluído** (InputText, Checkbox, RadioButton,
  ToggleSwitch).
- **Branch:** `docs/fase-a-inputs` (a partir de `origin/main`).

| Lote | Páginas | Commit |
|------|---------|--------|
| B1 — P0 Formulário | 7 | `03e5a93` |
| B2 — P0 Navegação | 13 | `5a2bee0` |
| B3 — P1 Dados/feedback | 9 | `8ead184` |
| B4 — P2 Layout/mídia | 8 | `c50edbc` |
| B5 — P3 Baixa | 4 | `b7f8780` |
| B6 — Utilitários | 5 | `a78cfdf` |

---

## 1. Estratégia

Duas frentes:

- **Fase B — placeholders dos componentes ainda não portados.** Criar páginas com os textos e
  exemplos da doc de referência (Angular) para que os exemplos sejam substituídos por exemplos
  Blazor reais quando cada componente for implementado.
- **Fase A — melhorias das páginas ruins existentes.** Reescrever as páginas de componentes já
  existentes no padrão ouro (`Button.razor`).

Ordem acordada: **Fase B primeiro** (cobertura imediata), depois Fase A.

---

## 2. Convenções do placeholder

- `@page "/<slug>"`, `DocComponent` com título e descrição traduzidos do upstream.
- Uma `DocSection` por seção do upstream, **na ordem oficial**, com `Description` em pt-BR e
  `Code` = **markup Angular da referência** (`CodeBlock Language="html"`).
- Sem demo funcional (o cartão de demonstração só existirá quando o componente for portado).
- Sem aba API (o componente não existe); `Theming`, `Passthrough` e `KnownIssues` trazem nota.
- `KnownIssues` registra que o componente não foi portado e que os exemplos são da referência.
- Componentes sem página no upstream (adições do Optimus UI) usam seções próprias inspiradas em
  componentes próximos, com aviso explícito em `KnownIssues`.

Referências são baixadas com `bash tools/doc-upstream/fetch.sh <componente> 21.0.0`.

---

## 3. Ranking das páginas existentes (pior → melhor)

Critério: cobertura das seções do upstream, presença de `Description`, Acessibilidade (leitor de
tela + tabela de teclado), API completa e KnownIssues — tendo `Button.razor` como modelo ouro.

### Tier 0 — Críticas
Table, DatePicker, MultiSelect, Select, Checkbox, ToggleSwitch, InputText, RadioButton, Tooltip,
Toast, Message, Menu, Dialog, Drawer, Paginator, VirtualScroller, ProgressSpinner, ProgressBar,
Skeleton, Badge, Tag, Chip, BlockUI, Popover, Card, Avatar, Divider, ScrollTop, FocusTrap, Overlay.

### Tier 1 — Médias
SelectButton, KeyFilter, FloatLabel, IftaLabel, IconField, Rating, Editor, InputOtp, InputGroup,
InputMask, SpeedDial, Listbox, TreeSelect, CascadeSelect, ConfirmDialog, ToggleButton.

### Tier 2 — Boas (sem ação agora)
AutoComplete, Password, SplitButton, InputNumber.

### Tier 3 — Modelo ouro
Button.

---

## 4. Componentes faltantes (41) + utilitários (5)

| Lote | Componentes | Ref upstream v21 |
|------|-------------|------------------|
| P0 Form (7) | Textarea, Fieldset, Slider, InputChips, Inplace, ColorPicker, FileUpload | 6; sem ref: InputChips |
| P0 Nav (13) | Tabs, TabMenu, TabView, Accordion, Breadcrumb, Toolbar, ContextMenu, Menubar, TieredMenu, PanelMenu, MegaMenu, Stepper, Steps | 11; sem ref: TabMenu, TabView |
| P1 Dados/feedback (9) | Tree, TreeTable, ConfirmPopup, InlineMessage, DataView, OrderList, PickList, MeterGroup, Timeline | 8; sem ref: InlineMessage |
| P2 Layout/mídia (8) | Panel, Splitter, ScrollPanel, Dock, Galleria, Carousel, Image, ImageCompare | 8 |
| P3 Baixa (4) | Knob, Terminal, OrganizationChart, Ripple | 4 |
| Utilitários (5) | DynamicDialog, StyleClass, Fluid, Chart.js, AnimateOnScroll | 5 |

Componentes sem página no upstream (adições do Optimus UI): **InputChips, TabMenu, TabView,
InlineMessage** — placeholders com seções próprias.

---

## 5. Batches (Fase B)

| Batch | Conteúdo | Status |
|-------|----------|--------|
| B1 — P0 Form (7) | Textarea, Fieldset, Slider, InputChips, Inplace, ColorPicker, FileUpload | ✅ Concluído |
| B2 — P0 Nav (13) | Tabs, TabMenu, TabView, Accordion, Breadcrumb, Toolbar, ContextMenu, Menubar, TieredMenu, PanelMenu, MegaMenu, Stepper, Steps | ✅ Concluído |
| B3 — P1 (9) | Tree, TreeTable, ConfirmPopup, InlineMessage, DataView, OrderList, PickList, MeterGroup, Timeline | ✅ Concluído |
| B4 — P2 (8) | Panel, Splitter, ScrollPanel, Dock, Galleria, Carousel, Image, ImageCompare | ✅ Concluído |
| B5 — P3 (4) | Knob, Terminal, OrganizationChart, Ripple | ✅ Concluído |
| B6 — Utilitários (5) | DynamicDialog, StyleClass, Fluid, Chart.js, AnimateOnScroll | ✅ Concluído |

Ações transversais por batch: apontar as rotas no `AppMenu.cs` (`ComingSoon = false`) e verificar
build + renderização das páginas.

**Fase B concluída** (6/6 lotes, 46 páginas). Próxima: Fase A (§6).

---

## 6. Fase A — Melhorias (depois da Fase B)

Ordem de execução pelo ranking (§3):

- **A1 — Form básico** (define o padrão Binding/Forms/Acessibilidade): InputText, Checkbox,
  RadioButton, ToggleSwitch.
- **A2 — Seleção complexa**: DatePicker, Select, MultiSelect.
- **A3 — Overlay/feedback**: Tooltip, Toast, Message, Dialog, Drawer, Popover, BlockUI. ✅ Concluído.
- **A4 — Dados/misc**: Table, Menu, Paginator, VirtualScroller, ProgressBar, ProgressSpinner,
  Skeleton, Badge, Tag, Chip, Card, Avatar, Divider, ScrollTop, FocusTrap, Overlay.
- **A5 — Médias**: SelectButton, KeyFilter, FloatLabel, IftaLabel, IconField, Rating, Editor,
  InputOtp, InputGroup, InputMask, SpeedDial, Listbox, TreeSelect, CascadeSelect, ConfirmDialog,
  ToggleButton.

---

## 7. Progresso

### B1 — P0 Formulário ✅

- Páginas criadas em `src/OpBlazorUI.Showcase/Components/Pages/`:
  `Textarea.razor`, `Fieldset.razor`, `Slider.razor`, `InputChips.razor`, `Inplace.razor`,
  `ColorPicker.razor`, `FileUpload.razor`.
- `InputChips` sem referência upstream: seções próprias, usando a classe real do tema
  `p-inputchips`.
- `AppMenu.cs`: rotas `/textarea`, `/fieldset`, `/slider`, `/inputchips`, `/inplace`,
  `/colorpicker`, `/fileupload`; item "Upload" renomeado para "FileUpload".
- Verificação: build 0 erros; as 7 rotas renderizam no Showcase sem UI de erro do Blazor.
- Git: branch `docs/placeholder-coverage` (empilhada sobre `chore/docs-state`); commit
  `docs(OpBlazorUI.Showcase): adicionar placeholders do P0 Formulário` (`03e5a93`).

### B2 — P0 Navegação ✅

- Páginas criadas: `Tabs.razor`, `TabView.razor`, `TabMenu.razor`, `Accordion.razor`,
  `Breadcrumb.razor`, `Toolbar.razor`, `ContextMenu.razor`, `Menubar.razor`, `TieredMenu.razor`,
  `PanelMenu.razor`, `MegaMenu.razor`, `Stepper.razor`, `Steps.razor`.
- `TabMenu` e `TabView` sem referência upstream (componentes legados removidos do PrimeNG):
  seções próprias inspiradas em `Tabs`/`Menubar`, com aviso em `KnownIssues`.
- `AppMenu.cs`: rotas `/tabs`, `/tabview`, `/tabmenu`, `/accordion`, `/breadcrumb`, `/toolbar`,
  `/contextmenu`, `/menubar`, `/tieredmenu`, `/panelmenu`, `/megamenu`, `/stepper`, `/steps`.
- Verificação: build 0 erros; `Tabs`, `TabMenu`, `PanelMenu` e `Steps` renderizam no Showcase sem
  UI de erro do Blazor.
- Git: commit `docs(OpBlazorUI.Showcase): adicionar placeholders do P0 Navegação` (`5a2bee0`).

### B3 — P1 Dados/feedback ✅

- Páginas criadas: `Tree.razor`, `TreeTable.razor`, `Timeline.razor`, `DataView.razor`,
  `OrderList.razor`, `PickList.razor`, `ConfirmPopup.razor`, `MeterGroup.razor`,
  `InlineMessage.razor`.
- `InlineMessage` sem referência upstream: seções próprias (Basic, Severidades, Acessibilidade),
  com aviso em `KnownIssues`.
- `AppMenu.cs`: rotas `/tree`, `/treetable`, `/timeline`, `/dataview`, `/orderlist`, `/picklist`,
  `/confirmpopup`, `/metergroup`, `/inlinemessage`.
- Verificação: build 0 erros; `Tree`, `DataView` e `InlineMessage` renderizam no Showcase sem UI de
  erro do Blazor.
- Git: commit `docs(OpBlazorUI.Showcase): adicionar placeholders do P1 (Dados/feedback)` (`8ead184`).

### B4 — P2 Layout/mídia ✅

- Páginas criadas: `Panel.razor`, `Splitter.razor`, `ScrollPanel.razor`, `Dock.razor`,
  `Galleria.razor`, `Carousel.razor`, `Image.razor`, `ImageCompare.razor`.
- `Image`: a referência upstream fica em `doc/Image` (pasta capitalizada), que o `fetch.sh` não
  localiza — as docs foram baixadas manualmente.
- `AppMenu.cs`: rotas `/panel`, `/splitter`, `/scrollpanel`, `/dock`, `/galleria`, `/carousel`,
  `/image`, `/imagecompare`.
- Verificação: build 0 erros; `Panel`, `Galleria` e `Image` renderizam no Showcase sem UI de erro do
  Blazor.
- Git: commit `docs(OpBlazorUI.Showcase): adicionar placeholders do P2 (Layout/mídia)` (`c50edbc`).

### B5 — P3 (componentes menores) ✅

- Páginas criadas: `Knob.razor`, `Terminal.razor`, `OrganizationChart.razor`, `Ripple.razor`.
- `Ripple` é uma diretiva (seções: import, default, custom, accessibility).
- Referências baixadas via `gh api` (a API anônima do GitHub estava limitada); `organizationchart`
  tem um arquivo de seção com nome atípico (`colored.doc.ts`).
- `AppMenu.cs`: rotas `/knob`, `/terminal`, `/organizationchart`, `/ripple`.
- Verificação: build 0 erros; `Knob` e `OrganizationChart` renderizam no Showcase sem UI de erro do
  Blazor.
- Git: commit `docs(OpBlazorUI.Showcase): adicionar placeholders do P3 (componentes menores)`
  (`b7f8780`).

### B6 — Utilitários ✅

- Páginas criadas: `DynamicDialog.razor`, `StyleClass.razor`, `Fluid.razor`, `Chart.razor`,
  `AnimateOnScroll.razor`.
- `DynamicDialog` e `StyleClass` não têm seção de acessibilidade no upstream; `Chart` tem só
  "Leitor de tela" (sem tabela de teclado) — reproduzido fielmente.
- `AppMenu.cs`: rotas `/dynamicdialog`, `/styleclass`, `/fluid`, `/chart`, `/animateonscroll`.
- Verificação: build 0 erros; `Chart`, `DynamicDialog` e `Fluid` renderizam no Showcase sem UI de
  erro do Blazor.
- Git: commit `docs(OpBlazorUI.Showcase): adicionar placeholders dos utilitários` (`a78cfdf`).

**Fase B encerrada.** A Fase A começa por A1 — Form básico (InputText, Checkbox, RadioButton,
ToggleSwitch), aplicando o padrão ouro de `Button.razor`.

### A3 — Overlay/feedback ✅

- Páginas reescritas: `Tooltip.razor`, `Toast.razor`, `Message.razor`, `Dialog.razor`,
  `Drawer.razor`, `Popover.razor`, `BlockUI.razor` — seções do `index.ts` na ordem, com
  **Acessibilidade** fiel (leitor de tela + tabela de teclado) e abas API/Theming/Pass Through/
  Problemas Conhecidos.
- Adaptações Blazor: `Toast` ganhou a seção **Serviço** (`OpMessageService`); `Message` é inline
  (dinâmico via lista + `@foreach`).
- Limitações registradas em **Problemas Conhecidos** (resumo):
  - `Tooltip`: sem eventos (`onShow`/`onHide`) nem `autoHide`/`options`/template.
  - `Toast`: headless só troca o conteúdo; auto-close por `Task.Delay` não sobrevive ao prerender.
  - `Message`: sem serviço dinâmico (é declarativo); `OnClose` não oculta sozinho.
  - `Dialog`/`Drawer`: `responsive`/`breakpoints` e `headless` não existem nativamente (adaptados
    por estilo); sem animação de saída.
  - `Popover`: exige `@ref` + `Show/Toggle` (sem gatilho automático); `datatable` ancora por
    `OnRowClick` (sem template de célula).
  - `BlockUI`: sem bloqueio do documento inteiro; demo simula a área.
- Verificação: build 0 erros; as 7 rotas renderizam no Showcase sem UI de erro do Blazor.
