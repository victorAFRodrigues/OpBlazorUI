# Análise de Cobertura da Documentação — OpBlazorUI Showcase

> Comparação página a página contra a referência OptimusUI/PrimeNG v21 (v21.primeng.org)
> Modelo de referência: `src/OpBlazorUI.Showcase/Components/Pages/Button.razor` (762 linhas, 21 seções, checklist §5 completo)

---

## Atualização — fundação de formulário (ver `docs/plano-fundacao.md`)

- As páginas de input passaram a ter a seção **Validação/Formulários** (`EditForm` +
  `DataAnnotationsValidator` + `ValidationMessage`): InputText, InputNumber, Checkbox, ToggleSwitch,
  ToggleButton, Select, MultiSelect, InputOtp, SelectButton (além das que já tinham: Password, Rating,
  Listbox, TreeSelect).
- O item **Forms** das tabelas de lacunas abaixo pode ser considerado **atendido** para essas páginas;
  a lacuna transversal restante é **Accessibility** (a maioria das páginas segue sem leitor de tela +
  tabela de teclado).
- `RadioButton` fica fora do `InputBase` (grupo N→1) e `SelectButton` múltiplo também — limitação
  documentada no playbook (§3.1).

---

## Resumo Executivo

| Categoria | Páginas | Status |
|-----------|---------|--------|
| **Modelo ouro** | 1 (Button) | ✅ Completa |
| **Boas** | 5 (AutoComplete, InputNumber, Password, SplitButton, Select*) | ⚠️ Quase completas |
| **Médias** | 12 | ❌ Faltam seções importantes |
| **Críticas/Pobres** | 15+ | ❌ Muitas seções upstream ausentes |

*Select tem 23 seções upstream, showcase tem apenas 6

---

## Comparação Detalhada por Componente

### ❌ CRÍTICAS — Mais usadas + maiores lacunas

| Componente | Upstream | Showcase | % | Lacunas Principais |
|------------|----------|----------|---|-------------------|
| **InputText** | 16 | 5 | 31% | FloatLabel, IftaLabel, Fluid, HelpText, Icons, KeyFilter, **Forms**, **Accessibility**, Style |
| **Checkbox** | 14 | 4 | 29% | Group (label), Dynamic, Filled, **Forms**, **Accessibility**, Style |
| **RadioButton** | 11 | 4 | 36% | Group (principal!), Dynamic, Filled, **Forms**, **Accessibility**, Style |
| **ToggleSwitch** | 10 | 3 | 30% | Preselection, Template (HandleTemplate), **Forms**, **Accessibility**, Style |
| **DatePicker** | **31** | 6 | 19% | Format, Locale, MinMax, Multiple, DateTemplate, Float/IftaLabel, ClearIcon, Sizes, Fluid, Filled, Disabled, Invalid, **Forms**, **Accessibility**, Events, Methods, TouchUI, Templates, Style |
| **Select** | **23** | 6 | 26% | Checkmark, Editable, Loading, VirtualScroll, LazyVirtualScroll, Float/IftaLabel, ClearIcon, Sizes, Fluid, **Forms**, **Accessibility**, Style |
| **MultiSelect** | **20** | 5 | 25% | Loading, VirtualScroll, Float/IftaLabel, ClearIcon, Sizes, Fluid, **Forms**, Templates (header/footer/filter), **Accessibility**, Style |
| **AutoComplete** | 23+ | 17 | 74% | Loading, Style, TemplateDriven/Reactive Forms (tem EditForm mas separado) |
| **Message** | 12 | 7 | 58% | Forms (validação), Dynamic, **Accessibility**, Style |
| **Toast** | 14 | 8 | 57% | Clear, Headless, Responsive (breakpoints), Target/Key, **Accessibility**, Style |

### ⚠️ MÉDIAS — Faltam seções importantes

| Componente | Upstream | Showcase | Lacunas |
|------------|----------|----------|---------|
| **Dialog** | 13 | 6 | LongContent, Responsive, Headless, **Accessibility**, Style, OverlaysInside |
| **Drawer** | 9 | 5 | Size (responsivo), Headless, **Accessibility**, Style |
| **BlockUI** | 5 | 3 | Document mode, **Accessibility**, Style |
| **Tooltip** | 9 | 4 | Event (hover/focus), AutoHide, Custom (template), Options, **Accessibility**, Style |
| **ProgressBar** | 7 | 5 | Dynamic, **Accessibility**, Style |
| **ProgressSpinner** | 5 | 2 | **Accessibility**, Style |
| **Skeleton** | 7 | 3 | Card, List, DataTable, **Accessibility** (mínima), Style |
| **Badge** | 10 | 4 | Button badge, Position (p-overlay-badge), **Accessibility**, Style |
| **Tag** | 8 | 5 | Template, **Accessibility**, Style |
| **Chip** | 7 | 4 | Template, **Accessibility**, Style |
| **KeyFilter** | 4 | 4 | Accessibility só redireciona p/ InputText |
| **SplitButton** | ~15 | 15 | ✅ Boa cobertura (tem Accessibility) |
| **Password** | ~15 | 14 | ✅ Muito boa (Accessibility, Forms, Templates) |
| **InputNumber** | ~15 | 14 | ✅ Excelente (Accessibility, FloatLabel, IftaLabel) |

