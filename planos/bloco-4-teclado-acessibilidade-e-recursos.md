# Bloco 4 — Teclado, acessibilidade e recursos que faltam

**Status:** em execução (sub-fases empilhadas; ordem: 4.1 → 4.2 → 4.3 → 4.4).

**Objetivo:** paridade de teclado/ARIA com o PrimeNG e os recursos mais usados que ainda faltam.
Os testes Playwright deste bloco cobrem teclado (hoje sem nenhum teste) e os recursos novos.

Caminhos relativos a `src/Components/OpBlazorUI.Base/Components/`.

## 4.0 Housekeeping

**Feito** (branch `feat/bloco4-0-housekeeping`):

- [x] Removidas as páginas `Dock`, `Terminal` e `OrganizationChart` do Showcase e as entradas de
  menu correspondentes (não serão portadas).
- [x] Plano atualizado: a lista de "não portados" estava desatualizada (Carousel, Galleria, Image,
  ImageCompare, Knob, Panel, ScrollPanel e Splitter já foram portados no lote P2).

## 4.1 Teclado e ARIA

Padrão adotado: foco roving (`tabindex` por item, só o focado em `0`), `aria-activedescendant` quando
o contêiner recebe o foco, `preventDefault` nas setas quando o componente as consome.

- [x] **4.1a Menus** (branch `feat/bloco4-1a-menus`): navegação por teclado (roving tabindex +
  foco real) para a família `OpMenuItems` — Menubar, TieredMenu e ContextMenu: ←→ (horizontal),
  ↑↓ (vertical), Home/End, Enter/Space (abre/seleciona), →/← (abre/fecha submenu), Escape fecha.
  `OpMenu` (popup) já tinha teclado (Bloco 3). MegaMenu, PanelMenu e TabMenu passam para 4.1b.
- [ ] **4.1b Abas e menus de referência** (Tabs, TabView, Steps, TabMenu, Accordion, MegaMenu,
  PanelMenu): mover o foco junto com a aba/etapa (`ElementReference`), pular desabilitados,
  `aria-disabled`; teclado no MegaMenu/PanelMenu (raiz navegável, `aria-controls`).
- [ ] **4.1c Árvores** (Tree, TreeTable, TreeSelect): navegação completa por teclado.
- [ ] **4.1d DatePicker**: setas na grade, PageUp/Down, células de mês/ano focáveis, Home/End.
- [ ] **4.1e Seleção** (Select, MultiSelect, Listbox, AutoComplete, CascadeSelect): `scrollIntoView`
  da opção focada, Space abre/alterna, `aria-activedescendant`.
- [ ] **4.1f Diversos**: DataTable (ordenação por teclado, `aria-sort`), SplitButton/SpeedDial,
  Tooltip (fechar com Escape — WCAG 1.4.13).
- [ ] **4.1g ARIA avulso**: `aria-required`, `aria-checked="mixed"`, `role="spinbutton"`,
  `aria-live` no medidor do Password.

## 4.2 Recursos que faltam

- [ ] **4.2a DataTable**: templates de célula/cabeçalho/rodapé; `DataKey`; paginação controlada
  reusando `OpPaginator`; filtro (coluna/global) via `OpFilterService`; modo lazy (`OnLazyLoad`,
  `TotalRecords`); `SelectionMode="single"` e select-all (`SelectionPageOnly`); cabeçalho fixo.
- [ ] **4.2b Overlays**: `blockScroll` (Dialog, Drawer, ConfirmDialog, BlockUI tela cheia); Dialog
  `Draggable`/`Resizable`/`Breakpoints`/`KeepInViewport`; Tooltip `AutoHide`/`Life`/`TooltipEvent`;
  Toast (pausa no hover, dedupe); ContextMenu `Target`/`Global`.
- [ ] **4.2c Outros**: FileUpload (`Url`/`Method`/`CustomUpload`/progresso); TabPanel `Closable`;
  Stepper `linear`; Paginator com estado interno e `@bind`; MultiSelect `SelectionLimit`/
  `SelectedItemsLabel`; AutoComplete `Delay`/`CompleteOnFocus`; PickList/OrderList (duplo clique,
  eventos, `Disabled`); VirtualScroller (`Delay`/`ResizeDelay`/`AutoSize`); Checkbox em grupo +
  `TrueValue`/`FalseValue`.

## 4.3 DynamicDialog

- [ ] `OpDialogService` (scoped) + `OpDynamicDialogRef` + `OpDynamicDialogConfig` (`Header`, `Width`,
  `Style`, `Modal`, `Closable`, `Data`); host único no layout renderizando `DynamicComponent` dentro
  do `OpDialog`; `InputValues` via `ParameterView`; `OnClose(TResult)`.
- [ ] Página `DynamicDialog.razor` reescrita com exemplos reais (padrão da `Button.razor`).

## 4.4 Chart (`OpBlazorUI.Charts`)

Decisão: projeto separado, com dependência de `Blazor-ApexCharts` (MIT, `net10.0`), para não
acoplar o `OpBlazorUI.Base` a uma lib de gráficos.

- [ ] Novo RCL `src/Components/OpBlazorUI.Charts` (referencia `Blazor-ApexCharts` e
  `OpBlazorUI.Base`); adicionado ao `OpBlazorUI.slnx` e referenciado pelo Showcase.
- [ ] `AddOpBlazorCharts()` → `AddApexCharts()` + bridge de tema (cores a partir dos tokens).
- [ ] `OpChart` (wrapper fino) sobre `ApexChart`/`ApexPointSeries`, com placeholder em SSR estático.
- [ ] Página `Chart.razor` reescrita com exemplos reais; documentar a divergência de API vs. o
  Chart.js do PrimeNG e a limitação de SSR.
- [ ] Packaging do pacote: fica no bloco 5 (junto com o release).

## Decisões (resolvidas)

1. **Chart:** projeto separado `OpBlazorUI.Charts` com `Blazor-ApexCharts` (não um wrapper de Chart.js).
2. **Escopo:** 4.1 e 4.2 completos, entregues em sub-fases.
3. **Ordem:** plano (4.1 → 4.2 → 4.3 → 4.4).
4. **Dock/Terminal/OrganizationChart:** removidos (páginas e menu).
