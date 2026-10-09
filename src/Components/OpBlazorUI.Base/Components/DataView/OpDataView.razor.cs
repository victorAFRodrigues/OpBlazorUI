using Microsoft.AspNetCore.Components;
using OpBlazorUI.Base.Components.Common;
using OpBlazorUI.Base.Components.Paginator;

namespace OpBlazorUI.Base.Components.DataView;

public partial class OpDataView<TItem> : OpComponentBase
{
    [Parameter] public IReadOnlyList<TItem> Items { get; set; } = Array.Empty<TItem>();

    /// <summary>Layout: <c>list</c> ou <c>grid</c>.</summary>
    [Parameter] public string Layout { get; set; } = "list";

    [Parameter] public bool Paginator { get; set; }
    [Parameter] public int Rows { get; set; } = 10;
    [Parameter] public int First { get; set; }
    [Parameter] public EventCallback<int> FirstChanged { get; set; }
    [Parameter] public EventCallback<int> RowsChanged { get; set; }
    [Parameter] public int TotalRecords { get; set; }
    [Parameter] public bool Lazy { get; set; }
    [Parameter] public int PageLinks { get; set; } = 5;
    [Parameter] public bool AlwaysShowPaginator { get; set; } = true;
    [Parameter] public IReadOnlyList<int>? RowsPerPageOptions { get; set; }
    [Parameter] public string PaginatorPosition { get; set; } = "bottom";
    [Parameter] public string? PaginatorStyleClass { get; set; }
    [Parameter] public RenderFragment? TemplateLeft { get; set; }
    [Parameter] public RenderFragment? TemplateRight { get; set; }

    [Parameter] public bool Loading { get; set; }
    [Parameter] public string? EmptyMessage { get; set; } = "No records found.";

    [Parameter] public RenderFragment? HeaderTemplate { get; set; }
    [Parameter] public RenderFragment? FooterTemplate { get; set; }
    [Parameter] public RenderFragment? EmptyTemplate { get; set; }
    [Parameter] public RenderFragment? LoadingTemplate { get; set; }
    [Parameter] public RenderFragment<OpDataViewListContext<TItem>>? ListTemplate { get; set; }
    [Parameter] public RenderFragment<OpDataViewGridContext<TItem>>? GridTemplate { get; set; }

    [Parameter] public EventCallback<OpPaginatorState> OnPageChange { get; set; }

    private string RootClass => Class("p-dataview p-component", StyleClass);

    private int EffectiveTotal => TotalRecords > 0 ? TotalRecords : Items.Count;

    private IReadOnlyList<TItem> PageItems
    {
        get
        {
            if (!Paginator || Lazy)
            {
                return Items;
            }

            return Items.Skip(First).Take(Rows).ToList();
        }
    }

    private async Task OnPageChangeHandler(OpPaginatorState state)
    {
        var rowsChanged = state.Rows != Rows;
        First = state.First;
        Rows = state.Rows;
        await FirstChanged.InvokeAsync(First);
        if (rowsChanged)
        {
            await RowsChanged.InvokeAsync(Rows);
        }

        await OnPageChange.InvokeAsync(state);
    }
}

public sealed record OpDataViewListContext<TItem>(IReadOnlyList<TItem> Items);

public sealed record OpDataViewGridContext<TItem>(IReadOnlyList<TItem> Items);
