# Plano de execução — Fundação (formulário, foco e estado do repo)

> Status: **PR 1 (Ondas A–E), PR 2 (foco em modais + Playwright) e PR 3 (roadmap/menu/docs) implementados — build verde e 2 testes de foco passando.**
> Escopo: 3 PRs anteriores aos lotes de componentes novos
> Motivação: os 47 componentes já portados ainda não formam um sistema — falta validação de
> formulário e foco nos overlays.

---

## 1. Contexto / descobertas

Catálogo canônico do upstream: `@openng/optimus-ui-themes/types` (**88** componentes).

| Métrica | Valor |
|---|---|
| Componentes no upstream | 88 |
| Já implementados | 47 |
| Faltando | 41 (todos com CSS e tokens no tema) |
| Componentes que derivam de `InputBase<T>` | **0** |
| Componentes que usam `OpFocusTrap` | **0** |

### 1.1 Formulário não funciona

Nenhum dos 18 componentes de entrada deriva de `InputBase<T>`; nenhum expõe `ValueExpression`;
nenhum toca `EditContext`, `ValidationMessage` ou `FieldIdentifier`.

Consequência: `EditForm` + `DataAnnotationsValidator` **não valida nada** hoje — e 8 páginas do
Showcase já usam `EditForm` com efeito zero.

### 1.2 `FocusTrap` existe e funciona, mas é órfão

`OpFocusTrap` (components/FocusTrap/) e o JS `focusTrapInit` (wwwroot/optimus.interop.js:40)
ciclam Tab/Shift+Tab corretamente. O problema é de composição: **nenhum componente o usa**.
Dialog, Drawer e ConfirmDialog têm `tabindex="-1"` e `FocusOnShow`, mas não prendem o foco nem
restauram o foco ao elemento que disparou — limitação já documentada nos Known Issues da página
`src/OpBlazorUI.Showcase/Components/Pages/ConfirmDialog.razor:224-241`.

### 1.3 Família booleana/radio não usa `Value` como modelo

| Componente | Parâmetro de modelo atual | Colide com `InputBase<TValue>` |
|---|---|---|
| `OpInputSwitch` | `Checked` / `CheckedChanged` (`bool`) | sim (não tem `Value`) |
| `OpCheckbox` | `Checked` / `CheckedChanged` (`bool?`) + `Value` (`string`, value HTML) | sim (dois usos de `Value`) |
| `OpToggleButton` | `Checked` / `CheckedChanged` (`bool`) | sim (não tem `Value`) |
| `OpRadioButton` | `ModelValue` / `ModelValueChanged` + `Value` (valor do item) | sim (grupo N→1, não é InputBase 1:1) |
| `OpSelectButton` | `Value` / `ValueChanged` (single) + `Multiple` / `MultipleValue` | parcial (modo múltiplo não é 1:1) |
| demais 13 | `Value` / `ValueChanged` | não |

### 1.4 Superfície de `InputBase<TValue>` no SDK fixado (net10.0)

Confirmada em `Microsoft.AspNetCore.Components.Web.dll`:

`AdditionalAttributes` · `CssClass` · `Name` · `EditContext` · `Value` / `ValueChanged` ·
`ValueExpression` · `FieldIdentifier` · `CurrentValue` / `CurrentValueAsString` ·
`TryParseValueFromString` · `FormatValueAsString`

**`AdditionalAttributes` e `CssClass` já existem na base** — as declarações locais vão colidir e
precisam sair. `Name`, `Class` e `Style` precisam ser verificados na primeira compilação da Onda A.

### 1.5 Estado do repo desatualizado

- `docs/roadmap.md` marca como pendentes vários componentes já entregues.
- `docs/doc-coverage-analysis.md` mede cobertura de documentação, não lacunas de componentes.
- A sidebar (`AppMenu.cs`) marca 30 dos 41 faltantes como "em breve" e **omite 11**: Dock,
  OrganizationChart, TabView, TabMenu, Steps, InlineMessage, InputChips, Slider, Knob, Terminal,
  Ripple.

---

## 2. Decisões tomadas

| Decisão | Escolha |
|---|---|
| Ordem de execução | Fundação completa (3 PRs) antes de componentes novos |
| Estratégia do form | Derivar de `InputBase<T>` (padrão Blazor) |
| Checkbox / Switch / Radio / ToggleButton | Renomear o modelo para `Value` (convenção Blazor / PrimeBlazor); `RadioButton` fora do `InputBase` |
| SelectButton | Modo single entra na Onda D; modo múltiplo fica fora, documentado |
| Escopo do PR 1 | 16 componentes, 5 ondas |

