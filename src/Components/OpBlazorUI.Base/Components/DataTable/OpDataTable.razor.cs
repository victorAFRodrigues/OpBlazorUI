using System.Reflection;
using Microsoft.AspNetCore.Components;
using OpBlazorUI.Base.Models;

namespace OpBlazorUI.Base.Components.DataTable;

public partial class OpDataTable<TItem> : ComponentBase
{
    private string _sortField = "";
    private int _sortOrder;
    private int _currentPage;

    private List<TItem>? _sortedCache;
    private List<TItem>? _pageCache;
    private bool _computed;
    private readonly Dictionary<string, PropertyInfo?> _propertyCache = new(StringComparer.OrdinalIgnoreCase);

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
    [Parameter] public string LoadingMode { get; set; } = "mask";
    [Parameter] public int SkeletonRows { get; set; }
    [Parameter] public string? StyleClass { get; set; }

    [Parameter] public RenderFragment? LoadingIconTemplate { get; set; }

    [Parameter] public EventCallback<(string Field, int Order)> OnSort { get; set; }
    [Parameter] public EventCallback<int> OnPage { get; set; }
    [Parameter] public EventCallback<TItem> OnRowClick { get; set; }

    [Parameter(CaptureUnmatchedValues = true)]
    public Dictionary<string, object>? AdditionalAttributes { get; set; }

    private IReadOnlyList<TItem> SourceItems => Items ?? Array.Empty<TItem>();

    private bool ShowSkeleton => Loading && string.Equals(LoadingMode, "skeleton", StringComparison.OrdinalIgnoreCase);

    private int SkeletonRowCount => SkeletonRows > 0 ? SkeletonRows : Rows;

    private List<TItem> SortedItems
    {
        get
        {
            EnsureComputed();
            return _sortedCache!;
        }
    }

    private int PageCount => Rows > 0
        ? Math.Max(1, (int)Math.Ceiling(SourceItems.Count / (double)Rows))
        : 1;

    private List<TItem> PageItems
    {
        get
        {
            EnsureComputed();
            return _pageCache!;
        }
    }

    // Os itens ordenados/paginados eram recalculados a cada acesso (várias vezes por render),
    // com reflection por comparação. Agora são computados uma vez por mudança de
    // Items/sort/página.
    private void EnsureComputed()
    {
        if (_computed)
        {
            return;
        }

        var list = SourceItems.ToList();

        if (_sortOrder != 0 && !string.IsNullOrEmpty(_sortField))
        {
            list.Sort((a, b) =>
            {
                var cmp = SafeCompare(GetFieldValue(a, _sortField), GetFieldValue(b, _sortField));
                return _sortOrder == 1 ? cmp : -cmp;
            });
        }

        _sortedCache = list;

        if (Paginator && Rows > 0)
        {
            var skip = Math.Max(0, _currentPage) * Rows;
            _pageCache = list.Skip(skip).Take(Rows).ToList();
        }
        else
        {
            // Rows <= 0: sem paginação (antes paginava com divisões inválidas).
            _pageCache = list;
        }

        _computed = true;
    }

    protected override void OnParametersSet()
    {
        _computed = false;

        // Ajusta a página quando Items/Rows mudam: evita "No results found" numa página que
        // deixou de existir (ex.: resultado filtrado).
        var pageCount = Rows > 0
            ? Math.Max(1, (int)Math.Ceiling(SourceItems.Count / (double)Rows))
            : 1;

        if (_currentPage >= pageCount) _currentPage = pageCount - 1;
        if (_currentPage < 0) _currentPage = 0;
    }

    private static int SafeCompare(object? a, object? b)
    {
        if (ReferenceEquals(a, b)) return 0;
        if (a is null) return -1;
        if (b is null) return 1;

        try
        {
            if (a is IComparable ca && a.GetType() == b.GetType())
            {
                return ca.CompareTo(b);
            }
        }
        catch
        {
            // comparação inválida: cai no ToString
        }

        return string.Compare(a.ToString(), b.ToString(), StringComparison.OrdinalIgnoreCase);
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

    [Parameter] public int PageLinkSize { get; set; } = 5;

    // Janela de páginas como o p-paginator do upstream (pageLinkSize).
    private IEnumerable<int> PageLinks
    {
        get
        {
            if (PageCount <= 0) return [];
            var start = Math.Max(0, _currentPage - PageLinkSize / 2);
            var end = Math.Min(PageCount, start + PageLinkSize);
            start = Math.Max(0, end - PageLinkSize);
            return Enumerable.Range(start, end - start);
        }
    }

    private string SortIconName(OpDataTableColumn col)
    {
        if (_sortField != col.Field || _sortOrder == 0) return "sort-alt";
        return _sortOrder == 1 ? "sort-amount-up-alt" : "sort-amount-down";
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
        // Ciclo: sem ordenação → asc → desc → sem ordenação. Após o reset (_sortOrder == 0) o
        // próximo clique na mesma coluna precisa recomeçar em asc, e não permanecer em 0.
        if (_sortField != col.Field || _sortOrder == 0)
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
        _computed = false;
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
        _computed = false;
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

        if (!_propertyCache.TryGetValue(field, out var property))
        {
            property = typeof(TItem).GetProperty(field, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
            _propertyCache[field] = property;
        }

        return property?.GetValue(item);
    }

    private static string BuildClass(params string?[] classes)
        => string.Join(' ', classes.Where(c => !string.IsNullOrWhiteSpace(c)));
}