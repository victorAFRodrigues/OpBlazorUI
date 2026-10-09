# Bloco 5 — Infraestrutura

**Status:** proposta, aguardando aprovação. O `global.json` já foi ajustado no bloco 1.

**Objetivo:** nada é publicado sem compilar e testar, o pacote NuGet só leva a biblioteca e o
repositório não guarda arquivos de ferramentas locais. Branch sugerida: `chore/bloco5-infra`.

## 5.1 CI e publicação (`.github/workflows/`)

- [ ] `ci.yml`: adicionar `dotnet test` (com instalação do Chromium do Playwright). Se já tiver
  entrado no bloco 1, só conferir.
- [ ] `release.yml`: hoje publica no GitHub Packages a partir da tag `v*` sem compilar nem
  testar. Rodar build e testes antes do `pack`/`push`, ou depender do CI do mesmo commit.
- [ ] `deploy-pages.yml`: depender do CI em vez de publicar a cada push na `main`.
- [ ] Conferir no CI se os temas gerados (`wwwroot/themes/*.css`) estão em dia com
  `tools/theme-gen` (rodar o gerador e falhar se houver diff).
- [ ] `OpBlazorUI.Base.csproj`: `Version` fixo em `1.0.0`; derivar de tag ou de uma propriedade
  do CI para builds locais não gerarem 1.0.0.

## 5.2 Pacote NuGet

- [ ] `src/Components/OpBlazorUI.Base/Demo/`: 11 componentes de demonstração e a classe pública
  `Product` são compilados na DLL e vão para o pacote como API pública. Não são usados em lugar
  nenhum (o Showcase tem páginas próprias). Remover a pasta.
- [ ] Quill 1.3.7 vendorizado (`wwwroot/quill.min.js`, 216 KB, CVE-2021-3163) é carregado pelo
  `OpBlazorUiSetup` em todas as aplicações. Ver decisão 1.
- [ ] `LICENSE`: incluir o aviso da licença BSD-3 do Quill.
- [ ] `README.md`: o link relativo `../../releases` quebra no README empacotado.

## 5.3 Repositório

- [ ] `graphify-out/` (124 arquivos, 5,6 MB, com caminhos locais da máquina do autor) e
  `.opencode/`: remover do controle de versão (`git rm --cached`) e adicionar ao `.gitignore`.
- [ ] `README.md`: a lista de componentes não bate com o código (lista InputMask e KeyFilter, que
  são parâmetros do `OpInputText`; ToggleSwitch se chama `OpInputSwitch`; omite AutoComplete,
  ConfirmDialog e InputIcon e os componentes dos lotes P0/P1).
- [ ] `tests/OpBlazorUI.Playwright/README.md`: atualizar "O que cobre" e a seção de CI.

## 5.4 Showcase

- [ ] `Components/Layout/AppMenu.cs` + `Sidebar.razor`: o selo "Portado" aparece para qualquer
  item com rota, inclusive páginas placeholder. Marcar as páginas não portadas explicitamente.

## Decisões em aberto

1. **Quill:** (a) atualizar para Quill 2.x e carregar o script sob demanda pelo próprio Editor;
   (b) manter 1.3.7 e só carregar sob demanda. Recomendo (a): o PrimeNG 21 usa Quill 2.
2. **`graphify-out/` e `.opencode/`:** remover do repositório ou manter versionado por algum
   motivo? Recomendo remover.
3. **Release:** depender do CI (job `needs` via `workflow_run`) ou repetir build/test no próprio
   `release.yml`? Recomendo repetir: é mais simples e não depende da ordem dos workflows.
