# Overlays e feedback

Overlays usam serviços registrados por `AddOpBlazorUI()` e um componente host montado uma
única vez no layout. O mesmo `Key` liga a chamada do serviço ao host.

| Recurso | Host | Serviço |
| --- | --- | --- |
| Dialog controlado | `<OpDialog @bind-Visible>` | — |
| Confirmação | `<OpConfirmDialog Key="...">` | `OpConfirmationService` |
| Toast | `<OpToast Key="...">` | `OpMessageService` |
| Dialog dinâmico | `<OpDynamicDialogHost />` | `OpDialogService` |
| Drawer/painel lateral | `<OpDrawer @bind-Visible>` | — |

Monte os hosts no `MainLayout`:

```razor
<OpToast />
<OpConfirmDialog Key="app" />
<OpDynamicDialogHost />
```

## Dialog

```razor
<OpButton Label="Editar perfil" OnClick="() => _visible = true" />

<OpDialog @bind-Visible="_visible" Header="Editar perfil" Modal="true" Style="width:25rem">
    <div class="flex items-center gap-4 mb-4">
        <label for="dlg-name" class="font-semibold w-24">Nome</label>
        <OpInputText Id="dlg-name" @bind-Value="_name" StyleClass="flex-auto" />
    </div>
    <FooterTemplate>
        <OpButton Label="Cancelar" Text="true" Severity="secondary" OnClick="() => _visible = false" />
        <OpButton Label="Salvar" Outlined="true" OnClick="Save" />
    </FooterTemplate>
</OpDialog>

@code {
    private bool _visible;
    private string? _name;

    private void Save()
    {
        // persistir
        _visible = false;
    }
}
```

- `@bind-Visible` abre/fecha; `VisibleChanged` dispara junto.
- `Position`: `center` (padrão), `left`, `right`, `top`, `bottom`, `topleft`, `topright`,
  `bottomleft`, `bottomright`.
- `HeaderTemplate`/`FooterTemplate` customizam as áreas; `Style`/`ContentStyle` ajustam
  largura e altura; `Maximizable`, `Draggable` e `Resizable` habilitam interações.
- `OnShow`/`OnHide` observam o ciclo do overlay.

## Confirmação

```razor
@inject OpConfirmationService ConfirmationService

<OpButton Label="Excluir" Severity="danger" Outlined="true" OnClick="ConfirmDelete" />

@code {
    private void ConfirmDelete() =>
        ConfirmationService.Confirm(new ConfirmOptions
        {
            Key = "app",
            Header = "Zona de perigo",
            Message = "Deseja excluir este registro?",
            Icon = "pi pi-exclamation-triangle",
            AcceptLabel = "Excluir",
            RejectLabel = "Cancelar",
            AcceptButtonProps = new() { ["severity"] = "danger" },
            RejectButtonProps = new() { ["severity"] = "secondary", ["outlined"] = true },
            Accept = () => Delete(),
            Reject = () => { /* opcional */ }
        });

    private void Delete()
    {
        // excluir
    }
}
```

- `Accept`/`Reject` são `Action`. Para código assíncrono, use `Accept = () => _ = DeleteAsync()`.
- `Position` muda o alinhamento do diálogo; `MessageTemplate`/`HeaderTemplate`/`FooterTemplate`
  customizam o conteúdo.
- `OpConfirmPopup` é a variante ancorada a um elemento (com `Target`).

## Toast

```razor
@inject OpMessageService MessageService

<OpButton Label="Salvar" OnClick="Save" />

@code {
    private void Save()
    {
        MessageService.Add(new OpToastMessage
        {
            Severity = "success",
            Summary = "Sucesso",
            Detail = "Registro salvo."
        });
    }
}
```

- Severidades: `success`, `info`, `warn`, `error`, `secondary`, `contrast`.
- `Key` direciona a mensagem para um host específico; sem `Key`, usa o host sem chave.
- `Sticky = true` mantém a mensagem até `MessageService.Clear()`/`Remove()`; `Life` define o
  tempo em ms (padrão 3000).
- `AddAll([...])` enfileira várias mensagens de uma vez.

## Dialog dinâmico

Útil quando o conteúdo do diálogo é um componente próprio que devolve um resultado.

```razor
@inject OpDialogService DialogService

<OpButton Label="Selecionar produto" OnClick="SelectProduct" />

@code {
    private async Task SelectProduct()
    {
        var reference = DialogService.Open<ProductPicker>(new OpDynamicDialogConfig
        {
            Header = "Selecione um produto",
            Width = "34rem",
            Modal = true,
            InputValues = new Dictionary<string, object?> { ["Category"] = "Bebidas" }
        });

        var selected = await reference.Result; // o que o picker devolver
        if (selected is string name)
        {
            // usar o resultado
        }
    }
}
```

O componente aberto recebe os `InputValues` como parâmetros e fecha pelo ref:

```razor
@* ProductPicker.razor *@
@code {
    [CascadingParameter] private OpDynamicDialogRef? DialogRef { get; set; }

    [Parameter] public string? Category { get; set; }

    private void Choose(string name) => DialogRef?.Close(name);
    private void Cancel() => DialogRef?.Close(null);
}
```

`reference.Result` é uma `Task` que completa quando o diálogo fecha (`Close(valor)`), com o
valor informado.

## Drawer, Popover e Tooltip

```razor
<OpDrawer @bind-Visible="_open" Header="Filtros" Position="left" Style="width:20rem">
    <!-- conteúdo -->
</OpDrawer>

<span @ref="_trigger" class="inline-flex">
    <OpButton Label="Compartilhar" @onclick="TogglePopover" />
</span>
<OpPopover @ref="_popover">
    <div class="flex flex-col gap-4 w-64">
        <!-- conteúdo -->
    </div>
</OpPopover>

<OpTooltip Content="Salva o registro" Position="top">
    <OpButton Label="Salvar" />
</OpTooltip>

@code {
    private bool _open;
    private OpPopover? _popover;
    private ElementReference _trigger;

    private void TogglePopover() => _ = _popover!.Toggle(_trigger);
}
```

- `OpDrawer`: painel lateral (`Position` = `left`, `right`, `top` ou `bottom`; `FullScreen`
  cobre a tela).
- `OpPopover` é aberto por API: guarde um `@ref` e chame `Toggle(trigger)` ou `Hide()`.
- `OpTooltip` envolve o elemento alvo (`<OpTooltip Content="..."><OpButton /></OpTooltip>`);
  `Position` aceita `top`, `bottom`, `left` e `right`.

## Erros comuns

- Esquecer de montar o host (`OpToast`, `OpConfirmDialog`, `OpDynamicDialogHost`) no layout:
  a chamada do serviço não mostra nada.
- Usar `Key` diferente entre host e chamada: a mensagem é roteada para um host inexistente.
- Registrar os serviços manualmente em vez de chamar `AddOpBlazorUI()`.
- Tentar abrir overlays em SSR estático: eles exigem a parte interativa da aplicação.
