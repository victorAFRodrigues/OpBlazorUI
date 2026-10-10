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
- [x] **4.1b Abas e accordion** (branch `feat/bloco4-1b-abas`): Tabs/TabView movem o foco junto
  com a aba (`ElementReference`), pulam desabilitados, Home/End, `aria-disabled`; Accordion com
  ↑↓/Home/End entre cabeçalhos, pulando desabilitados.
- [x] **4.1b2 Steps, TabMenu, MegaMenu e PanelMenu** (branch `feat/bloco4-1b2-menus`): Steps e
  TabMenu movem o foco e selecionam por setas/Home/End (pulando desabilitados); PanelMenu e MegaMenu
  ganham teclado (roving) com Enter/Space ativando e ←/→ expandindo/recolhendo, além de
  `aria-haspopup`/`aria-controls` no PanelMenu e foco no primeiro filho ao expandir.
- [x] **4.1c Árvores — Tree** (branch `feat/bloco4-1c-arvores`): navegação por teclado (↑↓, →/←
  expande/recolhe e move ao filho/pai, Home/End, Enter/Space seleciona) via `tree.interop.js` com
  roving tabindex. TreeTable e TreeSelect ficam em 4.1c2.
- [x] **4.1c2 Árvores — TreeTable e TreeSelect** (branch `feat/bloco4-1c2-arvores`): TreeSelect
  reusa o teclado do Tree dentro do overlay; TreeTable ganhou `initTreeTableKeyboard` (↑↓, →/←,
  Home/End, Enter/Space) usando `data-op-level` para pai/filho.
- [x] **4.1d DatePicker** (branch `feat/bloco4-1d-datepicker`): setas na grade de dias (←→↑↓),
  Home/End (início/fim da semana), PageUp/PageDown (mês), com o foco movido para a célula; dia com
  tabstop roving e `@onfocus`. Células de mês/ano focáveis ficam para depois.
- [x] **4.1e Seleção** (branch `feat/bloco4-1e-selecao`): `scrollIntoView` da opção focada no
  Select e no Listbox; Space seleciona a opção focada no Select; `aria-activedescendant` no Listbox.
  MultiSelect, AutoComplete e CascadeSelect ficam em 4.1e2.
- [x] **4.1e2 Seleção — MultiSelect e AutoComplete** (branch `feat/bloco4-1e2-selecao`):
  MultiSelect com Space alternando a opção focada e `scrollIntoView`; AutoComplete com
  `scrollIntoView` da sugestão destacada. CascadeSelect (`aria-activedescendant`) fica em 4.1e3.
- [x] **4.1e3 CascadeSelect** (branch `feat/bloco4-1e3-1f2`): `aria-activedescendant` aponta para o
  nó focado (`{id}_focused`), com o `<li>` focado recebendo esse id.
- [x] **4.1f Diversos — DataTable e Tooltip** (branch `feat/bloco4-1f-1g-diversos`): DataTable
  ordena por teclado (`tabindex` no cabeçalho, `aria-sort`, Enter/Space); Tooltip fecha com Escape
  (WCAG 1.4.13). SplitButton e SpeedDial ficam em 4.1f2.
- [x] **4.1f2 SplitButton e SpeedDial** (mesma branch): abrem pelo teclado com o foco no primeiro
  item/ação e navegam por setas (Home/End no menu); Escape fecha.
- [x] **4.1g ARIA avulso** (mesma branch): `aria-checked="mixed"` no Checkbox indeterminado;
  `role="spinbutton"` + `aria-valuenow/min/max` no InputNumber; `aria-live` no medidor do Password.
  (`aria-required` já coberto pelos componentes com `Required`.)

## 4.2 Recursos que faltam

- [x] **4.2a1 DataTable (parte 1)** (branch `feat/bloco4-2a1-datatable`): `OpDataTableColumn<TItem>`
  com `BodyTemplate`/`HeaderTemplate`/`FooterTemplate` (e `Footer` textual; `tfoot` automático);
  `DataKey`; paginação controlada reusando `OpPaginator` (`@bind-First`/`@bind-Rows` +
  `RowsPerPageOptions`, estado interno quando não vinculado); `SelectionMode="single"` +
  `SelectionPageOnly` + `aria-selected` nas linhas; `StickyHeader` + `ScrollHeight`.
- [ ] **4.2a2 DataTable (parte 2)**: filtro (global + por coluna) via `OpFilterService`; modo lazy
  (`Lazy`, `TotalRecords`, `OnLazyLoad`).
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