---

## Checklist do Playbook (§5) — O que mais falha

| Item do Checklist | Cumprem | Falham (maioria) |
|-------------------|---------|------------------|
| Todas seções upstream na mesma ordem | Button, AutoComplete, InputNumber, Password, SplitButton | **Todas as demais** |
| Toda seção tem `Description` | Button, AutoComplete, InputNumber, Password, Select (parcial) | InputText, Checkbox, RadioButton, ToggleSwitch, Chip, Tag, Badge, ProgressBar, ProgressSpinner, Tooltip, KeyFilter, Message |
| Seções extras Blazor (Binding, Forms, Events, Fluid, Render modes) | Button, AutoComplete, InputNumber, Password, Select | ToggleSwitch, Checkbox, RadioButton, InputText, FloatLabel, Message, Toast |
| **Acessibilidade: leitor + tabela teclado** | **Só**: Button, AutoComplete, InputNumber, Password, FloatLabel (parcial), Skeleton (parcial) | **27+ páginas SEM** |
| API: todos `[Parameter]`/eventos/templates do `.razor.cs` | Button, AutoComplete, Select, InputNumber, Password, SplitButton, DatePicker, Dialog, Drawer | ToggleSwitch, Checkbox, RadioButton, Chip, Tag, Badge, ProgressBar, ProgressSpinner, Tooltip, KeyFilter, Message |
| Temas: Classes CSS + `DocTokens` | Quase todas (usam DocTokens) | — |
| KnownIssues com lacunas reais | Button, Dialog, BlockUI, AutoComplete | **Quase todas dizem "Nenhum problema conhecido"** |

---

## Plano de Ação por Lotes (Playbook §6)

### Lote 1 — Form Básico (definem padrão Binding/Forms/Accessibility)

| Página | Seções Upstream Faltando | Seções Blazor Extras Necessárias | Prioridade |
|--------|--------------------------|----------------------------------|------------|
| **InputText** | FloatLabel, IftaLabel, Fluid, HelpText, Icons, KeyFilter, Forms | Binding (`@bind-Value`), Forms (EditForm + DataAnnotations), Events | 🔴 CRÍTICA |
| **Checkbox** | Group (label), Dynamic, Filled, Forms | Binding (`@bind-Checked`), Forms, Events | 🔴 CRÍTICA |
| **RadioButton** | Group (principal!), Dynamic, Filled, Forms | Binding (`@bind-ModelValue`), Forms, Events | 🔴 CRÍTICA |
| **ToggleSwitch** | Preselection, Template (HandleTemplate), Forms | Binding (`@bind-Checked`), Forms, Events | 🔴 CRÍTICA |
| **FloatLabel** | Style | Events, Templates | 🟡 MÉDIA |
| **InputNumber** | ✅ (já boa) | — | ✅ OK |
| **Password** | ✅ (já boa) | — | ✅ OK |

### Lote 2 — Seleção Complexa (maiores, mais usadas)

| Página | Seções Upstream Faltando Críticas | Prioridade |
|--------|-----------------------------------|------------|
| **DatePicker** | 31→6: Format, Locale, MinMax, Multiple, DateTemplate, Float/IftaLabel, ClearIcon, Sizes, Fluid, Filled, Disabled, Invalid, **Forms**, **Accessibility**, Events, Methods, TouchUI, Templates | 🔴 CRÍTICA |
| **Select** | 23→6: Checkmark, Editable, Loading, VirtualScroll, Float/IftaLabel, ClearIcon, Sizes, Fluid, **Forms**, **Accessibility**, Style | 🔴 CRÍTICA |
| **MultiSelect** | 20→5: Loading, VirtualScroll, Float/IftaLabel, ClearIcon, Sizes, Fluid, **Forms**, Templates, **Accessibility**, Style | 🔴 CRÍTICA |
| **AutoComplete** | Loading, Style (já tem 17/23+) | 🟡 MÉDIA |

### Lote 3 — Botões (reaproveita textos do Button)
- ToggleButton, SelectButton, SplitButton ✅, SpeedDial

### Lote 4 — Overlay/Feedback (nota render modes)

