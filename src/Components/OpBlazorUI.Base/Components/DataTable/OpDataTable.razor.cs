using System.Reflection;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using OpBlazorUI.Base.Components.Paginator;
using OpBlazorUI.Base.Models;

namespace OpBlazorUI.Base.Components.DataTable;

public partial class OpDataTable<TItem> : ComponentBase
{
    private string _sortField = "";
    private int _sortOrder;

    private int _first;
    private int _rows = 5;
    private int _lastFirstParam = int.MinValue;
    private int _lastRowsParam = int.MinValue;

    private List<TItem>? _sortedCache;
    private List<TItem>? _pageCache;
    private bool _computed;
    private readonly Dictionary<string, PropertyInfo?> _propertyCache = new(StringComparer.OrdinalIgnoreCase);

    [Parameter] public IReadOnlyList<TItem>? Items { get; set; }
    [Parameter] public IReadOnlyList<OpDataTableColumn> Columns { get; set; } = Array.Empty<OpDataTableColumn>();

    /// <summary>Nome da propriedade que identifica unicamente a linha (usado na seleção).</summary>
    [Parameter] public string? DataKey { get; set; }

    [Parameter] public bool Sortable { get; set; } = true;
    [Parameter] public bool StripedRows { get; set; }
    [Parameter] public bool ShowGridlines { get; set; }
    [Parameter] public bool RowHover { get; set; } = true;
    [Parameter] public string? Size { get; set; }

    /// <summary>Habilita seleção de linhas. <see cref="SelectionMode"/> define se é única ou múltipla.</summary>
    [Parameter] public bool Selection { get; set; }

    /// <summary><c>multiple</c> (padrão, com checkboxes) ou <c>single</c> (uma linha por clique).</summary>
    [Parameter] public string SelectionMode { get; set; } = "multiple";

    /// <summary>Em modo múltiplo, limita o select-all às linhas da página atual.</summary>
    [Parameter] public bool SelectionPageOnly { get; set; }

    [Parameter] public IReadOnlyList<TItem>? SelectedItems { get; set; }
    [Parameter] public EventCallback<IReadOnlyList<TItem>?> SelectedItemsChanged { get; set; }

    [Parameter] public bool Paginator { get; set; }
    [Parameter] public string PaginatorPosition { get; set; } = "bottom";

    /// <summary>Deslocamento (base 0) do primeiro item da página. Use com <c>@bind-First</c>.</summary>
    [Parameter] public int First { get; set; }
    [Parameter] public EventCallback<int> FirstChanged { get; set; }

    /// <summary>Itens por página. Use com <c>@bind-Rows</c>.</summary>
    [Parameter] public int Rows { get; set; } = 5;
    [Parameter] public EventCallback<int> RowsChanged { get; set; }
    [Parameter] public IReadOnlyList<int>? RowsPerPageOptions { get; set; }
    [Parameter] public int PageLinkSize { get; set; } = 5;

    [Parameter] public string? EmptyMessage { get; set; } = "No results found";
    [Parameter] public bool Loading { get; set; }
    [Parameter] public string LoadingMode { get; set; } = "mask";
    [Parameter] public int SkeletonRows { get; set; }
    [Parameter] public string? StyleClass { get; set; }

    /// <summary>Altura máxima do contêiner de rolagem (ex.: <c>20rem</c>). Habilita cabeçalho fixo de fato.</summary>
    [Parameter] public string? ScrollHeight { get; set; }

    /// <summary>Mantém o cabeçalho fixo durante a rolagem vertical.</summary>
    [Parameter] public bool StickyHeader { get; set; }

    [Parameter] public RenderFragment? LoadingIconTemplate { get; set; }

    [Parameter] public EventCallback<(string Field, int Order)> OnSort { get; set; }
    [Parameter] public EventCallback<int> OnPage { get; set; }
    [Parameter] public EventCallback<TItem> OnRowClick { get; set; }

    [Parameter(CaptureUnmatchedValues = true)]
    public Dictionary<string, object>? AdditionalAttributes { get; set; }

    private IReadOnlyList<TItem> SourceItems => Items ?? Array.Empty<TItem>();

