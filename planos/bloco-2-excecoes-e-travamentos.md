# Bloco 2 — Exceções e travamentos

**Status:** proposta, aguardando aprovação.

**Objetivo:** nenhuma ação comum do usuário pode lançar exceção não tratada (no Server isso
derruba o circuito) nem travar a tela. Branch sugerida: `fix/bloco2-excecoes`.

Caminhos relativos a `src/Components/OpBlazorUI.Base/`.

## 2.1 FileUpload (`Components/FileUpload/`)

- [ ] `OpFileUpload.razor.cs` (`OnFilesSelected`): `e.GetMultipleFiles(FileLimit ?? int.MaxValue)`
  lança `InvalidOperationException` quando o usuário escolhe mais arquivos que o limite. Chamar
  `GetMultipleFiles(int.MaxValue)` e aplicar `FileLimit` no loop, mostrando a mensagem de limite.
- [ ] Com `Multiple`, arquivos de seleções anteriores ficam ilegíveis: o `InputFile` do Blazor
  zera o mapa de arquivos a cada `change` (e há dois `InputFile`, botão e área de drop).
  Ver decisão 1.
- [ ] `OpFileUploadFile`: adicionar `OpenReadStream()` que usa `MaxFileSize` como
  `maxAllowedSize`. Hoje o padrão do Blazor (500 KB) faz o handler falhar com `IOException`.
- [ ] `IsAccepted`: `Accept="*/*"` rejeita tudo e `Accept="*"` lança
  `ArgumentOutOfRangeException` (`type[..-1]`). Tratar `*`, `*/*` e `tipo/*`.
- [ ] Modo single: `_files.Clear()` acontece antes da validação; uma escolha inválida apaga a
  válida anterior.
- [ ] `UploadAsync`: tratar exceções do `UploadHandler` (hoje sobem) e não marcar como "Enviado"
  sem handler.

## 2.2 InputOtp (`Components/InputOtp/`)

- [ ] Com `Template`, os `ElementReference` nunca são preenchidos e `_inputs[i].FocusAsync()`
  lança `InvalidOperationException` no primeiro caractere. Mover o foco por JS/índice ou só focar
  quando a referência existir.

## 2.3 SliderRange (`Components/Slider/OpSliderRange.razor.cs`)

- [ ] `Math.Clamp(x, lower, upper)` lança quando `lower > upper` (valor fora de `Min`/`Max`,
  ex.: `Min` muda para 10 com `Value=[0,5]`). Normalizar os valores antes do clamp.

## 2.4 DataTable e Paginator

- [ ] `DataTable/OpDataTable.razor.cs`: `SortedItems`/`PageItems` são recalculados a cada acesso
  (15–20 vezes por render) com reflection por comparação. Calcular uma vez por mudança de
  `Items`/sort/página e cachear os acessores de propriedade (`GetProperty` compilado).
- [ ] Ordenação com `Comparer<object?>.Default` lança para valores sem `IComparable` ou de tipos
  mistos. Usar um comparador seguro (fallback para `ToString`).
- [ ] Ajustar `_currentPage` em `OnParametersSet` quando `Items` ou `Rows` mudam (hoje a página 5
  de um resultado filtrado mostra "No results found").
- [ ] `Rows <= 0`: tabela vazia com paginador; tratar como "sem paginação".
- [ ] `Paginator/OpPaginator.razor.cs`: `Rows=0` (padrão) causa `DivideByZeroException` em
  `first / Rows`; o jump-to-page com `TotalRecords=0` emite `First` negativo.

## 2.5 Concorrência e timers

- [ ] `Toast/OpToast.razor.cs`: `Dispose` só chama `cts.Dispose()`; adicionar `Cancel()` para os
  timers de `AutoCloseAsync` não rodarem num componente descartado.
- [ ] `Services/OpMessageService.cs`: `List` sem lock; `Add` vindo de thread de fundo (Server)
  pode dar "Collection was modified" durante o render. Proteger a lista e fazer o Toast reagir
  via `InvokeAsync`.
- [ ] `ConfirmDialog/OpConfirmDialog.razor.cs` (`OnConfirmRequested`): altera estado fora de
  `InvokeAsync`.

## 2.6 Verificação

- [ ] Página `/_tests/bloco2` com FileUpload (limite, accept, múltiplas seleções), InputOtp com
  template, SliderRange com valor fora do intervalo e DataTable com 5 mil linhas.
- [ ] Testes Playwright para cada cenário acima, verificando que não há erro no console.

## Decisões em aberto

1. **FileUpload com várias seleções:** (a) ler o conteúdo de cada arquivo para memória ao
   selecionar (respeitando `MaxFileSize`), o que mantém o comportamento do PrimeNG mas custa
   memória; (b) trocar a lista inteira a cada seleção, documentando a diferença. Recomendo (a)
   com limite de tamanho total configurável.
2. **DataTable:** fazer só a correção de desempenho e de exceção aqui e deixar templates, filtro
   e lazy para o bloco 4? Recomendo que sim.
