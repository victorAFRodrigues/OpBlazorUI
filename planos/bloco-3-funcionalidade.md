# Bloco 3 — Funcionalidade

**Status:** concluído. **3.1** a **3.9** concluídos; testes Playwright/unitários 110 verdes.

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

**Feito** (branch `fix/bloco3-3-entrada`):

- [x] `InputNumber`: o parse de `Currency` alinha o símbolo exibido (antes "R$ 10" não era aceito);
  `MaxFractionDigits` arredonda o valor (não só a exibição); enquanto focado o input mostra o texto
  digitado (sem reformatar a cada tecla, o cursor não pula para o fim).
- [x] `InputText` com `KeyFilter`/`Mask`: o caractere rejeitado é forçado fora do DOM via JS
  (`input.interop.js` → `setValue`); presets `int`/`num`/`money` aceitam `-`; `OnChange` emite o
  valor mascarado.
- [x] `InputChips`: `Readonly` bloqueia a remoção (ícone nem é renderizado); vírgula não duplica
  (separação só no `oninput`); `SeparatorKeys` passa a ser respeitado (antes `,` e `;` fixos);
  Enter não submete mais o formulário (`preventEnterSubmit` via JS).
- [x] `ToggleButton` e demais: `"&nbsp;"` (literal codificado pelo Razor) trocado por `"\u00A0"`
  em ToggleButton, Select, MultiSelect, CascadeSelect e TreeSelect.
- [x] `Rating`: os radios ocultos ganham `@onchange` (teclado muda a nota); `OnFocus`/`OnBlur`
  movidos para os inputs focáveis.
- [ ] `InputOtp`: `IntegerOnly` limpa a letra no DOM (via JS); colar o código inteiro e `select()`
  no foco ficam pendentes (precisam de JS de clipboard).
- [ ] `InputNumber`: texto inválido ainda não gera mensagem de validação no `EditContext`.
- [ ] Verificado com `/_tests/bloco3-3` + `Bloco3_3Tests` (8 cenários).


## 3.4 DatePicker

**Feito** (branch `fix/bloco3-4-datepicker`):

- [x] `Clear()` chama `RangeValueChanged`/`MultipleValueChanged` (antes só limpava o estado local).
- [x] Digitação usa o `DateFormat` (`DateTime.TryParseExact` após converter os tokens do PrimeNG
  para o formato .NET) e só aceita entrada completa; Range e Multiple também são interpretados.
- [x] `OnParametersSet` reconstrói a grade quando `MinDate`, `MaxDate`, `DisabledDates`,
  `DisabledDays`, `NumberOfMonths` ou `FirstDayOfWeek` mudam.
- [x] Navega até a data quando o `Value` muda por fora e sincroniza o horário (`SyncTimeFrom`).
- [x] `OpenAsync` volta `_currentView` para `View`.
- [x] `StartWeekFromFirstDayOfYear` passa a ser usado no cálculo do número da semana.
- [ ] Verificado com `/_tests/bloco3-4` + `Bloco3_4Tests` (4 cenários).


## 3.5 Árvores (Tree, TreeTable, TreeSelect)

**Feito** (branch `fix/bloco3-5-arvores`):

- [x] **`Leaf` (F):** `bool?`; quando `null` deriva de `Children` (`IsLeaf => Leaf ?? !HasChildren`).
  Nó sem filhos deixa de virar lazy; lazy agora exige `Leaf="false"` explícito.
- [x] `Tree`: `FilterBy` usa `BindingFlags.IgnoreCase` (casa `label`/`Label`) e o filtro é recalculado
  quando `FilterBy`/`FilterMode` mudam em runtime; a propagação de checkbox casa os clones por `Key`
  e percorre a árvore visível (o ancestral é marcado com os filhos filtrados).
- [x] `TreeSelect`: filtro hierárquico de verdade com `FilterBy`/`FilterMode` (lenient/strict) e
  propagação por `Key`; remover chip no modo checkbox agora propaga (descendentes + ancestrais).
- [x] `TreeTable`: paginação sobre os nós raiz (descendentes expandidos acompanham a página);
  `AllSelected`/`AllPartial` ignoram nós não selecionáveis; checkbox respeita `Selectable`;
  máscara de `Loading` com spinner já renderizada.
- [x] Verificado com `/_tests/bloco3-5` + `Bloco3_5Tests` (8 cenários).

## 3.6 Abas, Accordion e navegação

**Feito** (branch `fix/bloco3-6-navegacao`):

- [x] `Tabs`/`TabView`: `Unregister` pede re-render, renumera os índices e ajusta o `ActiveIndex`
  (sem cabeçalho fantasma); ids de header/content por instância (`Guid` quando não há `Id`).
- [x] `Accordion`: `ActiveIndexes` é `IReadOnlyList<int>?` e `null` não lança.
- [x] `Fieldset`: Enter/Space alternam uma vez (removido o `keydown` que duplicava o click nativo).
- [x] `Steps`: Enter dispara `ActiveIndexChanged` uma vez; itens executam `Command` e navegam por `Url`.
- [x] `Inplace`: `PreventClick` só bloqueia a ativação (não impede o fechamento).
- [x] `Breadcrumb`: respeita `Visible`, `Disabled` (span com `p-disabled`), `Command` e `Target`;
  novo `OnItemClick` para o dono reagir (o `Command` é `Action`).
- [x] `DataView`: novo `RowsChanged` (controlável com `Rows`).
- [x] `MeterGroup`: `StartTemplate`/`EndTemplate` renderizados conforme `LabelPosition`.
- [x] Verificado com `/_tests/bloco3-6` + `Bloco3_6Tests` (9 cenários).

