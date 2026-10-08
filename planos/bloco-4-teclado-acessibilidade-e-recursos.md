# Bloco 4 — Teclado, acessibilidade e recursos que faltam

**Status:** proposta, aguardando aprovação.

**Objetivo:** paridade de teclado/ARIA com o PrimeNG e os recursos mais usados que ainda faltam.
Os testes Playwright deste bloco devem cobrir teclado, que hoje não tem nenhum teste.

Caminhos relativos a `src/Components/OpBlazorUI.Base/Components/`.

## 4.1 Teclado e ARIA (D)

- [ ] **Menus** (`Menu/OpMenuItems`, Menubar, TieredMenu, ContextMenu): todos os itens têm
  `tabindex="-1"` e não há `onkeydown`. Implementar setas, Home/End, Enter/Space e Escape por nível,
  com `aria-activedescendant`.
- [ ] **MegaMenu** e **PanelMenu**: o item raiz é um `<a>` sem `href` (Enter não dispara click);
  filhos inalcançáveis; falta `aria-controls`.
- [ ] **Tabs/TabView**: mover o foco junto com a aba (`ElementReference` nos botões); pular abas
  desabilitadas; Home/End; `aria-disabled`.
- [ ] **TabMenu** e **Steps**: só o item ativo é alcançável; adicionar setas.
- [ ] **Accordion**: ArrowUp/Down/Home/End entre cabeçalhos; `aria-disabled`.
- [ ] **Tree**, **TreeTable** e **TreeSelect**: navegação por teclado completa (hoje inexistente).
- [ ] **DatePicker**: setas na grade de dias; células de mês/ano focáveis.
- [ ] **Select, MultiSelect, Listbox, AutoComplete**: `scrollIntoView` da opção focada; Space abre
  o Select e alterna opção no MultiSelect; `aria-activedescendant` no Listbox e no CascadeSelect.
- [ ] **DataTable**: ordenação pelo teclado (`tabindex`, `aria-sort`, Enter).
- [ ] **Slider**: `preventDefault` nas setas/PageUp/PageDown/Home/End.
- [ ] **SplitButton** e **SpeedDial**: navegação por setas; submenus abrem pelo teclado/toque.
- [ ] **Tooltip**: fechar com Escape (WCAG 1.4.13).
- [ ] Outros: `aria-required` onde `Required` é declarado e não usado; `aria-checked="mixed"` no
  Checkbox indeterminado; `role="spinbutton"` no InputNumber; `aria-live` no medidor do Password.

## 4.2 Recursos que faltam

### DataTable
- [ ] Templates de célula, cabeçalho e corpo (hoje só `ToString()`; bloqueia uso real).
- [ ] `DataKey` para seleção e para manter estado entre recargas.
- [ ] Página controlada (`First`/`FirstChanged`), `RowsPerPageOptions`, relatório de página —
  reutilizando o `OpPaginator` em vez dos dois paginadores escritos à mão.
- [ ] Filtro (por coluna e global) usando o `OpFilterService`.
- [ ] Modo lazy (`OnLazyLoad`, `TotalRecords`), com ordenação no servidor.
- [ ] `SelectionMode="single"`; select-all de todos os itens (com `SelectionPageOnly`).
- [ ] Cabeçalho fixo (scrollable).

### Overlays
- [ ] `blockScroll` (scroll lock no `body`) em Dialog, Drawer, ConfirmDialog e BlockUI de tela
  inteira.
- [ ] Dialog: `Draggable`, `Resizable`, `Breakpoints`, `KeepInViewport`.
- [ ] Tooltip: `AutoHide`, `Life`, `TooltipEvent`.
- [ ] Toast: pausar o timer no hover; `PreventDuplicates` x `PreventOpenDuplicates`.
- [ ] ContextMenu: `Target`/`Global` com ligação automática ao evento `contextmenu`.

### Outros componentes
- [ ] FileUpload: `Url`/`Method`/`CustomUpload`, `OnProgress`/`OnError`, barra de progresso,
  botão de envio no modo `basic`.
- [ ] TabPanel `Closable` com `OnClose`.
- [ ] Stepper modo `linear`.
- [ ] Paginator com estado interno e `@bind` (`FirstChanged`/`RowsChanged`).
- [ ] MultiSelect `SelectionLimit` e `SelectedItemsLabel`; AutoComplete `Delay` (debounce) e
  `CompleteOnFocus`.
- [ ] PickList/OrderList: duplo clique, drag-and-drop, eventos `OnMoveToTarget`/`OnReorder`;
  respeitar `Disabled` nos botões de mover.
- [ ] VirtualScroller: `Delay`, `ResizeDelay`, `AutoSize`, virtualização de colunas.
- [ ] Checkbox em grupo (`value` + model em array) e `TrueValue`/`FalseValue`.

### Componentes ainda não portados (só placeholder no Showcase)

Carousel, Chart, Dock, DynamicDialog, Galleria, Image, ImageCompare, Knob, OrganizationChart,
Panel, ScrollPanel, Splitter e Terminal, além de Bind, ClassNames, DragDrop e Pass Through
(só `/coming-soon`). Sugiro priorizar Panel, Splitter e ScrollPanel, que são de layout e baratos.

## Decisões em aberto

1. **Escopo:** este bloco é grande. Recomendo fazer 4.1 inteiro e, de 4.2, só o DataTable e o
   `blockScroll` antes de portar componentes novos.
2. **Componentes não portados:** entram neste bloco ou viram um bloco 6?
