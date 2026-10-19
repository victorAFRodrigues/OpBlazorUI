# DataTable (CRUD)

`OpDataTable<TItem>` recebe `Items` e `Columns` e cobre listagem, ordenação, filtro,
paginação, seleção, expansão e edição. Sempre informe `DataKey` quando usar seleção,
expansão ou edição: é a propriedade que identifica a linha.

## Lista com ordenação, filtro e paginação

```razor
<OpDataTable Items="_products" Columns="_cols" DataKey="Id"
             Paginator="true" Rows="10" RowsPerPageOptions="@(new[] { 5, 10, 25 })"
             Sortable="true" RowHover="true" />

@code {
    private List<Product> _products = [];

    private static readonly List<OpDataTableColumn<Product>> _cols =
    [
        new() { Field = "Name", Header = "Nome", Sortable = true, Filter = true },
        new() { Field = "Category", Header = "Categoria", Filter = true },
        new() { Field = "Price", Header = "Preço" },
        new() { Field = "Stock", Header = "Estoque" }
    ];

    private sealed class Product
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public string Category { get; set; } = "";
        public decimal Price { get; set; }
        public int Stock { get; set; }
    }
}
```

- `Field` lê a propriedade por reflexão (case-insensitive) quando não há `BodyTemplate`.
- `Sortable` global combina com `OpDataTableColumn.Sortable`; `Filter` habilita o campo de
  busca no cabeçalho da coluna.
- `GlobalFilter` + `GlobalFilterFields` criam um filtro geral para a tabela.

## Templates de coluna e rodapé

```razor
<OpDataTable Items="_products" Columns="_cols" DataKey="Id" />

@code {
    private static RenderFragment PriceCell(Product product)
        => @<span>@product.Price.ToString("C")</span>;

    private static RenderFragment StockCell(Product product)
        => @<OpBadge Value="@product.Stock.ToString()"
                     Severity="@(product.Stock > 50 ? "success" : "danger")" />;

    private static RenderFragment ActionsCell(Product product)
        => @<OpButton Icon="pi pi-pencil" Rounded="true" Size="small" AriaLabel="Editar"
                      OnClick="() => Edit(product)" />;

    private static readonly List<OpDataTableColumn<Product>> _cols =
    [
        new() { Field = "Name", Header = "Nome", Footer = "Total" },
        new() { Field = "Price", Header = "Preço", BodyTemplate = PriceCell },
        new() { Field = "Stock", Header = "Estoque", BodyTemplate = StockCell },
        new() { Header = "Ações", BodyTemplate = ActionsCell, Sortable = false }
    ];
}
```

`BodyTemplate` é `RenderFragment<TItem>`: escreva um método que devolve a célula e atribua.

## Seleção

```razor
<OpDataTable Items="_products" Columns="_cols" DataKey="Id"
             Selection="true" SelectionMode="single"
             @bind-SelectedItems="_selected" />

<div>Selecionado: @(_selected.Count == 0 ? "nenhum" : _selected[0].Name)</div>

@code {
    private IReadOnlyList<Product> _selected = [];
}
```

- `SelectionMode`: `single` ou `multiple` (padrão `multiple`).
- Para checkbox/radio em uma coluna dedicada, use `SelectionMode` no `OpDataTableColumn`:

```csharp
new() { Header = "", SelectionMode = "single", Sortable = false, Width = "4rem" }
```

- `MetaKeySelection="true"` exige ⌘/Ctrl para somar/remover da seleção; `SelectionPageOnly`
  limita o "selecionar tudo" à página atual.
- `OnRowClick` recebe o item clicado; `SelectedItemsChanged` dispara quando a seleção muda.

## Carregamento lazy (servidor)