---

## 3. PR 1 — `EditForm` / validação

### 3.1 Base compartilhada

`src/Components/OpBlazorUI.Base/Components/Forms/OpInputBase.cs`

```csharp
public abstract class OpInputBase<TValue> : InputBase<TValue>
```

- Centraliza só o que é comum aos 16: `Invalid`, `Disabled`, `StyleClass`.
- `Fluid` e `Readonly` **não são comuns** — ficam nos derivados. Fluid falta em `InputOtp`,
  `Rating`, `InputSwitch`, `Checkbox` e `Listbox`; Readonly falta em `InputSwitch`.
- `Invalid` continua existindo e é OR-ed com o estado do `EditContext` (o upstream aceita
  `invalid="true"` sem `EditContext`).
- A classe do controle passa a usar o `CssClass` da base, em vez de um `IsInvalid` próprio.
- Remove as declarações locais de `Value`, `ValueChanged` e `AdditionalAttributes` — herdadas.
- Não usa JS → segue funcionando em SSR estático.

`OpCss.BuildClass` (OpCss.cs:5) passa a **deduplicar**: hoje é só `string.Join` com filtro de
vazio, então `p-invalid` vindo do `EditContext` + do parâmetro `Invalid` sairia duplicado.

### 3.2 Ondas de conversão

| Onda | Componentes | `TValue` | Risco |
|---|---|---|---|
| A | `OpInputText`, `OpPassword`, `OpInputOtp` | `string?` | baixo |
| B | `OpInputNumber`, `OpRating`, `OpInputSwitch`, `OpCheckbox`, `OpToggleButton` | `decimal?`, `int`, `bool`, `bool?`, `bool` | médio (renome de API) |
| C | `OpDatePicker` | `DateTime?` | médio (27 pontos de escrita de `Value`) |
| D | `OpSelect`, `OpListbox`, `OpMultiSelect`, `OpAutoComplete`, `OpSelectButton` (single) | genérico | médio |
| E | `OpTreeSelect`, `OpCascadeSelect` | composto | alto (valor é caminho / `CascadeselectValue`) |

Cada onda compila e é revisável isolada.

### 3.3 Regras de conversão

- `Value = x; await ValueChanged.InvokeAsync(x)` → `CurrentValue = x`.
- `FormatValueAsString` e `TryParseValueFromString` só onde há conversão textual própria
  (`InputNumber`, `DatePicker`) — evita quebra do `CurrentValueAsString` sob `EditForm`.
- `InputSwitch.Checked` → `Value`; `ToggleButton.Checked` → `Value`; `Checkbox.Checked` → `Value`
  e o `Value` string (value HTML) sai; `SelectButton` usa `Value` no modo single.
- `RadioButton` e `SelectButton` múltiplo ficam fora do `InputBase`; a limitação de validação de
  grupo em `EditForm` fica documentada (sem `EditContext`/`FieldIdentifier` nesta fundação).
- `p-invalid` continua sendo aplicado exatamente onde o upstream aplica (controle e/ou wrapper).
- Renomear `Checked` → `Value` é quebra de API aceita: a biblioteca não tem consumidor externo.
- Migração interna obrigatória no mesmo PR (senão o Showcase não compila):
  - `src/OpBlazorUI.Showcase/Components/Pages/ToggleSwitch.razor` (7 usos)
  - `src/OpBlazorUI.Showcase/Components/Pages/Checkbox.razor` (9 usos)
  - `src/OpBlazorUI.Showcase/Components/Pages/ToggleButton.razor` (5 usos)
  - `src/OpBlazorUI.Showcase/Components/Layout/Configurator.razor:50`
  - `src/OpBlazorUI.Showcase/Components/Pages/InputGroup.razor` (3 usos)

### 3.4 Fora do escopo (com justificativa)

- `OpEditor` — o valor é HTML; o upstream TextEditor não é input de formulário.
- `OpRadioButton` — grupo N→1, não é `InputBase` 1:1.
- `OpSelectButton` no modo múltiplo (`Multiple` / `MultipleValue`) — não é `InputBase` 1:1.
- Wrappers sem valor: `IconField`, `InputGroup`, `FloatLabel`, `IftaLabel`.

### 3.5 Showcase

- Bloco `EditForm` real (`DataAnnotationsValidator` + `ValidationMessage`) nas páginas de input.
- Padronizar esse bloco no `docs/fase6-docs-playbook.md` — hoje 8 páginas têm `EditForm` sem
  nenhuma validação aparecer.

