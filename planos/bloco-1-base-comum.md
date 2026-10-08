# Bloco 1 — Base comum

**Status:** implementado na branch `fix/bloco1-base` (enviada ao GitHub). Compila sem erros;
ainda não foi testado em execução.

## 1.1 Descarte ✅

- [x] `Forms/OpInputBase.cs`: `DisposeAsync` chama `((IDisposable)this).Dispose()`. Sem isso o
  `InputBase` nunca desinscrevia o `OnValidationStateChanged` do `EditContext`.
- [x] `Common/OpComponentBase.cs` e `OpInputBase.cs`: o `Interop` não é recriado depois do descarte.
- [x] `Accordion/OpAccordionPanel.razor.cs`: sai da lista do pai num override de `DisposeAsync`
  (o `IDisposable.Dispose()` nunca rodava). `OpAccordion.Unregister` renumera os painéis.

## 1.2 Interop JS tolerante a falhas ✅

- [x] `OpInterop.cs`: import com falha não fica em cache; queda do circuito, cancelamento e
  chamadas após o descarte viram no-op; `JSException` vai para o log (`ILogger`); módulos ainda
  em import no descarte também são descartados.
- [x] `OpBlazorUiSetup.razor`: não derruba o circuito se `optimus.interop.js` não carregar.
- [x] `Editor/OpEditor`: mostra um erro quando o Quill não carrega; o init é abortado se o
  componente for descartado enquanto aguarda o Quill (`editor.interop.js`).

## 1.3 Overlays: clique fora e Escape ✅

- [x] `wwwroot/overlay.interop.js`: um par de listeners (window, captura) para todos os overlays,
  na ordem de abertura. Escape vai só para o overlay do topo e não propaga; clique fora fecha
  quando o alvo não está no painel, na âncora nem num overlay aberto acima.
- [x] `Overlay/OpOverlayAttach.razor`: novos parâmetros `OnOutsideClick`, `OnEscape`,
  `Dismissable` e `AutoZIndex`.
- [x] Saem do fechamento por `focusout`: Select, MultiSelect, AutoComplete, CascadeSelect,
  TreeSelect, DatePicker, Menu (popup) e SplitButton. Tab fecha pelo teclado (no MultiSelect e no
  DatePicker, só a partir do gatilho, para navegar dentro do painel).
- [x] Popover e ConfirmPopup trocam o `addOutsideClickListener` próprio pelo mecanismo novo.
- [x] Dialog, Drawer e ConfirmDialog recebem o Escape pela pilha (respeitando `CloseOnEscape`).
- [x] Menubar, TieredMenu, MegaMenu e ContextMenu fecham os submenus com clique fora e Escape.

## 1.4 Cultura em CSS e ARIA ✅

- [x] `OpCss.Num(double)`: ponto decimal, até 4 casas.
- [x] Slider, SliderRange e MeterGroup usam o helper nos estilos e em `aria-value*`.

## 1.5 Verificação

- [x] Página `/_tests/bloco1` e `tests/OpBlazorUI.Playwright/Bloco1Tests.cs` (5 cenários).
- [x] `global.json` com `rollForward: latestFeature` (antes nenhum `dotnet` funcionava no repo
  sem o SDK 10.0.301 exato).
- [ ] Rodar `dotnet test tests/OpBlazorUI.Playwright --filter Bloco1Tests`. Na primeira
  tentativa o fixture não subiu o Showcase dentro de 180 s; a causa não foi investigada.
- [ ] Verificação manual no Showcase (WASM) e numa app Interactive Server.
- [ ] Adicionar `dotnet test` ao `ci.yml` (aprovado para este bloco; adiado até os testes
  passarem localmente pelo menos uma vez).

## Pontos para conferir ao testar

- Cliques dentro do painel (opção, filtro, rolagem, "selecionar todos" do MultiSelect) não podem
  fechar o painel.
- Tab fecha Select, CascadeSelect e TreeSelect; no MultiSelect e no DatePicker só ao sair do campo.
- Escape em Dialogs aninhados fecha um por vez; num Select dentro de um Dialog, só o Select.
- Submenus de Menubar, TieredMenu e MegaMenu fecham com clique fora e Escape.

## Decisões tomadas

1. Perda de foco deixou de fechar painéis; Tab fecha pelo tratamento de teclado (como o PrimeNG).
2. `JSException` é registrada no log e ignorada, exceto no Editor, que mostra o erro.
3. Cenários de teste ficam em rotas `/_tests/...` do Showcase.
4. `dotnet test` entra no CI (pendente, ver 1.5).

## Ficou para outros blocos

- Ordem dos painéis do Accordion e das abas segue o registro, não a marcação (bloco 3).
- Reancorar ContextMenu, ConfirmPopup e Popover quando já abertos com outro alvo (bloco 3).
- Restaurar o foco do Dialog/Drawer quando o pai fecha pelo binding (bloco 3).
- Os `try/catch` repetidos em volta de chamadas ao `Interop` nos componentes ainda existem; são
  redundantes agora e podem ser removidos quando cada arquivo for tocado.
