# Fase 5 — Auditoria de CSS (classes, inlineStyles e visual)

Branch: `fix/fase5-auditoria-css` (a partir de `fix/fase4-5-render-modes`). Ainda **não enviada** ao GitHub.

Objetivo: alinhar DOM, classes CSS e estilos inline dos componentes ao upstream Angular
(Optimus UI, `packages/optimus-ui/src/<nome>/style/<nome>style.ts` + template em `<nome>.ts`),
já que o port copiou o CSS do tema mas não o que os componentes fazem em runtime.

## Commits

1. `fix(css): alinha classes e estilos inline de botoes e componentes misc ao upstream`
2. `fix(css): CSS extra dos componentes Angular no gerador, overlays e inputs` (este documento)

## O que foi feito

### Botões e componentes diversos (commit 1)

| Componente | Correção |
|---|---|
| SpeedDial | Classes `p-speeddial-{type}`, `p-speeddial-direction-{dir}` (exceto circle), `p-disabled`; inlineStyles de direção (flex-direction/align) no root e na lista; posicionamento circular/semi/quarter do upstream (`left/top/right/bottom` + `--item-diff-x/y` medidos via JS `speedDialItemDiff`); raio padrão `itens × 20`; ações `secondary`/`small`/`rounded`, desabilitadas por item; `p-hidden` para item invisível; máscara `p-speeddial-mask p-overlay-mask` fora do root; fechar ao clicar fora (`HideOnClickOutside`, antes ignorado); `transition-delay` invertido ao fechar. **Breaking:** `ItemTemplate` agora recebe `SpeedDialItemContext` (`Item`, `Index`, `Click`); `ButtonRounded` removido (sempre arredondado). **Bug:** re-render do pai (ex.: `OnClick` do `ButtonTemplate`) fechava o menu logo após abrir — `Visible` só sincroniza quando o parâmetro muda. |
| SplitButton | Root com `p-splitbutton-outlined/-text/-sm/-lg`; Raised/Rounded/Plain não vão mais para os botões internos (`Plain` removido); dropdown sempre `p-button-icon-only`. **Bug:** o label nunca aparecia (o `@ContentTemplate` nulo virava `ChildContent`). |
| Button | `p-button-icon-only` não se aplica com badge; `LoadingIcon` conta como ícone; severidade em minúsculas; spinner com `p-button-loading-icon p-button-icon p-button-icon-{pos}`. |
| ProgressBar | Indeterminado sem label; determinado sempre com `p-progressbar-label`, texto oculto em 0, template dentro do label. |
| Avatar | `p-avatar-label` (era `p-avatar-text`); prioridade label > ícone > imagem; `ChildContent` junto. |
| Tag | `IconTemplate` independe de `Icon`; `ChildContent` junto do conteúdo. |
| Message | Estrutura `root > p-message-content-wrapper > p-message-content > (ícone, texto, fechar)`; `ContainerTemplate` agora funciona; severidade em minúsculas. |
| Card | Removido o wrapper `p-card-caption` (não existe no Angular). |
| OverlayBadge | Sem `p-component`; `StyleClass/Style/BadgeSize/BadgeDisabled` vão para o badge interno. |
| ProgressSpinner | Sem `p-component`; `aria-busy`. |
| Badge | `p-badge-circle` / `p-badge-dot`; desabilitado = `display:none` (como `badgeDisabled`). |
| Divider | inlineStyles de alinhamento (justify/align) e classe padrão iguais ao upstream; regras do `optimus-base.css` removidas. |
| Menu | `p-focus` não é mais aplicado no hover (ficava preso); limpo no blur da lista. |
| Table | Container com `overflow:auto` e `thead` sticky (inlineStyles). |

### Gerador de temas: CSS extra dos componentes Angular (commit 2)

Causa raiz de vários bugs: 36 arquivos `style/*style.ts` do Angular injetam CSS que **não** está em
`@openng/optimus-ui-styles` (zebrado da Table, loader do Scroller, Drawer posicionado, `.p-card{display:block}`…).

- `tools/theme-gen/extract-extras.mjs` (`npm run extract -- <clone do optimus-ui>`) extrai esse CSS para
  `tools/theme-gen/component-extras.json` (versionado, com o commit de origem — hoje `9d54a87`).
- `generate.mjs` anexa esse CSS ao fim de cada tema (dt() resolvido por preset). Regras exclusivas do Angular
  (`p-button`, `.ng-invalid`) simplesmente não casam. Ficam de fora (`SKIP_RULES`) as que brigam com o
  posicionamento fixed do `overlay.interop.js`: `.p-component-overlay.p-component` e `.p-password-overlay`.
- Temas regenerados (`aura/lara/nora.css`, +~1300 linhas cada).

### Overlays e dados (commit 2)

| Componente | Correção |
|---|---|
| Dialog | Máscara escura (`p-overlay-mask`) **só quando `Modal`**; sem modal, a máscara tem `pointer-events:none` e o dialog `auto` (a página continua clicável); `DismissableMask` só vale com modal; removido o `padding:1rem` da máscara (não existe no upstream). |
| Drawer | DOM igual ao Angular: o drawer se posiciona sozinho (`p-drawer-{pos}`, `p-drawer-enter-{pos}`, `p-drawer-full`) e a máscara é um irmão que só existe com `Modal`. Novo `OpOverlayAttach MaskPrevious`: a máscara recebe o z-index do drawer − 1 (antes a ordem era aleatória e a máscara podia cobrir o drawer em tela cheia). |
| Toast | Botão de fechar envolto em `<div>` (as margens em % do tema dependem disso). |
| Paginator | `p-disabled` em first/prev/next/last (opacidade e sem hover). |
| Table | Paginador interno com janela de 5 páginas (`PageLinkSize`); ícones de sort `sort-alt` / `sort-amount-up-alt` / `sort-amount-down`; zebrado agora funciona (CSS extra). |
| VirtualScroller | Classes `p-virtualscroller-horizontal p-horizontal-scroll` / `-both p-both-scroll`; loader com `p-virtualscroller-loader-mask`; regras próprias do `optimus-base.css` removidas em favor das do upstream. |