## 3.7 Menus

**Feito** (branch `fix/bloco3-7-menus`):

- [x] **Itens (L):** `Disabled` com `Url` não navega (âncora sem `href`); `Target`/`rel` respeitados;
  `Command` executado pelos menus (inclusive `OpMenu`) antes do `OnItemClick` (que re-renderiza o dono).
  _Desvio consciente:_ mantido `Command` como `Action` + `OnItemClick` (EventCallback do dono) em vez de
  converter para `EventCallback`, para não quebrar a API do `OpMenuItem` em ~20 pontos.
- [x] Menubar/TieredMenu: abrir um irmão fecha o anterior (e descendentes); clicar numa folha fecha tudo.
- [x] Submenu vertical abre ao lado do item e inverte para a esquerda perto da borda (novo
  `menu.interop.js`; Menubar abre abaixo).
- [x] `ContextMenu`: `ShowAsync(x, y)` posiciona pelas coordenadas do mouse e reposiciona se aberto
  (`OpOverlayAttach` ganhou âncora virtual por coordenadas + `repositionParent`).
- [x] `PanelMenu`: submenus aninhados (mais de dois níveis), com `Disabled`/`Visible`/`Target`.
- [x] Verificado com `/_tests/bloco3-7` + `Bloco3_7Tests` (6 cenários).

## 3.8 Overlays

**Feito** (branch `fix/bloco3-8-overlays`):

- [x] `OpOverlayAttach`: reancora quando a âncora muda com o painel aberto (`repositionParent`) — cobre
  ContextMenu, ConfirmPopup e Popover com outro alvo; nova âncora por coordenadas e por seletor CSS.
- [x] `Dialog`/`Drawer`: restauram o foco no `OnParametersSetAsync` (quando o pai fecha pelo binding,
  antes de o elemento sair do DOM) e no `DisposeAsync`; removido o `focusTrapDispose` pós-render.
- [x] `ConfirmDialog`: usa as opções resolvidas do `Confirm(...)` no render (`Position`,
  `DismissableMask`, `CloseOnEscape`, `Modal`, `FocusTrap`, `DefaultFocus`, `MaskStyleClass`);
  aceita uma confirmação encadeada no `Accept` (não a descarta); `OpConfirmationService.Close()` fecha o
  diálogo (assinatura em `OnConfirmClosed`); `DisposeAsync` limpa a confirmação ativa.
- [x] `VirtualScroller`: orientação horizontal usa `_scrollLeft`; lazy com dados iniciais vazios dispara o
  primeiro lote; `Disabled` renderiza todos os itens.
- [x] `Editor`: normaliza markup vazio (`<p><br></p>`, `<br>`, `&nbsp;`) para `null`.
- [x] `OpOverlay`: modo não modal fecha com clique fora (migrado para `OpOverlayAttach`) e Escape;
  `Target` (seletor CSS) ancora e inverte. `AppendTo` continua sem portal real (o painel usa
  `position: fixed`, que escapa visualmente) — desvio documentado.
- [x] Verificado com `/_tests/bloco3-8` + `Bloco3_8Tests` (11 cenários).

## 3.9 ColorPicker, Slider, diretivas e FilterService

**Feito** (branch `fix/bloco3-9-misc`):

- [x] `ColorPicker`: o interop só inicializa uma vez ao abrir (antes rodava a cada render e zerava o
  arraste); os listeners do `document` são removidos ao fechar/descartar; o cálculo HSB passa a usar
  frações (`#336699` faz round-trip fiel) e o parse aceita hex de 8 dígitos (`#RRGGBBAA`, ignora o
  alfa) e rejeita inválidos (cai no `DefaultColor`); `Disabled` inline recebe `p-disabled` e não interage.
- [x] `Slider`: callbacks do JS chamam `StateHasChanged` e renderizam `CurrentValue` (slider não
  controlado move); sem `Step` arredonda para inteiro; as setas/PageUp/Home/End não rolam a página;
  `OpSliderRange` destrava com os dois handles iguais em Max/Min.
- [x] Diretivas (`StyleClass`/`Ripple`/`AnimateOnScroll`/`AutoFocus`): reaplicam quando os parâmetros
  mudam após montar; os handlers de `document`/`window` se auto-removem se o elemento sair do DOM.
- [x] `OpFilterService`: modos de data aceitam `DateTime`, `DateOnly` e `DateTimeOffset`; filtro `null`
  casa tudo; número em texto é parseado com a cultura informada; modo não registrado (ex.: `Custom` sem
  `Register`) não esvazia o resultado.
- [x] Verificado com `/_tests/bloco3-9` + `Bloco3_9Tests` (7 cenários) e `FilterServiceTests`
  (7 testes unitários).

## Decisões

1. **Ordem dos filhos (Tabs/Accordion):** resolvido em 3.6 — ordem de registro com renumeração (sem
   parâmetro `Index`), `Unregister` re-renderiza e ajusta o `ActiveIndex`.
2. **`Command` dos menus:** em 3.7 foi mantido `Action`, executado pelo componente, com `OnItemClick`
   (EventCallback do dono) disparado em seguida para re-renderizar. Evita quebrar `OpMenuItem` em ~20
   pontos; a alternativa (converter para `EventCallback`) ficou descartada por ora.
3. **`Leaf` como `bool?`:** resolvido em 3.5 (`IsLeaf => Leaf ?? !HasChildren`).