---

## 4. PR 2 — Foco nos modais

- Compor `OpFocusTrap` dentro do root de `OpDialog` (somente quando `Modal`), `OpDrawer` (somente
  quando `Modal`, que é o default) e `OpConfirmDialog`.
- `wwwroot/optimus.interop.js`:
  - `focusTrapInit(element, initialFocusSelector)` — foco inicial além do ciclo de Tab;
  - `focusRemember(id)` / `focusRestore(id)` — guardam o elemento que disparou num `WeakMap`
    (não serializa `ElementReference`).
- Precedência de foco: o trap com `initialFocusSelector` vence; sem seletor, o root continua
  recebendo `FocusOnShow`. Isso resolve a corrida com o `FocusAsync()` atual.
- Preservar `Escape` → fechar dentro do trap.
- `DefaultFocus` do ConfirmDialog: marcar os botões com `data-pc-focus="accept|reject|close"`
  (mesmo nome do upstream). `OpButton` já splata `AdditionalAttributes` no `<button>`, mas hoje o
  `OpConfirmDialog` **não repassa atributos** aos botões — adicionar parâmetros
  `AcceptButtonAttributes` / `RejectButtonAttributes` (ou equivalente) para o atributo chegar lá.
  `none` = não focar.
- Retornar o foco ao elemento que disparou ao fechar.
- Tudo em `OnAfterRenderAsync` com `try/catch`, para não quebrar SSR estático.
- **Paridade:** só modais prendem foco — o overlay de `Select`, `DatePicker` e `MultiSelect`
  não prende, igual ao upstream.
- Novo projeto de teste `tests/OpBlazorUI.Playwright` (novo csproj, adicionado à solução), rodando
  contra o Showcase. Cenários: ciclo de Tab, retorno ao trigger, `DefaultFocus="accept"`, console
  limpo. Observação: nenhum workflow roda testes hoje (o `ci.yml` só faz restore/build/pack);
  por ora são locais.
- Atualizar os Known Issues em
  `src/OpBlazorUI.Showcase/Components/Pages/ConfirmDialog.razor:224-241` e a docs de Dialog/Drawer.

---

## 5. PR 3 — Estado do repo

- `docs/roadmap.md`: substituir pelos números reais (47/88) e pela ordem dos lotes.
- `AppMenu.cs`: incluir os 11 faltantes que nem estão marcados — Dock, OrganizationChart,
  TabView, TabMenu, Steps, InlineMessage, InputChips, Slider, Knob, Terminal, Ripple.
- `docs/doc-coverage-analysis.md`: atualizar.

---

## 6. Verificação (por PR)

- `dotnet build OpBlazorUI.slnx` → 0 erros (sem novos warnings além dos 13 preexistentes).
- PR 1: form com `Required` / `MinLength` no Showcase, em modo claro e escuro; Showcase compila
  após a migração interna dos 5 arquivos listados na seção 3.3.
- PR 2: `tests/OpBlazorUI.Playwright` — Tab cicla dentro do diálogo, foco volta ao trigger,
  `DefaultFocus="accept"` foca o botão, console limpo.
- Paridade: diff de markup contra o upstream nos componentes tocados; a única mudança de DOM
  esperada é `data-pc-focus`.

---

## 7. Próximos lotes (após a fundação)

| Lote | Componentes |
|---|---|
| P0 — Formulário | Textarea, Fieldset, Slider, InputChips, Inplace, ColorPicker, FileUpload |
| P0 — Navegação | Tabs (+ TabMenu, TabView), Accordion, Breadcrumb, Toolbar, ContextMenu, Menubar, TieredMenu, PanelMenu, MegaMenu, Stepper (+ Steps) |
| P1 — Dados e feedback | Tree, TreeTable, ConfirmPopup, InlineMessage, DataView, OrderList, PickList, MeterGroup, Timeline |
| P2 — Layout e mídia | Panel, Splitter, ScrollPanel, Dock, Galleria, Carousel, Image, ImageCompare |
| P3 — Baixa prioridade | Knob, Terminal, OrganizationChart, Ripple |

Unstyled, RTL e preset Tailwind/PrimeFlex ficam fora dos lotes.

---

## 8. Fora de escopo

- Componentes novos (tratados nos lotes da seção 7, depois da fundação).
- Unstyled / RTL / PrimeFlex.
- Commits e PRs: só quando pedidos.