### Inputs (commit 2)

| Componente | Correção |
|---|---|
| OpIcon | Sempre emite `p-icon` + a classe da seção (o tamanho via `--p-icon-size` e as regras sm/lg voltaram a valer nos ~28 usos). |
| Select / MultiSelect / AutoComplete / Listbox | `ScrollHeight` padrão do upstream (`200px`; Listbox `14rem`) — listas longas não crescem mais sem limite. |
| InputNumber | Root `p-inputwrapper`, `-filled`, `-focus` (FloatLabel flutua); classe de layout só com `ShowButtons` (antes sempre `stacked`, com padding extra); `p-variant-filled`, `p-filled` e `p-inputtext-fluid` no input; sem `p-disabled` no root (opacidade dobrada). |
| InputOtp | **Bug:** os inputs usavam a classe crua `InputClass` e ficavam sem estilo; agora `p-inputotp-input p-inputtext` + `p-filled`/variant por input; root sem `p-disabled`/variant. |
| Password | Sem `p-disabled` no root (opacidade dobrada). |
| AutoComplete / DatePicker | `p-inputtext-fluid` no input com `Fluid`. |
| DatePicker | Com `ShowOtherMonths=false`, a célula `<td>` continua existindo (as semanas não se deslocam mais). |
| MultiSelect | Cabeçalho por padrão (`ShowHeader`) com checkbox de selecionar todos (`ShowToggleAll`); filtro em `p-multiselect-filter-container`; `p-multiselect-option-selected` (`HighlightOnSelect`). |
| Listbox | Checkboxes com `p-listbox-option-check-icon`; toggle-all só com `Checkbox` e antes do filtro. |
| TreeSelect | Root com `p-inputwrapper`, `-filled`, `-focus`, `p-treeselect-open`, `-clearable`, `-display-chip`, `p-inputfield-sm/lg`. |
| Checkbox | Indeterminado não recebe `p-checkbox-checked`. |
| Chip | Imagem **ou** ícone (não os dois). |

Também: README com a API atual de temas (`OpThemeService`).

## Verificação

- Build do Showcase: 0 erros (12 avisos pré-existentes de nulabilidade/campo não usado, nenhum novo).
- Visual (Edge headless, Showcase na porta 5199, tema escuro): SpeedDial (todos os tipos/direções, máscara,
  template, clique fora), SplitButton, ProgressBar, Message, Avatar, Divider, Drawer (lateral e tela cheia,
  ordem de z-index), Dialog sem máscara, Table (zebrado, paginador, sort), VirtualScroller (vertical,
  horizontal, grid), MultiSelect aberto.
- Não verificados visualmente: InputNumber, InputOtp, Listbox, TreeSelect, DatePicker (outros meses), Toast,
  Paginator, modo claro, presets Lara/Nora. Comparação com optimus.openng.org não foi possível (proxy exige autenticação).

## O que faltou

1. **Ícones SVG incompletos no `OpIcon`**: `sort-alt` tem só 1 dos 4 paths do upstream (na Table o ícone de
   "não ordenado" aparece como um "^"). Provavelmente outros ícones também foram portados só com o primeiro path.
   Próximo passo: script que compara cada `case` do `OpIcon.razor` com
   `packages/optimus-ui/src/icons/<nome>/<nome>.ts` e copia os paths que faltam.
2. Verificação visual do que ficou pendente (lista acima), em claro e escuro e nos presets Lara/Nora.
3. Itens LOW das auditorias, não aplicados:
   - Dialog: botões do header como `p-button` text/rounded/secondary (hoje `p-dialog-header-icon` próprio).
   - Paginator: rows-per-page / jump-to-page com `OpSelect`/`OpInputNumber` em vez de `select`/`input` nativos.
   - BlockUI: modo documento (`p-blockui-mask-document`, fixed) quando não há alvo.
   - Tooltip: o wrapper `op-tooltip-wrapper` (inline-block) pode alterar o layout de alvos em bloco.
   - AutoComplete: `p-focus` no chip focado.
   - TreeSelect: chips em `p-treeselect-chip-item`, sem botão de remover; painel com `p-component-overlay`.
   - Filtros de Select/MultiSelect/TreeSelect não seguem `Size`/variante filled.
   - Password: overlay sem `p-component`.
   - Rating: `p-focus-visible` para o anel de foco pelo teclado.
   - Listbox: parâmetro `Fluid` (`p-listbox-fluid`).
   - Chip: `tabindex` no root que o upstream não tem.
4. Pendências antigas: fechar o Menu popup ao clicar fora; o `OpOverlay` genérico ainda é só CSS;
   `&nbsp;` literal no ToggleButton; problema do backspace na máscara (InputMask).
5. Fase 6 (docs): plano já aprovado (lotes 0–6, ApiTable por reflexão + `/// <summary>`, exemplos da Table).

## Como retomar

```bash
git checkout fix/fase5-auditoria-css
git push -u origin fix/fase5-auditoria-css     # a branch ainda não foi enviada

# regenerar temas (após atualizar o clone do optimus-ui)
cd tools/theme-gen
npm install
npm run extract -- <caminho do clone de openng-org/optimus-ui>
npm run build
```
