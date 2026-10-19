---
name: OpBlazorUI
description: Use ao escrever código Blazor com a biblioteca OpBlazorUI (componentes Op*, temas Aura/Lara/Nora, pacote OpBlazorUI): instalação, formulários, DataTable, overlays, temas e ícones. Consulte llms.txt e o manifesto antes de inventar propriedades.
---

# OpBlazorUI

Biblioteca Blazor (Razor Class Library, .NET 10) que porta o Optimus UI — fork MIT do
PrimeNG v21. Os componentes usam o prefixo `Op*`, as classes CSS são `p-*` e há três temas:
Aura (padrão), Lara e Nora.

## Fluxo de trabalho

1. Localize o componente no índice: `<base>/llms.txt`.
2. Leia o Markdown do componente: `<base>/llms/<id>.md`.
3. Confirme nomes, tipos e valores padrão no manifesto: `<base>/llms/manifest.json`.
4. Se a tarefa casar com uma receita, leia o arquivo correspondente em `references/`.
5. Escreva o código seguindo os exemplos e compile (`dotnet build`).

Base padrão: <https://victorafrodrigues.github.io/OpBlazorUI>. Para fixar uma versão, use o
`llms-full.txt` e o `manifest.json` anexados à release correspondente.

## Receitas

- [Instalação e render modes](references/instalacao-e-render-modes.md)
- [Formulários e validação](references/formularios-e-validacao.md)
- [DataTable (CRUD)](references/datatable-crud.md)
- [Overlays e feedback](references/overlays-e-feedback.md)
- [Temas e ícones](references/temas-e-icones.md)

## Convenções

- Tipos: `OpButton`, `OpDataTable<TItem>`, `OpDialog`... Namespace `OpBlazorUI.Components.<Nome>`.
- Parâmetros em PascalCase (`Value`, `Placeholder`, `StyleClass`); binding com `@bind-Value`;
  callbacks `OnXxx`; templates `XxxTemplate` e `ChildContent`.
- `StyleClass` adiciona classes ao elemento raiz; `Style`/`Width`/`Height` são inline.
- Severidades: `primary`, `secondary`, `success`, `info`, `warn`, `danger`, `contrast`.
- Ícones: `<i class="pi pi-search"></i>` (fonte nativa; não crie SVGs). A lista de glyphs
  está em `<base>/llms/icons.md`.
- Classes CSS dos componentes são `p-*`; utilitários de layout (flex, grid, espaçamento) são
  responsabilidade da aplicação.
- Textos e descrições em pt-BR; nomes de tipos, propriedades e classes em inglês.

## Armadilhas comuns

- Não escreva componentes "de memória": confirme os parâmetros no manifesto. Propriedade
  inexistente quebra a compilação.
- O pacote está no NuGet.org: `dotnet add package OpBlazorUI` não exige feed nem PAT.
- `<OpBlazorUiSetup />` precisa estar na parte interativa da aplicação (não em SSR estático).
- Estado sempre *scoped*; nunca `static` — a lib funciona em Server, WebAssembly e Auto.
- O tema é aplicado por `op-theme.js` + `<OpBlazorUiSetup />`; o modo escuro é a classe
  `app-dark` no `<html>`.
- `OpToast`, `OpConfirmDialog` e `OpDynamicDialogHost` precisam estar montados no layout
  (uma vez) e os serviços registrados por `AddOpBlazorUI()`.
- `OpDataTable`: `Items` + `Columns` (lista de `OpDataTableColumn`); use `DataKey` para
  seleção, expansão e edição.
- Não edite os CSS de `wwwroot/themes/*.css` à mão: personalize pelas variáveis `--p-*` ou
  troque o preset.