| Página | Lacunas Críticas | Prioridade |
|--------|------------------|------------|
| **Dialog** | LongContent, Responsive, Headless, **Accessibility**, Style, OverlaysInside | 🟡 MÉDIA |
| **Drawer** | Size, Headless, **Accessibility**, Style | 🟡 MÉDIA |
| **Tooltip** | Event, AutoHide, Custom, Options, **Accessibility**, Style | 🟡 MÉDIA |
| **Toast** | Clear, Headless, Responsive, Target/Key, **Accessibility**, Style | 🟡 MÉDIA |
| **Message** | Forms, Dynamic, **Accessibility**, Style | 🟡 MÉDIA |
| **BlockUI** | Document, **Accessibility**, Style | 🟡 MÉDIA |
| **Popover** | (verificar upstream) | 🟢 BAIXA |

### Lote 5 — Dados/Misc

| Página | Lacunas | Prioridade |
|--------|---------|------------|
| **Table** | Básica — sorting, filtering, paging, selection, templates, virtual scroll, export, **Accessibility** | 🔴 CRÍTICA |
| **Badge/Tag/Chip** | **Accessibility**, Style, Templates | 🟡 MÉDIA |
| **ProgressBar/Spinner** | **Accessibility**, Style | 🟡 MÉDIA |
| **Skeleton** | Card, List, DataTable, Style | 🟡 MÉDIA |
| **Menu** | Básica — templates, popup, **Accessibility** | 🟡 MÉDIA |

---

## Como Executar por Componente

```bash
# 1. Baixar referência upstream
bash tools/doc-upstream/fetch.sh <componente> 21.0.0

# 2. Inventariar componente Blazor
# Ler src/Components/OpBlazorUI.Base/Components/<Componente>/Op<Componente>.razor.cs
# Listar TODOS os [Parameter], EventCallback, RenderFragment

# 3. Montar matriz de cobertura (playbook §3)

# 4. Escrever página seguindo esqueleto (playbook §2)

# 5. Verificar
dotnet build OpBlazorUI.slnx
dotnet run --project src/OpBlazorUI.Showcase
# Testar: tema claro/escuro, console sem erros, demos interativas
```

---

## Mapeamento de Nomes (PrimeNG → OpBlazorUI)

| PrimeNG (upstream) | OpBlazorUI | Comando fetch.sh |
|---------------------|------------|------------------|
| button | Button | `button` |
| inputtext | InputText | `inputtext` |
| checkbox | Checkbox | `checkbox` |
| toggleswitch / inputswitch | ToggleSwitch | `toggleswitch` |
| radiobutton | RadioButton | `radiobutton` |
| floatlabel | FloatLabel | `floatlabel` |
| inputnumber | InputNumber | `inputnumber` |
| password | Password | `password` |
| datepicker / calendar | DatePicker | `datepicker` |
| select / dropdown | Select | `select` |
| multiselect | MultiSelect | `multiselect` |
| autocomplete | AutoComplete | `autocomplete` |
| dialog | Dialog | `dialog` |
| drawer / sidebar | Drawer | `drawer` |
| blockui | BlockUI | `blockui` |
| tooltip | Tooltip | `tooltip` |
| progressbar | ProgressBar | `progressbar` |
| progressspinner | ProgressSpinner | `progressspinner` |
| skeleton | Skeleton | `skeleton` |
| badge | Badge | `badge` |
| tag | Tag | `tag` |
| chip | Chip | `chip` |
| keyfilter | KeyFilter | `keyfilter` |
| splitbutton | SplitButton | `splitbutton` |
| message | Message | `message` |
| toast | Toast | `toast` |
| table / datatable | Table / DataTable | `table` |
| menu | Menu | `menu` |

---

## Decisões Pendentes

1. **Ordem de execução**: Começar pelo Lote 1 (InputText, Checkbox, RadioButton, ToggleSwitch, FloatLabel) sequencialmente?
2. **Escopo por PR**: Um componente por PR ou agrupar família (ex.: Checkbox+RadioButton+ToggleSwitch juntos)?
3. **KnownIssues reais**: Documentar lacunas como `OpButtonGroup` não existe, `pButton` directive não tem equivalente?
4. **Accessibility**: Copiar texto exato do upstream (screen reader + keyboard support tables) via `fetch.sh` e adaptar para Blazor?
5. **Componentes sem upstream direto**: Como tratar `OpInputGroup`, `OpIconField`, `OpInputOtp`, `OpInputMask`, `OpRating`, `OpEditor`, `OpCascadeSelect`, `OpTreeSelect`, `OpListbox`, `OpVirtualScroller`, `OpPaginator`, `OpScrollTop`, `OpFocusTrap`, `OpCard`, `OpDivider`, `OpAvatar`?

---

## Próximo Passo Sugerido

Iniciar com **InputText.razor** (Lote 1) — é a base para vários componentes e define o padrão de Binding/Forms/Accessibility.

```bash
bash tools/doc-upstream/fetch.sh inputtext 21.0.0
# Analisar tools/doc-upstream/out/inputtext/
# Atualizar src/OpBlazorUI.Showcase/Components/Pages/InputText.razor
```