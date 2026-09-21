# Plano de execução — VirtualScroller

> Status: **pendente** (plano salvo para execução futura)
> Componente: `OpVirtualScroller<TItem>`
> Motivação: performance ao lidar com grandes volumes de dados (Listbox, TreeSelect, DataTable, etc.)

---

## 1. Contexto / descobertas

- **Nenhum componente usa virtualização hoje.** Listbox, TreeSelect, MultiSelect e Select só
  aplicam `max-height` (renderizam tudo). DataTable não tem scroll nenhum.
- **JS interop é necessário.** O `@onscroll` do Blazor (`ScrollEventArgs`) não expõe
  `scrollTop`/`scrollLeft` — só `Type`, `TimeStamp`, `ClientX/Y`. Sem a posição do scroll não
  dá para calcular o intervalo visível.
- **Padrão de interop já existe no projeto** (copiar dele):
  - Módulos ES em `wwwroot/*.interop.js` (ex.: `optimus.interop.js`, `editor.interop.js`).
  - Carregados via `Js.InvokeAsync<IJSObjectReference>("import", "./_content/OpBlazorUI.Base/xxx.interop.js")`.
  - `DotNetObjectReference.Create(this)` + `[JSInvokable]` para callbacks JS→.NET.
- **CSS do tema está incompleto.** O `aura.css` só tem `.p-virtualscroller-loader` e
  `.p-virtualscroller-loading-icon` (e `.p-datatable-virtualscroller-spacer`, não usado).
  **Faltam** as classes-base `.p-virtualscroller`, `.p-virtualscroller-content` e
  `.p-virtualscroller-spacer` → definir no `.razor.css` escopado do componente (ou inline).

---

## 2. Decisão de arquitetura (fronteira JS↔.NET)

Escolhida a **Opção A — .NET-driven**, com tuning:

- **JS só reporta** `scrollTop`/`scrollLeft` (throttled via `requestAnimationFrame`) e oferece
  `scrollTo` + observação de resize (`ResizeObserver`).
- **C# calcula o intervalo** visível e re-renderiza só os itens do range.
- **Tuning:** conteúdo com `transform: translateY(...)` (GPU-composited); `StateHasChanged`
  **somente** quando `first`/`last` cruzam o limite de `numToleratedItems`.

### Por que A (e não B)

1. O diff do Blazor já é incremental — a árvore de render só tem ~30 itens visíveis.
2. O custo real (round-trip JS→.NET/frame) é controlável com rAF + detecção de mudança.
3. Consistência C#-centric (Listbox/TreeSelect/DataTable) + fácil de integrar e testar.
4. A Opção B (JS fazendo `translate` direto no DOM) só compensa em datasets 100k+ com DOM
   pesado em device fraco — não é o caso dos consumidores atuais.

> Se um dia o DataTable precisar de mais, migrar o núcleo para a B mantendo a interface do
> componente estável (não quebrar consumidores).

---

## 3. Componente `OpVirtualScroller<TItem>`

Arquivos:
- `src/Components/OpBlazorUI.Base/Components/VirtualScroller/OpVirtualScroller.razor`
- `.../OpVirtualScroller.razor.cs`
- `.../OpVirtualScroller.razor.css`
- `src/Components/OpBlazorUI.Base/wwwroot/virtualscroller.interop.js`

### 3.1 Parâmetros

| Nome | Tipo | Padrão | Descrição |
|------|------|--------|-----------|
| Items | `IReadOnlyList<TItem>?` | null | Dados exibidos. |
| ItemSize | `int` ou `int[]` | - | Altura/largura do item (array = grid `both`). |
| Orientation | `string` | vertical | `vertical` \| `horizontal` \| `both`. |
| ScrollHeight | `string?` | null | Altura do viewport. |
| ScrollWidth | `string?` | null | Largura do viewport. |
| Delay | `int` | 0 | Atraso do scroll antes de carregar. |
| ResizeDelay | `int` | 0 | Atraso após resize da janela. |
| Step | `int` | 0 | Quantos itens carregar por chunk no modo lazy. |
| Lazy | `bool` | false | Carrega dados sob demanda. |
| ShowLoader | `bool` | false | Exibe loader (máscara) durante carregamento. |
| Loading | `bool` | false | Indica carregamento. |
| NumToleratedItems | `int?` | null | Itens extras fora do viewport (default ≈ metade dos visíveis). |
| Disabled | `bool` | false | Elimina o scroller e renderiza o conteúdo direto. |
| AppendOnly | `bool` | false | Anexa itens sem remover (cuidado com dados grandes). |
| Inline | `bool` | false | Exibição inline. |
| AutoSize | `bool` | false | Altura/largura dinâmica do container. |
| StyleClass / Style | `string?` | null | Classes / estilo inline da raiz. |
| Id | `string?` | null | Id único. |
| TabIndex | `int` | 0 | Ordem de tabulação. |

