# Bloco 3 — Funcionalidade

**Status:** em execução. **3.1 Seleção** (`fix/bloco3-1-selecao`) e **3.2 Formulários**
(`fix/bloco3-2-formularios`) concluídos; testes Playwright 50/50.

**Decisões aprovadas:** ordem por registro renumerado (sem `Index` novo por ora); `Command` →
`EventCallback`; `Leaf` → `bool?`. Aplicadas conforme os grupos forem implementados.

**Objetivo:** corrigir comportamentos errados, sem adicionar recursos novos. É o bloco maior;
sugiro dividir em sub-branches por grupo (3.1 a 3.9), cada uma com sua página `/_tests/bloco3-*`.

Caminhos relativos a `src/Components/OpBlazorUI.Base/`.

## 3.1 Seleção (Select, MultiSelect, Listbox, AutoComplete, SelectButton, CascadeSelect)

**Feito** (helper comum `OpSelectOption` + correções por componente):

- [x] **`OptionValue` (G):** helper `Components/Select/OpSelectOption.cs`. A seleção grava o valor da
  opção (`ToValue`, com `Convert.ChangeType` de fallback) e compara `GetValue(option)` com o `Value`
  primitivo (nunca reaplica `OptionValue` sobre ele). Aplicado em Select, MultiSelect, Listbox,
  AutoComplete, SelectButton e CascadeSelect.
- [x] `Select`: modo `Editable` com `value`/`@oninput` (digitar abre e filtra; `FilterActive`).
- [x] Navegação com grupos (Select, MultiSelect): `NavigableOptions` usa os filhos achatados.
- [x] Select e MultiSelect: o input do filtro também trata ArrowDown/ArrowUp/Enter.
- [x] `AutoComplete`: apagar o texto limpa o `Value`; rótulo recalculado quando `Suggestions` chega
  depois (compara referência); `ForceSelection` sem diferenciar maiúsculas; `Separator` separado no
  `OnMultipleInput` (sem prender/duplicar); foco só chama `CompleteMethod` respeitando `MinLength`.
- [x] `Listbox`: `AllSelected`/toggle-all usam `ToggleableOptions` (habilitadas e visíveis) em vez da
  contagem.
- [x] `MultiSelect`: "N items selected" alinhado (`> MaxSelectedLabels` nos dois modos).
- [x] Desempenho: índice passado no render (sem `IndexOf` por opção); `mouseenter` só chama
  `StateHasChanged` quando o índice muda.
- [ ] Verificado com `/_tests/bloco3-1` + `Bloco3_1Tests` (6 cenários).


## 3.2 Formulários: EditContext e SSR

**Feito** (branch `fix/bloco3-2-formularios`; TreeSelect/DatePicker ficam nos blocos 3.5/3.4):

- [x] **Seleção múltipla (I):** `Listbox` (`SelectedValues`), `AutoComplete` e `SelectButton`
  (`MultipleValue`) notificam o `EditContext` via `*Expression` + `NotifyFieldChanged`. Verificado
  com validação ao vivo (a mensagem some sem novo submit).
- [x] **SSR estático (H):** `name="@NameAttributeValue"` nos inputs nativos (Checkbox, Textarea,
  InputNumber) e `<input type="hidden">` com o valor em Select, MultiSelect, AutoComplete,
  CascadeSelect, Listbox, Rating e InputChips.
- [x] `OpInputBase`: o fallback de `ValueExpression` agora é atribuído à propriedade antes do base
  (não reconstrói o `ParameterView`, que descartava o `EditContext` cascateado). Novo parâmetro
  `IgnoreEditContext` remove o `EditContext` das instâncias internas (checkbox de
  MultiSelect/Listbox/Tree/TreeTable/DataTable), que notificavam um campo falso.
- [x] `RadioButton`: passa a integrar com o `EditContext` (notifica o `ModelValue`) e renderiza
  `value="@Value"`.
