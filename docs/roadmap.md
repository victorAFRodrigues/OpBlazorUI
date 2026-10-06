# Roadmap — Estado dos componentes (Optimus UI → OpBlazorUI)

> Fonte canônica: catálogo `@openng/optimus-ui-themes/types` e a doc de referência
> ([v21.primeng.org](https://v21.primeng.org)).
> Plano de fundação (formulário + foco): `docs/plano-fundacao.md`.

---

## Estado atual

| Métrica | Valor |
|---|---|
| Componentes no upstream | 88 |
| Já implementados | 47 |
| Faltando | 41 (todos já têm CSS e tokens no tema) |

**Fundação concluída** (não adiciona componentes, mas muda o comportamento da biblioteca):

- Todos os inputs derivam de `OpInputBase<TValue>` (`Invalid`/`Disabled`/`StyleClass`,
  integração real com `EditForm`/`DataAnnotations`).
- Dialog/Drawer modais e ConfirmDialog prendem o foco (`Tab`) e restauram o foco ao gatilho.

**Documentação — Fase B concluída:** 46 páginas de placeholder para os 41 componentes faltantes +
5 utilitários (textos/exemplos da doc de referência, no PR #16). Detalhes e próximos passos em
`docs/plano-placeholders.md` e `docs/handoff.md`; a seguir, Fase A (melhoria das páginas existentes).

---

## Componentes faltantes (41) por lote

Ordem recomendada de implementação, do mais estruturante ao mais periférico.

### P0 — Formulário (7)
Textarea, Fieldset, Slider, InputChips, Inplace, ColorPicker, FileUpload.

### P0 — Navegação (13)
Tabs, TabMenu, TabView, Accordion, Breadcrumb, Toolbar, ContextMenu, Menubar, TieredMenu,
PanelMenu, MegaMenu, Stepper, Steps.

### P1 — Dados e feedback (9)
Tree, TreeTable, ConfirmPopup, InlineMessage, DataView, OrderList, PickList, MeterGroup, Timeline.

### P2 — Layout e mídia (8)
Panel, Splitter, ScrollPanel, Dock, Galleria, Carousel, Image, ImageCompare.

### P3 — Baixa prioridade (4)
Knob, Terminal, OrganizationChart, Ripple.

Fora dos lotes: Unstyled, RTL, preset PrimeFlex/Tailwind.

---

## Menu do Showcase

A sidebar (`Components/Layout/AppMenu.cs`) segue a categorização do PrimeNG e marca os itens ainda
não implementados como **em breve** (`/coming-soon/<slug>`). Os 11 itens que faltavam na lista já
foram incluídos: Dock, OrganizationChart, TabView, TabMenu, Steps, InlineMessage, InputChips, Slider,
Knob, Terminal, Ripple.

---

## Observações gerais

- Todos os 41 faltantes têm classe + tokens no tema; o custo é markup, API e comportamento.
- Padrão de entrega: fases aprovadas, build verde e conferência no Showcase (claro/escuro, console).
- Commits seguem `docs/padrao-de-commits.md`; tags `v*` publicam o pacote (ver `AGENTS.md`).
