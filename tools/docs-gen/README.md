# docs-gen

Gerador dos artefatos de documentação para agentes de IA do OpBlazorUI. Não usa Node:
lê a solução .NET, as páginas do Showcase e os arquivos estáticos do repositório.

## O que ele gera

| Arquivo | Conteúdo |
| --- | --- |
| `artifacts/docs/manifest.json` | Catálogo completo: componentes, tipos, parâmetros, eventos, templates, serviços, enums, ícones, seções e exemplos. |
| `artifacts/docs/llms.txt` | Índice em Markdown no padrão [llms.txt](https://llmstxt.org/), com guias, componentes, serviços e recursos. |
| `artifacts/docs/llms/<id>.md` | Um arquivo por componente, guia ou serviço: API refletida, exemplos, theming e links. |
| `artifacts/docs/llms-full.txt` | Todo o conteúdo em um único arquivo, para fixar uma versão ou dar contexto amplo ao agente. |
| `artifacts/docs/llms/icons.md` | Glyphs da fonte nativa (`pi pi-*`). |
| `artifacts/docs/llms/services.md` | Assinaturas de todos os serviços públicos. |
| `artifacts/docs/llms/enums.md` | Enums públicos da biblioteca. |
| `artifacts/docs/docs-audit.json` | Relatório de divergências entre as tabelas de API das páginas e o código. |

Os artefatos são **gerados no CI** e ficam fora do commit (`artifacts/` está no `.gitignore`).
As páginas do Showcase continuam sendo a fonte das descrições em pt-BR.

## Uso

```bash
# manifesto + llms.txt + Markdown por componente + llms-full.txt
dotnet run --project tools/docs-gen -- generate --out artifacts/docs

# auditoria (warn = só relata; fail = código 1 se houver drift hard)
dotnet run --project tools/docs-gen -- audit --mode warn
dotnet run --project tools/docs-gen -- audit --mode fail

# confere se os artefatos de IA estão completos e se os links locais existem
dotnet run --project tools/docs-gen -- verify --dir artifacts/docs

# ajuda
dotnet run --project tools/docs-gen -- help
```

Opções úteis:

- `--version <semver>` carimba a versão da lib no manifesto/relatório.
- `--commit <sha>` carimba o commit (padrão: `git rev-parse --short HEAD`).
- `--base-url <url>` muda a base das URLs de documentação.
- `--dir <diretório>` aponta o diretório conferido pelo `verify` (padrão: `artifacts/docs`).
- `--repo <caminho>` aponta a raiz do repositório quando o comando não roda na raiz.

## De onde vêm os dados

- **Reflexão sobre `OpBlazorUI.dll` e `OpBlazorUI.Charts.dll`**: assinaturas reais,
  parâmetros (`[Parameter]`, `[CascadingParameter]`, `[EditorRequired]`), eventos,
  templates, serviços e enums. Propriedades públicas sem `[Parameter]` (tipos de
  contexto, opções) vão para `properties`.
- **XML docs** (`OpBlazorUI.xml`): resumos de tipos e membros. Os `.csproj` da lib e
  dos gráficos têm `GenerateDocumentationFile=true`; o build do docs-gen copia os XMLs
  para o próprio `bin`.
- **Páginas do Showcase** (`src/OpBlazorUI.Showcase/Components/Pages/*.razor`):
  descrições, valores padrão, seções, exemplos (`private const string` + raw strings)
  e tabelas de API. O parser entende as tabelas `<ApiTable>` com `Rows="@(new[] { ... })"`
  e ignora chaves/aspas dentro das strings das células.
- **`AppMenu.cs`**: rótulo, rota, categoria e status de cada página.
- **`DocTheming.cs`**: classes CSS e tokens documentados.
- **`openng-icons.css`**: lista de glyphs `pi pi-*`.
- **`overrides.json`**: exceções e aliases (ver abaixo).

## Auditoria (`audit`)

Compara cada propriedade `[Parameter]` do código com as tabelas de API da página do
componente, **por tipo** (tabelas com o nome do tipo no label ou no `Caption`).

- **hard** — drift que deve ser corrigido:
  - `missing_in_docs`: propriedade própria do tipo sem linha na tabela.
  - `phantom_in_docs`: linha na tabela que não corresponde a nenhuma propriedade/método.
- **soft** — aviso, não bloqueia:
  - `missing_inherited`: parâmetro herdado (ex.: `Id`, `StyleClass`, `DisplayName`).
  - `type_mismatch`: tipo documentado diferente do tipo refletido.

O CI roda em `--mode fail`: qualquer hard novo quebra o job. Os soft continuam no
relatório para limpeza incremental.

### Allowlist (`overrides.json`)

```json
"audit": {
  "alwaysIgnore": [ "AdditionalAttributes" ],
  "ignore": {
    "AutoComplete": [ "MultipleValueExpression" ],
    "FileUpload": [ "Method", "Name", "Url" ],
    "Listbox": [ "SelectedValuesExpression" ],
    "RadioButton": [ "ModelValueExpression" ],
    "SelectButton": [ "MultipleValueExpression" ]
  }
}
```

Motivos das omissões atuais:

- `*Expression`: plumbing gerado por `@bind`; não é um parâmetro que o consumidor
  normalmente escreve. Continua no manifesto com `documented: false`.
- `FileUpload.Name/Url/Method`: declarados mas **sem uso** no componente (candidatos a
  remoção numa limpeza de API). Não documentar até a funcionalidade existir.
- `AdditionalAttributes`: captura de atributos livres; documentado globalmente.

Sempre que possível, corrija a página em vez de ampliar a allowlist.

## `overrides.json`

- `componentOverrides`: por rótulo do menu, permite trocar `id`, `type`, `page` e
  adicionar `aliases` em inglês/pt-BR para busca no manifesto.
- `kindOverrides`: classifica páginas que não são componentes (`guide`, `service`).
- `audit`: allowlist acima.

## Adicionar um componente novo

1. Crie a entrada no `AppMenu.cs` e a página em `Components/Pages`, seguindo o padrão
   de `Button.razor` (seções do upstream, acessibilidade, API, temas).
2. Documente a API com `<ApiTable>` dentro de `<Api>`; para tipos auxiliares, use uma
   seção com o nome do tipo (ex.: `OpAccordionPanel`) ou `Caption="NomeDoTipo"`.
3. Se o componente tiver theming, registre em `DocTheming.cs`.
4. Rode `generate` e `audit --mode fail`; corrija os hard e revise os soft.
5. O CI publica o manifesto e os arquivos de `llms/` como artefato (`docs-artifacts`).

## Publicação

- **GitHub Pages** (`deploy-pages.yml`): gera os artefatos e copia `llms.txt`,
  `llms-full.txt` e `llms/manifest.json` para a raiz do site, além de `llms/*.md`.
  A página `/llms-txt` do Showcase documenta o uso.
- **Release** (`release.yml`): gera com a versão da tag e anexa `manifest.json`,
  `llms.txt` e `llms-full.txt` à GitHub Release. Para fixar uma versão, o
  `llms-full.txt` é autossuficiente; os links do `llms.txt` apontam para a versão
  publicada no Pages.
- **CI**: o job `Docs em dia` roda `generate`, `verify` e `audit --mode fail`, e
  publica o diretório `artifacts/docs` como artefato.
