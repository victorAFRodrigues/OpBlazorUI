# Bloco 5 — Infraestrutura

**Status:** implementado (branch `chore/bloco5-infra`).

**Objetivo:** nada é publicado sem compilar e testar, o pacote NuGet só leva a biblioteca e o
repositório não guarda arquivos de ferramentas locais.

## 5.1 CI e publicação (`.github/workflows/`)

- [x] `ci.yml`: `dotnet test` com Chromium do Playwright (já existia); adicionado o job `themes` que
  regenera e falha se houver diff; o job `build` passou a empacotar também o `OpBlazorUI.Charts`.
- [x] `release.yml`: roda build (Debug) + testes do Playwright antes do `pack`/`push`; empacota Base
  e Charts e publica ambos.
- [x] `deploy-pages.yml`: passou a depender do CI (`workflow_run` de `CI` na `main`, só quando
  `conclusion == success`).
- [x] Conferência dos temas gerados no CI (job `themes`).
- [x] `Version` dos csproj: padrão `0.0.0-dev`; o release injeta a versão da tag via `-p:Version`.

## 5.2 Pacote NuGet

- [x] `src/Components/OpBlazorUI.Base/Demo/`: removida (11 componentes de demonstração + `Product`).
- [x] Quill atualizado para **2.0.3** e carregado **sob demanda** pelo `editor.interop.js` (o
  `<script>` global saiu do `OpBlazorUiSetup`; o CSS `quill.snow.css` continua global).
- [x] `LICENSE`: aviso da BSD-3 do Quill.
- [x] `README.md`: link de Releases trocado por URL absoluta (não quebra no pacote).

## 5.3 Repositório

- [x] `README.md`: lista de componentes corrigida (removidos `InputMask`/`KeyFilter`, que são
  parâmetros do `OpInputText`; `ToggleSwitch` → `InputSwitch`; incluídos AutoComplete,
  ConfirmDialog, ConfirmPopup, DynamicDialog, InputIcon e outros).
- [x] `tests/OpBlazorUI.Playwright/README.md`: seção "O que cobre" atualizada (blocos 1–4).
- [ ] **Decisão:** `graphify-out/` e `.opencode/` **mantidos** versionados (não removidos).

## 5.4 Showcase

- [x] `AppMenu`/`Sidebar`: páginas-placeholder (Carousel, Fluid, Galleria, Image, ImageCompare,
  Knob, Panel, ScrollPanel, Splitter) não exibem mais o selo "Portado".

## Decisões (resolvidas)

1. **Quill:** atualizar para 2.x e carregar sob demanda. ✅
2. **`graphify-out/` e `.opencode/`:** manter versionados. ✅
3. **Release:** repetir build+teste no próprio `release.yml`. ✅
