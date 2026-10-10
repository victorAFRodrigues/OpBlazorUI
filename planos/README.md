# Planos de correção — auditoria de outubro/2026

Em 08/10/2026 o projeto inteiro (`main` em `3aea800`) foi auditado em quatro frentes: inputs,
overlays e serviços, componentes novos (navegação e dados) e infraestrutura. Os achados foram
agrupados em cinco blocos, executados nesta ordem. Cada bloco tem o próprio arquivo com as
tarefas, os arquivos envolvidos e as decisões em aberto.

| Bloco | Tema | Status |
|---|---|---|
| [1](bloco-1-base-comum.md) | Base comum: descarte, interop JS, clique fora/Escape, cultura | Implementado em `fix/bloco1-base`; testes Playwright 5/5 (falta verificação manual em Server) |
| [2](bloco-2-excecoes-e-travamentos.md) | Exceções que derrubam o circuito e travamentos | Implementado em `fix/bloco2-excecoes` (empilhado no bloco 1); testes 6/6 |
| [3](bloco-3-funcionalidade.md) | Funcionalidade quebrada por componente | Implementado (3.1–3.9) em stack de branches; testes 110 verdes |
| [4](bloco-4-teclado-acessibilidade-e-recursos.md) | Teclado, ARIA e recursos que faltam (comparado ao PrimeNG) | Em execução (4.0 feito) |
| [5](bloco-5-infraestrutura.md) | CI, release, pacote NuGet e repositório | Proposta |

## Como os blocos são executados

- Um plano só é executado depois de aprovado; as decisões em aberto de cada arquivo são
  respondidas antes de começar.
- Uma branch por bloco (`fix/blocoN-...`), um commit por etapa, no padrão
  `tipo(OpBlazorUI.Base | OpBlazorUI.Showcase): descrição`.
- Cenários de teste que precisam de montagem específica ficam em páginas `/_tests/...` do
  Showcase (fora do menu) e são cobertos por testes Playwright em `tests/OpBlazorUI.Playwright`.
- A biblioteca precisa funcionar em Interactive Server, WebAssembly e Auto, e renderizar em SSR
  estático. O Showcase roda só em WebAssembly; o modo Server precisa de verificação à parte.

## Problemas que se repetem (referência)

Corrigir na base resolve vários componentes de uma vez. A coluna "Bloco" indica onde cada um é
tratado.

| # | Problema | Bloco |
|---|---|---|
| A | Descarte: com `IAsyncDisposable` o Blazor não chama `Dispose()` (OpInputBase, Accordion) | 1 ✅ |
| B | Clique fora dependia de `focusout` | 1 ✅ |
| C | Escape propagava para o overlay de baixo | 1 ✅ |
| D | Teclado ausente ou quebrado (menus, abas, árvores, Rating) | 4 |
| E | Cultura pt-BR em CSS/ARIA (Slider) | 1 ✅ |
| E2 | Cultura na leitura de números e datas (InputNumber, DatePicker, FilterService) | 3 |
| F | `Leaf=false` por padrão em Tree, TreeTable e TreeSelect | 3 |
| G | `OptionValue` não funciona em seis componentes de seleção | 3 |
| H | SSR estático: nenhum input usa `NameAttributeValue` | 3 |
| I | Seleção múltipla fora do `EditContext` | 3 |
| J | Interop JS sem tratamento de falhas | 1 ✅ |
| K | Listeners JS nunca removidos (ColorPicker, diretivas, Slider) | 3 |
| L | Itens desabilitados com `Url` navegam; `Command`/`Target` ignorados | 3 |
