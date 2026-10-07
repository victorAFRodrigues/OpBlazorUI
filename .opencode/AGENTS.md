# AGENTS.md — OpBlazorUI

Instruções para agentes de código (OpenCode e similares) neste repositório. Comunicação sempre em **pt-BR**.

## O projeto

- Port para **Blazor** do [Optimus UI](https://optimus.openng.org) (fork MIT do PrimeNG v21).
  Referência de comportamento e documentação: <https://v21.primeng.org>.
- Biblioteca ainda em construção e sem consumidores externos: é permitido quebrar APIs e remover arquivos.
- Estrutura:
  - `src/Components/OpBlazorUI.Base` — biblioteca de componentes (Razor Class Library, prefixo `Op*`).
  - `src/OpBlazorUI.Showcase` — site de documentação (Blazor WebAssembly, publicado no GitHub Pages).
  - `tools/theme-gen` — gerador dos temas (Aura, Lara, Nora) a partir de `@openng/optimus-ui-themes`.
  - `tools/doc-upstream/fetch.sh` — baixa as seções da doc do PrimeNG de um componente.

## Regras de trabalho

- **Trabalho grande** (várias fases, muitos arquivos, docs de um lote): apresente um plano em fases, com as
  decisões em aberto, e só execute depois de aprovado.
- **Paridade com o upstream**: siga o markup, as classes CSS e os `inlineStyles` do Optimus UI/PrimeNG.
  Desvios conscientes ficam comentados no código (ex.: `gap: 0` no SpeedDial circular).
- **Render modes**: a lib precisa funcionar em Interactive Server, WebAssembly e Auto, e renderizar em SSR
  estático. O estado é sempre *scoped*, nunca `static`; o tema é aplicado por JS (`op-theme.js`).
- **Temas**: `wwwroot/themes/{aura,lara,nora}.css` são **gerados** — não edite à mão. CSS extra de componente
  vai em `tools/theme-gen/component-extras.json`, e depois o tema é regenerado.
- **Ícones**: use os glyphs da fonte nativa (`<i class="pi pi-*">`); o wrapper SVG `OpIcon` foi removido.
  Não crie ícones SVG à mão — se faltar um glyph, prefira uma lib de referência (ex.: Tabler Icons).
- **Showcase sem Node**: o Tailwind é um bundle estático versionado (`wwwroot/css/tailwind.css`). Antes de usar
  uma classe Tailwind nova, confirme que ela existe no bundle; se não existir, use uma classe já presente ou
  estilo inline. Para regenerar, siga o cabeçalho de `wwwroot/css/tailwind.input.css`.
- **Documentação de componente**: mantenha o padrão da página-modelo
  `src/OpBlazorUI.Showcase/Components/Pages/Button.razor` (seções do upstream na ordem, acessibilidade com
  leitor de tela + tabela de teclado, API completa, temas e problemas conhecidos reais).

## Build e verificação

```bash
dotnet build OpBlazorUI.slnx
dotnet run --project src/OpBlazorUI.Showcase
```

- O `global.json` fixa a versão do SDK; se ela não estiver instalada, rode o `dotnet` fora da pasta do repositório.
- Mudança visual só está pronta depois de conferida no Showcase: modos claro e escuro, sem erros no console.

## Commits, branches e releases

- Padrão de commit:

  ```
  tipo(Escopo): descrição
  ```

  - `tipo`: `fix`, `feat` ou `chore` (o histórico também usa `docs` e `refactor`).
  - `Escopo`: o projeto modificado — `OpBlazorUI.Base`, `OpBlazorUI.Showcase`. Vários ficam separados por ` | `,
    ex.: `fix(OpBlazorUI.Base | OpBlazorUI.Showcase): ...`. Fora dos projetos, use o nome da pasta
    (`chore(tools)`, `chore(docs)`).
  - Descrição em pt-BR, verbo no presente e em minúsculo, sem ponto final; detalhes no corpo, em um segundo `-m`.
- Nunca commite direto no `main`: crie um branch (`fix/...`, `feat/...`, `chore/...`) e abra um PR.
- Faça commit e push só quando o usuário pedir.
- **Tags `v*` publicam**: o `release.yml` gera o pacote `OpBlazorUI.Base` no GitHub Packages e cria uma release.
  Crie tags só quando pedido, sempre a partir do `main`, depois do merge.
- Push no `main` republica o Showcase no GitHub Pages.
