using System.Reflection;
using Microsoft.AspNetCore.Components;
using OpBlazorUI.Base.Models;

namespace OpBlazorUI.Base.Components.DataTable;

public partial class OpDataTable<TItem> : ComponentBase
{
    private string _sortField = "";
    private int _sortOrder;
    private int _currentPage;

    [Parameter] public IReadOnlyList<TItem>? Items { get; set; }
    [Parameter] public IReadOnlyList<OpDataTableColumn> Columns { get; set; } = Array.Empty<OpDataTableColumn>();
    [Parameter] public bool Sortable { get; set; } = true;
    [Parameter] public bool StripedRows { get; set; }
    [Parameter] public bool ShowGridlines { get; set; }
    [Parameter] public bool RowHover { get; set; } = true;
    [Parameter] public string? Size { get; set; }
    [Parameter] public bool Selection { get; set; }
    [Parameter] public IReadOnlyList<TItem>? SelectedItems { get; set; }
    [Parameter] public EventCallback<IReadOnlyList<TItem>?> SelectedItemsChanged { get; set; }
    [Parameter] public bool Paginator { get; set; }
    [Parameter] public string PaginatorPosition { get; set; } = "bottom";
    [Parameter] public int Rows { get; set; } = 5;
    [Parameter] public string? EmptyMessage { get; set; } = "No results found";
    [Parameter] public bool Loading { get; set; }
    [Parameter] public string? StyleClass { get; set; }

    [Parameter] public RenderFragment? LoadingIconTemplate { get; set; }

    [Parameter] public EventCallback<(string Field, int Order)> OnSort { get; set; }
    [Parameter] public EventCallback<int> OnPage { get; set; }
    [Parameter] public EventCallback<TItem> OnRowClick { get; set; }

    [Parameter(CaptureUnmatchedValues = true)]
    public Dictionary<string, object>? AdditionalAttributes { get; set; }

    private IReadOnlyList<TItem> SourceItems => Items ?? Array.Empty<TItem>();

    private List<TItem> SortedItems
    {
        get
        {
            var list = SourceItems.ToList();
            if (_sortOrder == 0 || string.IsNullOrEmpty(_sortField)) return list;
            list.Sort((a, b) =>
            {
                var va = GetFieldValue(a, _sortField);
                var vb = GetFieldValue(b, _sortField);
                var cmp = Comparer<object?>.Default.Compare(va, vb);
                return _sortOrder == 1 ? cmp : -cmp;
            });
            return list;
        }
    }

    private int PageCount => Math.Max(1, (int)Math.Ceiling(SortedItems.Count / (double)Math.Max(1, Rows)));

    private List<TItem> PageItems
    {
        get
        {
            if (!Paginator) return SortedItems;
            var skip = _currentPage * Rows;
            return SortedItems.Skip(skip).Take(Rows).ToList();
        }
    }

    private List<TItem> Selected
    {
        get
        {
            var list = new List<TItem>();
            var sel = SelectedItems ?? Array.Empty<TItem>();
            foreach (var item in sel) list.Add(item);
            return list;
        }
    }

    private bool AllPageSelected => PageItems.Count > 0 && PageItems.All(IsSelected);

    private string RootClass => BuildClass(
        "p-datatable p-component",
        RowHover || Selection ? "p-datatable-hoverable" : null,
        StripedRows ? "p-datatable-striped" : null,
        ShowGridlines ? "p-datatable-gridlines" : null,
        Size == "small" ? "p-datatable-sm" : null,
        Size == "large" ? "p-datatable-lg" : null,
        StyleClass);

    private string ThClass(OpDataTableColumn col) => BuildClass(
        "p-datatable-header-cell",
        col.Sortable && Sortable ? "p-datatable-sortable-column" : null,
        _sortField == col.Field && _sortOrder != 0 ? "p-datatable-column-sorted" : null,
        col.StyleClass);

    private string TrClass(TItem item) => BuildClass(
        Selection ? "p-datatable-selectable-row" : null,
        IsSelected(item) ? "p-datatable-row-selected" : null);

    private string SortIconName(OpDataTableColumn col)
    {
        if (_sortField != col.Field || _sortOrder == 0) return "chevrons-up-down";
        return _sortOrder == 1 ? "chevron-up" : "chevron-down";
    }

    private bool IsSelected(TItem item)
    {
        var sel = SelectedItems;
        if (sel is null) return false;
        foreach (var s in sel)
        {
            if (EqualityComparer<TItem>.Default.Equals(s, item)) return true;
        }

        return false;
    }

    private async Task ToggleSort(OpDataTableColumn col)
    {
        if (!Sortable || !col.Sortable) return;
        if (_sortField != col.Field)
        {
            _sortField = col.Field;
            _sortOrder = 1;
        }
        else if (_sortOrder == 1)
        {
            _sortOrder = -1;
        }
        else
        {
            _sortOrder = 0;
        }

        _currentPage = 0;
        await OnSort.InvokeAsync((_sortField, _sortOrder));
    }

    private async Task ToggleRowSelection(TItem item)
    {
        var list = Selected;
        var idx = list.FindIndex(s => EqualityComparer<TItem>.Default.Equals(s, item));
        if (idx >= 0) list.RemoveAt(idx);
        else list.Add(item);
        await SelectedItemsChanged.InvokeAsync(list);
    }

    private async Task ToggleAllSelection()
    {
        var list = Selected;
        if (AllPageSelected)
        {
            foreach (var item in PageItems)
            {
                list.RemoveAll(s => EqualityComparer<TItem>.Default.Equals(s, item));
            }
        }
        else
        {
            foreach (var item in PageItems)
            {
                if (!IsSelected(item)) list.Add(item);
            }
        }

        await SelectedItemsChanged.InvokeAsync(list);
    }

    private async Task GoToPage(int page)
    {
        if (page < 0 || page >= PageCount || page == _currentPage) return;
        _currentPage = page;
        await OnPage.InvokeAsync(page);
    }

    private async Task OnRowClicked(TItem item)
    {
        if (Selection)
        {
            await ToggleRowSelection(item);
        }

        await OnRowClick.InvokeAsync(item);
    }

    private object? GetFieldValue(TItem item, string field)
    {
        if (item is null) return null;
        var property =
            typeof(TItem).GetProperty(field, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
        return property?.GetValue(item);
    }

    private static string BuildClass(params string?[] classes)
        => string.Join(' ', classes.Where(c => !string.IsNullOrWhiteSpace(c)));
}