- [x] `Checkbox` com `Readonly`: `preventDefault` no clique/espaço impede o toggle nativo.
- [x] `Textarea`: renderiza `value="@CurrentValue"` (o pai consegue limpar após digitação).
- [ ] Verificado com `/_tests/bloco3-2` + `Bloco3_2Tests` (4 cenários).


## 3.3 Entrada de texto e números

- [ ] `InputNumber`: com `Currency`, o parse usa o símbolo da cultura e nenhuma edição é aceita;
  `MaxFractionDigits` não é aplicado ao valor; texto inválido não gera erro de validação;
  reformatar a cada tecla joga o cursor para o fim.
- [ ] `InputText` com `KeyFilter`/`Mask`: o caractere rejeitado continua no DOM quando o valor não
  muda (forçar re-render ou filtrar no `keydown`/`beforeinput` via JS); presets `int`/`num` não
  aceitam `-`; `OnChange` emite o valor sem máscara.
- [ ] `InputOtp`: `IntegerOnly` deixa a letra visível; colar o código inteiro; `select()` no foco;
  `UpdateValueAsync` remove buracos.
- [ ] `InputChips`: Enter submete o formulário (falta `preventDefault`); vírgula cria o chip duas
  vezes; `Readonly` não bloqueia remoção; `SeparatorKeys` ignorado (`,` e `;` fixos).
- [ ] `ToggleButton` (e outros): `"&nbsp;"` é codificado pelo Razor e aparece como texto.
- [ ] `Rating`: os radios ocultos não têm `@onchange` (teclado não muda a nota); `OnFocus`/
  `OnBlur` num `div` não focável nunca disparam.

## 3.4 DatePicker

- [ ] `Clear()` não chama `RangeValueChanged`/`MultipleValueChanged`.
- [ ] Digitação: usar o `DateFormat` no parse (hoje `DateTime.TryParse` com a cultura atual) e não
  aceitar entradas parciais; suportar Range e Multiple.
- [ ] `OnParametersSet` chamar `RebuildMonths()` quando `MinDate`, `MaxDate`, `DisabledDates`,
  `DisabledDays`, `NumberOfMonths` ou `FirstDayOfWeek` mudarem.
- [ ] Navegar até a data quando `Value` muda por fora; sincronizar o horário.
- [ ] `OpenAsync` volta `_currentView` para `View`.
- [ ] `StartWeekFromFirstDayOfYear` declarado e não usado.

## 3.5 Árvores (Tree, TreeTable, TreeSelect)

- [ ] **`Leaf` (F):** derivar de `Children` quando não informado (tornar `bool?`); hoje todo nó
  sem filhos vira lazy e o spinner gira para sempre.
- [ ] `Tree`: `FilterBy="label"` não casa com a propriedade `Label` (comparação sensível a
  maiúsculas) e a árvore fica vazia; com filtro, a propagação de checkbox não atualiza os
  ancestrais (clones); o filtro só recalcula quando a referência de `Nodes` muda.
- [ ] `TreeSelect`: `FilterBy`/`FilterMode` ignorados; o filtro perde a hierarquia; remover chip
  no modo checkbox não propaga.
- [ ] `TreeTable`: paginar pelos nós raiz; `AllSelected` conta nós não selecionáveis; checkbox
  ignora `Selectable`; mostrar spinner em `Loading`.

## 3.6 Abas, Accordion e navegação

- [ ] `Tabs`/`TabView`: `Unregister` não pede re-render (cabeçalho fantasma); ordem segue o
  registro, não a marcação; `ActiveIndex` não é ajustado quando abas são removidas; ids fixos
  (`optabs_0_header`) repetem com duas instâncias.