    private bool IsSingleSelection => Selection && string.Equals(SelectionMode, "single", StringComparison.OrdinalIgnoreCase);
    private bool ShowCheckboxColumn => Selection && !IsSingleSelection;

    private bool ShowSkeleton => Loading && string.Equals(LoadingMode, "skeleton", StringComparison.OrdinalIgnoreCase);

    private int SkeletonRowCount => SkeletonRows > 0 ? SkeletonRows : _rows;

    private int PageCount => Paginator && _rows > 0
        ? Math.Max(1, (int)Math.Ceiling(SourceItems.Count / (double)_rows))
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

        if (Paginator && _rows > 0)
        {
            var skip = Math.Max(0, _first);
            _pageCache = list.Skip(skip).Take(_rows).ToList();
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

        if (First != _lastFirstParam)
        {
            _lastFirstParam = First;
            _first = First;
        }

        if (Rows != _lastRowsParam)
        {
            _lastRowsParam = Rows;
            _rows = Rows;
        }

        if (_rows <= 0)
        {
            _rows = 1;
        }

        // Ajusta a página quando Items/Rows mudam: evita "No results found" numa página que
        // deixou de existir (ex.: resultado filtrado).
        var maxFirst = Math.Max(0, (PageCount - 1) * _rows);
        if (_first > maxFirst) _first = maxFirst;
        if (_first < 0) _first = 0;
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

    private bool AllPageSelected => ShowCheckboxColumn && PageItems.Count > 0 && PageItems.All(IsSelected);

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
        col.StyleClass,
        col.HeaderStyleClass);

    private string TrClass(TItem item) => BuildClass(
        Selection ? "p-datatable-selectable-row" : null,
        IsSelected(item) ? "p-datatable-row-selected" : null);

    private string TableContainerStyle => ScrollHeight is null
        ? "overflow: auto;"
        : $"overflow: auto; max-height: {ScrollHeight};";

    private string TheadStyle => StickyHeader ? "position: sticky; top: 0; z-index: 1;" : "position: sticky;";

    private bool HasFooter => Columns.Any(c => c.Footer is not null || (c is OpDataTableColumn<TItem> t && t.FooterTemplate is not null));

    private string SortIconName(OpDataTableColumn col)
    {
        if (_sortField != col.Field || _sortOrder == 0) return "sort-alt";
        return _sortOrder == 1 ? "sort-amount-up-alt" : "sort-amount-down";
    }

    private bool IsSelected(TItem item)
    {
        var sel = SelectedItems;
        if (sel is null) return false;

        if (!string.IsNullOrEmpty(DataKey))
        {
            var key = GetFieldValue(item, DataKey);
            foreach (var s in sel)
            {
                if (Equals(GetFieldValue(s, DataKey), key)) return true;
            }

            return false;
        }

        foreach (var s in sel)
        {
            if (EqualityComparer<TItem>.Default.Equals(s, item)) return true;
        }

        return false;
    }

    private string? SortAria(OpDataTableColumn col)
    {
        if (!Sortable || !col.Sortable) return null;
        if (_sortField == col.Field && _sortOrder != 0)
        {
            return _sortOrder == 1 ? "ascending" : "descending";
        }

        return "none";
    }

    private Task OnHeaderKeydown(KeyboardEventArgs e, OpDataTableColumn col)
        => e.Key is "Enter" or " " or "Spacebar" ? ToggleSort(col) : Task.CompletedTask;

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

        _first = 0;
        _computed = false;
        await OnSort.InvokeAsync((_sortField, _sortOrder));
    }

    private async Task ToggleRowSelection(TItem item)
    {
        if (IsSingleSelection)
        {
            var single = IsSelected(item) ? new List<TItem>() : new List<TItem> { item };
            await SelectedItemsChanged.InvokeAsync(single);
            return;
        }

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

    private async Task OnPaginatorChange(OpPaginatorState state)
    {
        _first = state.First;
        _computed = false;

        if (state.Rows != _rows)
        {
            _rows = state.Rows;
            if (RowsChanged.HasDelegate)
            {
                await RowsChanged.InvokeAsync(_rows);
            }
        }

        if (FirstChanged.HasDelegate)
        {
            await FirstChanged.InvokeAsync(_first);
        }

        await OnPage.InvokeAsync(state.Page);
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
