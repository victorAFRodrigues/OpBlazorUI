# Bloco 3 — Funcionalidade

**Status:** proposta, aguardando aprovação.

**Objetivo:** corrigir comportamentos errados, sem adicionar recursos novos. É o bloco maior;
sugiro dividir em sub-branches por grupo (3.1 a 3.9), cada uma com sua página `/_tests/bloco3-*`.

Caminhos relativos a `src/Components/OpBlazorUI.Base/`.

## 3.1 Seleção (Select, MultiSelect, Listbox, AutoComplete, SelectButton, CascadeSelect)

- [ ] **`OptionValue` (G):** a seleção só aceita `option is TValue` e grava a opção inteira; a
  exibição faz `GetOptionValue((object)Value)` sobre o valor primitivo. Centralizar num helper
  comum: gravar `GetOptionValue(option)` e comparar `GetOptionValue(option)` com `Value`.
- [ ] `Select`: modo `Editable` é só um esqueleto (input sem `value` nem `@oninput`).
- [ ] Navegação com grupos (Select, MultiSelect, AutoComplete): o índice usa os grupos em vez das
  opções achatadas; Enter seleciona o grupo ou nada.
- [ ] Select e MultiSelect: o input do filtro não trata ArrowDown/Enter.
- [ ] `AutoComplete`: apagar o texto não limpa o `Value`; rótulo não é recalculado quando
  `Suggestions` chega depois; `ForceSelection` diferencia maiúsculas; `Separator` duplica o texto;
  todo foco chama `CompleteMethod` ignorando `MinLength`.
- [ ] `Listbox`: `AllSelected` compara só a contagem; "selecionar todos" inclui desabilitados e
  itens escondidos pelo filtro.
- [ ] `MultiSelect`: off-by-one entre os modos chip e vírgula no "N items selected".
- [ ] Desempenho: `RenderOption` faz `VisibleOptions.IndexOf` por opção (O(n²)) e cada
  `mouseenter` chama `StateHasChanged`.

## 3.2 Formulários: EditContext e SSR

- [ ] **Seleção múltipla (I):** `MultipleValue` (AutoComplete, SelectButton), `SelectedValues`
  (Listbox), `Selection` (TreeSelect) e `RangeValue`/`MultipleValue` (DatePicker) notificam o
  `EditContext` (`NotifyFieldChanged` com um `FieldIdentifier` próprio).
- [ ] **SSR estático (H):** usar `NameAttributeValue` em todos os inputs; Select, MultiSelect,
  AutoComplete, CascadeSelect, TreeSelect, Listbox, Rating, InputNumber e InputChips precisam de
  um `<input type="hidden">` com o valor.
- [ ] `OpInputBase`: o `ValueExpression` injetado por padrão faz componentes internos (checkbox
  do MultiSelect, input do Password) notificarem um campo falso no `EditContext` do formulário.
- [ ] `RadioButton`: herda de `ComponentBase`; integrar ao `EditContext` e renderizar `value`.
- [ ] `Checkbox` com `Readonly`: o navegador alterna o `checked` nativo e o diff não corrige.
- [ ] `Textarea`: renderizar `value="@CurrentValue"` em vez de conteúdo filho (limpar pelo pai
  não limpa o campo depois de digitado).

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
