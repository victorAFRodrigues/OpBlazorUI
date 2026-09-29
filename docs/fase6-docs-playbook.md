# Fase 6 — Playbook de documentação por componente

> Objetivo: cada página do Showcase deve cobrir **no mínimo** as seções da doc de referência
> ([v21.primeng.org](https://v21.primeng.org), base do Optimus UI), adaptadas ao Blazor, e acrescentar o que
> é específico do Blazor (binding, eventos, formulários, render modes).
>
> Página-modelo: `src/OpBlazorUI.Showcase/Components/Pages/Button.razor` (antes: 7 seções / sem textos;
> depois: 21 seções, todas com explicação, demo e código, API dividida em Propriedades/Templates/Eventos).

---

## 1. Fluxo por página (≈ 1–3 h cada)

1. **Baixar a referência**
   ```bash
   bash tools/doc-upstream/fetch.sh <componente>        # ex.: inputnumber, toggleswitch, datepicker
   ```
   Imprime a ordem oficial das seções (`id` + `label`) e o template de cada `*doc.ts`
   (texto explicativo + markup da demo). Arquivos ficam em `tools/doc-upstream/out/<componente>/` (ignorado no git).
   Tag padrão `21.0.0`; passe `master` como 2º argumento para a versão mais nova.
   > O nome é o da pasta no PrimeNG, que às vezes difere do nosso (ToggleSwitch = `toggleswitch`,
   > Table = `table`, Select = `select`).

2. **Inventariar o componente Blazor** — ler `OpX.razor.cs` e listar *todos* os `[Parameter]`,
   `EventCallback` e `RenderFragment`. É daqui que sai a aba API (não do upstream).

3. **Montar a matriz de cobertura** (rascunho mental ou no PR):

   | Seção upstream | Parâmetro Blazor existe? | Ação |
   |---|---|---|
   | existe e funciona | sim | portar demo + texto |
   | existe, mas é conceito Angular (diretiva, `ngModel`, `formControl`, `pt`) | — | **adaptar** ao equivalente Blazor e explicar a diferença |
   | existe, mas não temos o recurso | não | registrar em **Problemas Conhecidos** (e, se valer, no `roadmap.md`) |
   | não existe no upstream, mas é útil em Blazor | — | seção extra (ver §3) |

4. **Escrever a página** seguindo o esqueleto do §2 e as convenções do §4.

5. **Verificar**
   - `dotnet build` do Showcase (rodar fora do repo por causa do `global.json`).
   - Subir na porta 5199 e tirar prints com `%TEMP%/opshot/shot.mjs` (tema claro e escuro).
   - Clicar nas demos interativas (loading, eventos, bind) e checar console sem erros.
   - Se usou classe Tailwind nova, confirmar que existe em `wwwroot/css/tailwind.css`
     (ou rodar `npm run build:css` no Showcase).

6. **Checklist final** (§5) e commit `docs(showcase): <Componente> — paridade com a doc de referência`.

---

## 2. Esqueleto da página

```razor
@page "/x"
@namespace OpBlazorUI.Showcase.Components.Pages
@using OpBlazorUI.Showcase.Components.Doc
@using OpBlazorUI.Base.Components.X

<PageTitle>X - OpBlazorUI</PageTitle>

<DocComponent Title="X" Description="(frase do upstream traduzida)">
    <Features>
        <DocSection Label="Importação" Id="import">
            <Code><CodeBlock Code="@ImportCode"/></Code>
        </DocSection>

        <DocSection Label="Básico" Id="basic">
            <Description>1–3 frases: o que é, qual parâmetro controla, quando usar.</Description>
            <ChildContent>…demo…</ChildContent>
            <Code><CodeBlock Code="@BasicCode"/></Code>
        </DocSection>

        … uma DocSection por seção do upstream, na mesma ordem …
        … seções extras do Blazor (§3) antes de Acessibilidade …

        <DocSection Label="Acessibilidade" Id="accessibility">
            <Description><h3>Leitor de tela</h3><p>…</p></Description>
            <Code>
                <CodeBlock Code="@AccessibilityCode"/>          @* opcional *@
                <div class="doc-section-description"><h3>Suporte a teclado</h3></div>
                <ApiTable Headers="@(new[] { "Tecla", "Função" })" Rows="…"/>
            </Code>
        </DocSection>
    </Features>

    <Api>
        <DocSection Label="Propriedades" Id="api-props">…ApiTable Nome/Tipo/Padrão/Descrição…</DocSection>
        <DocSection Label="Templates" Id="api-templates">…RenderFragments…</DocSection>
        <DocSection Label="Eventos" Id="api-events">…EventCallbacks…</DocSection>
    </Api>

    <Theming>
        <DocSection Label="Classes CSS" Id="style">…styledoc do upstream + classes que o componente aplica…</DocSection>
        <DocSection Label="Temas" Id="theming"><ChildContent><DocTokens Component="x"/></ChildContent></DocSection>
    </Theming>

    <Passthrough>…texto padrão (pt não portado)…</Passthrough>
    <KnownIssues>…lacunas reais, em lista…</KnownIssues>
</DocComponent>

@code {
    // estado das demos interativas
    // dados repetidos das demos (arrays de severidade etc.) → um @foreach na demo
    private const string BasicCode = """
        <OpX … />
        """;
}
```

---

## 3. Seções extras típicas do Blazor

Incluir só as que se aplicam ao componente:

| Seção | Quando | Conteúdo |
|---|---|---|
| **Binding** (`@bind-Value`, `ValueChanged`, `@bind-Value:after`) | todo input/seleção | demo mostrando o valor ao vivo |
| **Formulários** (`EditForm`, `DataAnnotationsValidator`, `Invalid`) | todo input | substitui "Reactive Forms"/"Template Driven" do upstream |
| **Eventos** | componentes com `EventCallback` | handler síncrono e `async Task`, com contador/log |
| **Largura total** (`Fluid`) | se o parâmetro existir e o upstream não demonstrar | |
| **Diretiva → markup/classes** | quando o upstream tem `pX` (diretiva) | mostrar as classes CSS equivalentes |
| **Render modes** | componentes com JS/overlay | nota sobre SSR estático/Server/WASM, se houver comportamento diferente |

---

## 4. Convenções

**Texto**
- pt-BR, tom de referência técnica: *o que faz → qual parâmetro → quando usar / cuidado*.
- Nome de parâmetro sempre em `<code>` e na grafia C# (`IconPos`, não `iconPos`).
- Diferenças em relação ao Angular são ditas explicitamente ("Não existe diretiva no Blazor como…").
- Não traduzir literalmente frases que não fazem sentido em Blazor.

**Demo e código**
- O código exibido deve ser **exatamente** o que a demo renderiza (quando a demo usa `@foreach` sobre um array,
  o snippet mostra os componentes expandidos — é o que o usuário copia).
- Demos interativas trazem o `@code` relevante no snippet (ex.: Loading).
- Código em `private const string XCode = """ … """;` no fim do arquivo, nunca inline em `Code="@(\"…\")"`.
- `Language` correto no `CodeBlock` (`razor` é o padrão; também `csharp`, `html`, `xml`, `css`, `bash`) — o
  realce de sintaxe (`Components/Doc/SyntaxHighlighter.cs`, paleta Rider Dark) depende dele.
- Labels das demos em pt-BR; valores de parâmetros (`"secondary"`, `"small"`) em inglês, como na API.
- Botões/elementos só com ícone sempre com `AriaLabel`.
- Layout da demo com as utilitárias Tailwind do upstream (`flex flex-wrap gap-4 justify-center`).

**Ids**
- `Id` explícito em toda `DocSection`, igual ao `id` do upstream (`icononly`, `raisedtext`) — evita slug com acento e
  mantém os links compatíveis. Seções extras usam id em inglês (`fluid`, `events`, `forms`).

**API**
- Fonte é o `.razor.cs`, não o upstream. Todo `[Parameter]` público aparece — inclusive `AdditionalAttributes`.
- Dividir em Propriedades / Templates / Eventos; na coluna Tipo, escapar genéricos (`EventCallback&lt;T&gt;`).
- Valores permitidos de strings enumeradas listados na descrição.

**Problemas Conhecidos**
- Só lacunas reais e verificáveis (recurso do upstream sem equivalente, bug conhecido). Evitar "Nenhum problema".

---

## 5. Checklist por página

- [ ] Todas as seções do `index.ts` upstream presentes, na mesma ordem (ou adaptadas/justificadas)
- [ ] Toda seção tem `Description` (exceto Importação)
- [ ] Snippet idêntico à demo; demos interativas incluem o `@code`
- [ ] Seções extras do Blazor aplicáveis (§3)
- [ ] Acessibilidade: leitor de tela + tabela de teclado
- [ ] API: todos os `[Parameter]`/eventos/templates do `.razor.cs`
- [ ] Temas: Classes CSS + `DocTokens`
- [ ] Problemas Conhecidos preenchido com lacunas reais
- [ ] Build ok, prints claro/escuro, console sem erros, interações testadas

---

## 6. Ordem sugerida

Critério: páginas mais usadas e mais pobres primeiro; componentes da mesma família juntos (o texto se reaproveita).

| Lote | Páginas | Observação |
|---|---|---|
| 1 — Form básico | InputText, Checkbox, RadioButton, ToggleSwitch, InputNumber, Password | define o padrão das seções Binding/Formulários |
| 2 — Botões | ToggleButton, SelectButton, SplitButton, SpeedDial | reaproveita textos do Button |
| 3 — Seleção | Select, MultiSelect, AutoComplete, Listbox, CascadeSelect, TreeSelect, DatePicker | seções grandes (filtro, grupo, template, virtual scroll) |
| 4 — Form auxiliares | FloatLabel, IftaLabel, IconField, InputGroup, InputMask, InputOtp, KeyFilter, Rating, Editor | |
| 5 — Overlay/feedback | Dialog, Drawer, Popover, Tooltip, Toast, Message, BlockUI, Overlay | nota de render modes |
| 6 — Dados/misc | Table, Paginator, VirtualScroller, Menu, Badge, Tag, Chip, Avatar, Card, Divider, Skeleton, ProgressBar, ProgressSpinner, ScrollTop, FocusTrap | Table é a maior; fazer isolada |

## 7. Melhorias de infraestrutura (opcionais, fazer antes do lote 3)

- **Navegação "Nesta página"** no `DocComponent` (o `DocSectionNav` já existe e só é usado no `GuidePage`);
  a doc de referência tem esse índice lateral e com 20+ seções ele faz falta.
- **`ApiTable` gerada por reflexão** sobre os `[Parameter]` do componente (nome/tipo/padrão automáticos, descrição
  por dicionário) — elimina a classe mais comum de erro (parâmetro esquecido/tipo errado).
- **`DocSection` com slot de texto após o código** (hoje a Acessibilidade usa o slot `Code` para a tabela de teclado).