- [ ] `Accordion`: ordem segue o registro; `ActiveIndexes=null` lança.
- [ ] `Fieldset`: Enter alterna duas vezes (keydown + click nativo do botão).
- [ ] `Steps`: Enter dispara `ActiveIndexChanged` duas vezes; `Command`/`Url` ignorados.
- [ ] `Inplace`: `PreventClick` também bloqueia o fechamento.
- [ ] `Breadcrumb`: `Command`, `Disabled`, `Visible` e `Target` ignorados.
- [ ] `DataView`: `Rows` e `First` não são controláveis (sem `RowsChanged`/`FirstChanged`).
- [ ] `MeterGroup`: `StartTemplate`/`EndTemplate` não renderizados.

## 3.7 Menus

- [ ] **Itens (L):** `Disabled` com `Url` não pode navegar; respeitar `Target`; `Command` como
  `EventCallback` (ou chamar `StateHasChanged` do dono); `OpMenu` não executa `Command`.
- [ ] Menubar/TieredMenu: abrir um item irmão fecha o anterior; clicar numa folha fecha os
  submenus.
- [ ] Submenu vertical (TieredMenu, ContextMenu) abre ao lado do item e inverte perto da borda.
- [ ] `ContextMenu`: posicionar pelas coordenadas do mouse; `ShowAsync` com o menu aberto
  reposiciona.
- [ ] `PanelMenu`: mais de dois níveis.

## 3.8 Overlays

- [ ] `OpOverlayAttach`: reanexar quando `Anchor` muda com o painel aberto (resolve ContextMenu,
  ConfirmPopup e Popover com outro alvo).
- [ ] `Dialog`/`Drawer`: restaurar o foco quando o pai fecha pelo binding e no `DisposeAsync`
  (hoje o `focusTrapDispose` roda com o elemento já fora do DOM).
- [ ] `ConfirmDialog`: usar as opções resolvidas do `Confirm(...)` no render (`DismissableMask`,
  `Position` etc. são ignorados); não perder uma confirmação encadeada dentro do `Accept`;
  `OpConfirmationService.Close()` fecha o diálogo; `DisposeAsync` limpa a confirmação ativa.
- [ ] `VirtualScroller`: orientação horizontal não recalcula a janela (usa `_scrollTop`); lazy com
  dados iniciais vazios nunca carrega; `Disabled` renderiza só ~2 itens.
- [ ] `Editor`: normalizar `"<p><br></p>"` para `null`.
- [ ] `OpOverlay`: `Target`/`AppendTo` não usados; modo não modal não fecha com clique fora
  (migrar para `OpOverlayAttach`).

## 3.9 ColorPicker, Slider, diretivas e FilterService

- [ ] `ColorPicker`: o `init` do JS roda a cada render e zera o arraste; listeners no `document`
  nunca removidos; conversão HSB perde precisão (`#336699` → `#346799`); `Disabled` inline ainda
  interativo; hex de 8 dígitos e inválido.
- [ ] `Slider`: callbacks do JS sem `StateHasChanged` (slider não controlado não move); arredondar
  quando não há `Step`; dois handles em `Max` travam; setas rolam a página.
- [ ] Diretivas (StyleClass, Ripple, AnimateOnScroll, AutoFocus): o dispose roda com o elemento
  fora do DOM e não remove listeners; parâmetros alterados após montar não têm efeito.
- [ ] `OpFilterService`: modos de data só com `DateTime`; filtro `null` deve casar tudo (como o
  PrimeNG); número em texto parseado com a cultura atual; modo `Custom` não registrado esvazia.

## Decisões em aberto

1. **Ordem dos filhos (Tabs/Accordion):** reordenar pela marcação exige descobrir a posição no
   DOM (JS) ou usar `@key`/índice explícito. Recomendo um parâmetro opcional `Index`/`Value` e,
   sem ele, a ordem de registro com renumeração (comportamento atual corrigido).
2. **`Command` dos menus:** trocar `Action` por `EventCallback` quebra a API de `OpMenuItem`.
   Compatibilidade é livre neste projeto; recomendo trocar.
3. **`Leaf` como `bool?`:** também muda a API de `OpTreeNode`/`TreeNode`. Recomendo.