```razor
<OpDataTable Items="_page" Columns="_cols" DataKey="Id"
             Lazy="true" Paginator="true" TotalRecords="_total"
             OnLazyLoad="OnLazyLoad" />

@code {
    private int _total;
    private List<Product> _page = [];

    private async Task OnLazyLoad(OpDataTableLazyLoadEvent e)
    {
        // e.First, e.Rows, e.SortField, e.SortOrder e e.Filters
        var result = await Api.GetProductsAsync(e.First, e.Rows);
        _page = result.Items;
        _total = result.Total;
    }
}
```

No modo lazy a tabela não ordena/filtra/pagina no cliente: quem faz isso é a sua fonte de
dados, a partir do `OpDataTableLazyLoadEvent`.

## Edição de linha

```razor
<OpDataTable @ref="_table" Items="_products" Columns="_editCols" DataKey="Id" EditMode="row"
             OnRowEditInit="OnRowEditInit" OnRowEditSave="OnRowEditSave"
             OnRowEditCancel="OnRowEditCancel" />

@code {
    private OpDataTable<Product>? _table;
    private readonly Dictionary<int, Product> _clones = [];

    private void OnRowEditInit(Product item) => _clones[item.Id] = Clone(item);

    private void OnRowEditSave(Product item) => _clones.Remove(item.Id);

    private void OnRowEditCancel(Product item)
    {
        if (_clones.Remove(item.Id, out var clone))
        {
            item.Name = clone.Name;
            item.Price = clone.Price;
        }
    }

    private static RenderFragment ActionsCell(Product item) =>
        @<span class="flex gap-2">
            @if (_table!.IsRowInEditMode(item))
            {
                <OpButton Icon="pi pi-check" Rounded="true" Size="small" AriaLabel="Salvar"
                          OnClick="() => _table.SaveEditAsync(item)" />
                <OpButton Icon="pi pi-times" Rounded="true" Size="small" Severity="secondary"
                          AriaLabel="Cancelar" OnClick="() => _table.CancelEditAsync(item)" />
            }
            else
            {
                <OpButton Icon="pi pi-pencil" Rounded="true" Size="small" Severity="secondary"
                          AriaLabel="Editar" OnClick="() => _table.StartEditAsync(item)" />
            }
        </span>;
}
```

Métodos públicos úteis do `OpDataTable<TItem>`: `StartEditAsync`, `SaveEditAsync`,
`CancelEditAsync`, `IsRowInEditMode`, `SelectRowAsync`, `UnselectRowAsync`,
`ToggleRowExpansionAsync`, `FilterGlobalAsync`.

## Excluir com confirmação

```razor
@inject OpConfirmationService ConfirmationService

<OpButton Icon="pi pi-trash" Rounded="true" Size="small" Severity="danger"
          AriaLabel="Excluir" OnClick="() => ConfirmDelete(product)" />

@code {
    private void ConfirmDelete(Product product) =>
        ConfirmationService.Confirm(new ConfirmOptions
        {
            Header = "Excluir produto",
            Message = $"Deseja excluir {product.Name}?",
            Icon = "pi pi-exclamation-triangle",
            AcceptLabel = "Excluir",
            AcceptButtonProps = new() { ["severity"] = "danger" },
            RejectButtonProps = new() { ["severity"] = "secondary", ["outlined"] = true },
            Accept = () => _ = DeleteAsync(product)
        });

    private async Task DeleteAsync(Product product)
    {
        await Api.DeleteAsync(product.Id);
        _products.Remove(product);
    }
}
```

`Accept`/`Reject` são `Action`: para chamar código assíncrono, descarte a `Task` com `_ =`
ou exponha um método `void` que inicia a operação. Detalhes em
[overlays-e-feedback.md](overlays-e-feedback.md).

## Dicas

- `ScrollHeight="400px"` (ou `flex`) + `StickyHeader="true"` para rolagem vertical;
  `Frozen`/`FrozenItems` congelam colunas e linhas.
- `Loading`/`LoadingMode` (`mask` ou `skeleton`) mostram o estado de carregamento.
- `StateKey` + `StateStorage` persistem ordenação, filtros e página.
- A tabela não pagina/filtra no servidor sem `Lazy`; sem ele, todos os `Items` ficam no cliente.
