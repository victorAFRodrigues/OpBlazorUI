# Roadmap — Otimização de componentes (Optimus UI → OpBlazorUI)

> Fonte: doc oficial da Optimus UI (optimus.openng.org) cruzada com a biblioteca atual.
> Critério de priorização: **base usado por outros** → já tem CSS no tema (`aura.css`) → esforço baixo.

---

## Componentes já portados (31)

Button, CascadeSelect, Checkbox, DataTable, DatePicker, Editor, FloatLabel, Icon, IconField,
IftaLabel, InputGroup, InputIcon, InputNumber, InputOtp, InputText, KeyFilter (via InputText),
Listbox, Menu, Message, MultiSelect, Password, RadioButton, Rating, Select, SelectButton,
SpeedDial, SplitButton, ThemeSwitcher (ToggleSwitch), Toast, ToggleButton, Tooltip, TreeSelect.

---

## Tier 1 — Base de construção (próximo lote recomendado)

Pequenos, CSS já no tema, destravam loading/status/avatares e permitem refatorações.

| Componente | Usado por | CSS no tema |
|---|---|---|
| Skeleton | DataTable (loading), VirtualScroller (loader), Listbox/Tree (lazy), Card, Image | 17 |
| Badge (+ OverlayBadge) | Button (badge inline), Menu, Tabs, Avatar | 91 |
| Avatar (+ AvatarGroup) | Menu, Listbox, DataTable, Chip, comentários | 59 |
| Chip | MultiSelect/TreeSelect (display=chip — hoje inline), AutoComplete | ✓ |
| Tag | DataTable (status), standalone | 67 |
| ProgressSpinner | Button/Select/TreeSelect/DataTable (loading — hoje `pi-spinner` inline) | 23 |
| ProgressBar | DataTable (progresso), standalone | 28 |

### Refatorações habilitadas pelo Tier 1
- `OpMultiSelect` / `OpTreeSelect`: usar `OpChip` real em vez do markup `p-chip` inline.
- `OpButton`: ganhar slot de `Badge` (a doc do Button cita suporte embutido).
- `OpDataTable` / `OpSelect` / `OpTreeSelect`: usar `OpProgressSpinner` no estado de loading.
- `OpDataTable`: usar `OpSkeleton` no estado de carregamento das linhas.

---

## Tier 2 — Fundação de overlay (bloqueia o grupo Overlay)

| Componente | Observação |
|---|---|
| FocusTrap | ⚠️ sem CSS (utilitário invisível) |
| Overlay API | utilitário base de posicionamento |
| Popover | base de vários menus/overlays |
| Dialog | usa Overlay + FocusTrap |
| Drawer | usa Overlay + FocusTrap |

Necessários para: ConfirmDialog, ConfirmPopup, ContextMenu, Menubar, MegaMenu e para
posicionar corretamente os overlays dos selects (hoje `position:absolute` simplificado).

---

## Tier 3 — Data + Painel

| Componente | Observação |
|---|---|
| Paginator | hoje o DataTable não tem paginação (CSS: 74) |
| VirtualScroller | plano detalhado em `docs/virtualscroller-plan.md` |
| Divider | base de painel (CSS: 34) |
| Card | container (CSS: 25) |
| BlockUI | (CSS: 6) |
| ScrollTop | ⚠️ sem CSS no tema |

---

## Pendências já mapeadas

- [ ] **Tier 1** — Skeleton, Badge, Avatar, Chip, Tag, ProgressSpinner, ProgressBar.
- [ ] **Tier 2** — FocusTrap, Overlay API, Popover, Dialog, Drawer.
- [ ] **Tier 3** — Paginator, VirtualScroller, Divider, Card, BlockUI, ScrollTop.
- [ ] **VirtualScroller** — implementar conforme `docs/virtualscroller-plan.md` (Opção A, .NET-driven).

---

## Observações gerais

- O `aura.css` já contém classes + design tokens para a maioria dos componentes (skeleton, avatar,
  badge, tag, progressspinner, progressbar, divider, paginator, card, blockui, ripple, overlay).
- `focustrap` e `scrolltop` **não** têm CSS no tema (0 ocorrências) → exigiriam CSS próprio ou
  atualização do tema.
- Padrão de entrega: um commit por componente + tag + push, como nos lotes anteriores
  (v1.0.1, v1.1.0, v1.2.0).