### 3.2 Eventos

| Nome | Tipo | Descrição |
|------|------|-----------|
| OnLazyLoad | `EventCallback<VirtualScrollerLazyLoadEvent>` | `{ First, Last }` — chunk a carregar. |
| OnScroll | `EventCallback<VirtualScrollerScrollEvent>` | `{ Top, Left }` — posição do scroll. |
| OnScrollIndexChange | `EventCallback<VirtualScrollerIndexEvent>` | `{ First, Last }` — range visível. |

### 3.3 Templates

| Nome | Tipo | Descrição |
|------|------|-----------|
| Content | `RenderFragment<VirtualScrollerContentContext<TItem>>?` | Conteúdo custom (ctx: itens visíveis + `ScrollToIndex`). |
| Item | `RenderFragment<VirtualScrollerItemContext<TItem>>?` | Item (ctx: item, index, even/odd). |
| Loader | `RenderFragment?` | Loader customizado. |
| LoaderIcon | `RenderFragment?` | Ícone do loader. |

### 3.4 Método público

- `Task ScrollToIndexAsync(int index, string behavior = "auto")` → chama `scrollTo` no JS
  (`behavior`: `auto` \| `smooth`).

### 3.5 Cálculo do range (C#)

```
viewportSize  = (resize callback) ou parse de ScrollHeight/ScrollWidth
visibleCount  = ceil(viewportSize / itemSize) + 2 * numToleratedItems
first         = clamp(floor(scrollTop / itemSize) - numToleratedItems, 0, count)
last          = min(first + visibleCount, count)
```

---

## 4. Interop (`virtualscroller.interop.js`)

- `init(dotNet, id, element, options)`: registra scroll (rAF) + `ResizeObserver`; reporta
  `top/left` no scroll e `width/height` no resize.
- `getScrollPosition(id)` → `{ top, left }`.
- `scrollTo(id, top, left, behavior)` → define `scrollTop`/`scrollLeft`.
- `dispose(id)` → remove listeners.

Callbacks `[JSInvokable]` no .NET: `NotifyScroll(double top, double left)` e
`NotifyResize(double width, double height)`.

---

## 5. DOM

```
.p-virtualscroller            (scroll container, overflow:auto, height/width do viewport)
  ├ .p-virtualscroller-spacer (height/width = total = items.Count * itemSize)
  ├ conteúdo (transform: translateY/X(first*itemSize))  → itens visíveis
  └ .p-virtualscroller-loader / .p-virtualscroller-loading-icon
```

Grid (`both`): `ItemSize = [rowHeight, colWidth]`; colunas = `floor(viewportWidth / colWidth)`.

---

## 6. Página de doc (`src/OpBlazorUI.Showcase/Components/Pages/VirtualScroller.razor`)

Seções:
- Importação
- Acessibilidade (lista semântica, atributos repassados, sem interação embutida)
- Básico (100k itens)
- Delay
- Grid (`orientation="both"`, `ItemSize=[h,w]`)
- Horizontal
- Lazy (`Lazy` + `OnLazyLoad` + template `loadingItem`/skeleton)
- Loader (`ShowLoader` + `LoaderTemplate`)
- Programático (`ScrollToIndex`)
- Template (`Content`/`Item`/`Loader`/`LoaderIcon`)

---

## 7. Wiring + entrega

- `src/OpBlazorUI.Showcase/Components/Layout/AppMenu.cs`: `VirtualScroller` → `/virtualscroller`
  (já existe `GroupItem("VirtualScroller")` no grupo **Data**).
- `src/OpBlazorUI.Showcase/Components/Doc/DocTheming.cs`: entrada `virtualscroller`
  (classes + tokens) + palavras novas no dicionário (`scroller`, `spacer`, `loader`, etc.).
- `README.md`: adicionar `VirtualScroller`.
- Build (`dotnet build OpBlazorUI.slnx`), correção de erros.
- Commit, tag `v1.3.0` em `dev`, `git push origin dev && git push origin v1.3.0`.

---

## 8. Integração futura (fora deste escopo)

- Listbox (`virtualScroll`), TreeSelect (`virtualScroll`), MultiSelect/Select
  (`virtualScroll` + `scrollHeight`) e DataTable (virtual scrolling do body) passariam a
  envolver a lista no `OpVirtualScroller` em vez de `max-height`.
- Cada consumidor tem topologia própria (grupos, nós recursivos, linhas de tabela) → follow-up
  separado por componente.
