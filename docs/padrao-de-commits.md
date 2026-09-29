# Padrão de commits

```
tipo(Escopo): descrição curta
```

Com corpo opcional em um segundo `-m`:

```bash
git commit -m "fix(OpBlazorUI.Base | OpBlazorUI.Showcase): corrige x, y e z" -m "- detalhe do x
- detalhe do y
- detalhe do z"
```

## Tipo

| Tipo | Quando |
|---|---|
| `feat` | funcionalidade nova (componente, parâmetro, página da doc, recurso do Showcase) |
| `fix` | correção de bug ou de comportamento/visual |
| `chore` | manutenção sem efeito no comportamento (build, dependências, scripts, CI, organização) |

Também aparecem no histórico `docs` (documentação) e `refactor` (reestruturação sem mudar comportamento).

## Escopo

O **projeto (namespace) modificado**, entre parênteses:

- `OpBlazorUI.Base` — biblioteca de componentes (`src/Components/OpBlazorUI.Base`)
- `OpBlazorUI.Showcase` — site de documentação (`src/OpBlazorUI.Showcase`)

Quando o commit mexe em mais de um projeto, separe com ` | `:

```
fix(OpBlazorUI.Base | OpBlazorUI.Showcase): ...
```

Arquivos fora dos projetos (`tools/`, `docs/`, `.github/`) usam o nome da pasta como escopo, ex.: `chore(tools)`.

## Descrição e corpo

- Em pt-BR, no presente, começando com verbo em minúsculo (`corrige`, `adiciona`, `remove`), sem ponto final.
- Título curto e objetivo; o detalhamento vai no corpo (segundo `-m`), em lista quando houver mais de um item.

## Exemplos

```
feat(OpBlazorUI.Base): adiciona ícone plus ao OpIcon
fix(OpBlazorUI.Base | OpBlazorUI.Showcase): centraliza o ícone do SpeedDial e alinha as cores do Configurator
chore(tools): atualiza o gerador de temas para o Optimus UI 2.1
```
