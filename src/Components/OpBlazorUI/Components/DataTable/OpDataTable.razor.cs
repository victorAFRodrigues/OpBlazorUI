using System.Reflection;
using System.Text;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using OpBlazorUI.Components.Paginator;
using OpBlazorUI.Globalization;
using OpBlazorUI.Models;
using OpBlazorUI.Services;

namespace OpBlazorUI.Components.DataTable;

/// <summary>Estado entregue ao <see cref="OpDataTable{TItem}.OnLazyLoad"/> no modo lazy.</summary>
public sealed class OpDataTableLazyLoadEvent
{
    public int First { get; init; }
    public int Rows { get; init; }
    public string? SortField { get; init; }
    public int SortOrder { get; init; }
    public IReadOnlyList<OpDataTableSortMeta>? MultiSortMeta { get; init; }
    public IReadOnlyDictionary<string, object?>? Filters { get; init; }
    public string? GlobalFilter { get; init; }
}

/// <summary>Estado de ordenação de uma coluna (modo múltiplo).</summary>
public sealed class OpDataTableSortMeta
{
    public string Field { get; init; } = "";

    /// <summary>1 (ascendente) ou -1 (descendente).</summary>
    public int Order { get; init; }
}

/// <summary>Estado entregue ao <see cref="OpDataTable{TItem}.OnSort"/>.</summary>
public sealed class OpDataTableSortEvent
{
    public string Field { get; init; } = "";
    public int Order { get; init; }
    public IReadOnlyList<OpDataTableSortMeta> MultiSortMeta { get; init; } = Array.Empty<OpDataTableSortMeta>();
}

public partial class OpDataTable<TItem> : ComponentBase
{
    private readonly List<OpDataTableSortMeta> _sortMeta = new();
    private string? _lastSortFieldParam;
    private int _lastSortOrderParam;
    private IReadOnlyList<OpDataTableSortMeta>? _lastMultiSortMetaParam;
    private readonly string _selectionGroupName = $"op-dt-sel-{Guid.NewGuid():N}";

    private int _first;
    private int _rows = 5;
    private int _lastFirstParam = int.MinValue;
    private int _lastRowsParam = int.MinValue;

    private string? _globalFilter;
    private string? _lastGlobalFilterParam;
    private readonly Dictionary<string, object?> _columnFilters = new(StringComparer.OrdinalIgnoreCase);

    private List<TItem>? _sortedCache;
    private List<TItem>? _pageCache;
    private int _displayCount;
    private bool _computed;
    private readonly Dictionary<string, PropertyInfo?> _propertyCache = new(StringComparer.OrdinalIgnoreCase);

    [Inject] private OpFilterService FilterService { get; set; } = default!;
    [Inject] private OpTranslationService TranslationService { get; set; } = default!;

    private OpTranslation Tr => TranslationService.Translation;

    [Parameter] public IReadOnlyList<TItem>? Items { get; set; }
    [Parameter] public IReadOnlyList<OpDataTableColumn> Columns { get; set; } = Array.Empty<OpDataTableColumn>();

    /// <summary>Nome da propriedade que identifica unicamente a linha (usado na seleção).</summary>
    [Parameter] public string? DataKey { get; set; }

    [Parameter] public bool Sortable { get; set; } = true;

    /// <summary><c>single</c> (padrão) ou <c>multiple</c> (com ⌘/Ctrl para adicionar colunas).</summary>
    [Parameter] public string SortMode { get; set; } = "single";

    /// <summary>Habilita o terceiro estado do ciclo de ordenação (asc → desc → sem ordenação).</summary>
    [Parameter] public bool RemovableSort { get; set; }

    /// <summary>Campo da ordenação inicial (combinado com <see cref="SortOrder"/>).</summary>
    [Parameter] public string? SortField { get; set; }

    /// <summary>1 (asc) ou -1 (desc) para a ordenação inicial.</summary>
    [Parameter] public int SortOrder { get; set; }

    /// <summary>Ordenação inicial do modo múltiplo (a ordem da lista define a prioridade).</summary>
    [Parameter] public IReadOnlyList<OpDataTableSortMeta>? MultiSortMeta { get; set; }

    [Parameter] public bool StripedRows { get; set; }
    [Parameter] public bool ShowGridlines { get; set; }
    [Parameter] public bool RowHover { get; set; } = true;
    [Parameter] public string? Size { get; set; }

    /// <summary>Habilita seleção de linhas. <see cref="SelectionMode"/> define se é única ou múltipla.</summary>
    [Parameter] public bool Selection { get; set; }

    /// <summary><c>multiple</c> (padrão) ou <c>single</c> (uma linha por clique). A coluna de
    /// checkbox/radio é declarada em <see cref="OpDataTableColumn.SelectionMode"/>.</summary>
    [Parameter] public string SelectionMode { get; set; } = "multiple";

    /// <summary>Exige ⌘/Ctrl para somar linhas à seleção múltipla (clique simples seleciona só a linha).</summary>
    [Parameter] public bool MetaKeySelection { get; set; }

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

    /// <summary>Filtro global aplicado a <see cref="GlobalFilterFields"/> (ou a todas as colunas).</summary>
    [Parameter] public string? GlobalFilter { get; set; }
    [Parameter] public EventCallback<string?> GlobalFilterChanged { get; set; }

    /// <summary>Campos considerados pelo filtro global. Sem valor, usa o <c>Field</c> de todas as colunas.</summary>
    [Parameter] public IReadOnlyList<string>? GlobalFilterFields { get; set; }

    [Parameter] public EventCallback<IReadOnlyDictionary<string, object?>> OnFilter { get; set; }

    /// <summary>Modo lazy: a tabela não filtra/ordena/pagina; quem consome faz isso e devolve a página em <c>Items</c>.</summary>
    [Parameter] public bool Lazy { get; set; }

    /// <summary>Total de registros (no modo lazy) usado pelo paginador.</summary>
    [Parameter] public int TotalRecords { get; set; }

    [Parameter] public EventCallback<OpDataTableLazyLoadEvent> OnLazyLoad { get; set; }

    [Parameter] public string? EmptyMessage { get; set; }
    [Parameter] public bool Loading { get; set; }
    [Parameter] public string LoadingMode { get; set; } = "mask";
    [Parameter] public int SkeletonRows { get; set; }
    [Parameter] public string? StyleClass { get; set; }

    /// <summary>Altura máxima do contêiner de rolagem (ex.: <c>20rem</c>). Habilita cabeçalho fixo de fato.</summary>
    [Parameter] public string? ScrollHeight { get; set; }

    /// <summary>Mantém o cabeçalho fixo durante a rolagem vertical.</summary>
    [Parameter] public bool StickyHeader { get; set; }

    [Parameter] public RenderFragment? LoadingIconTemplate { get; set; }

    [Parameter] public EventCallback<OpDataTableSortEvent> OnSort { get; set; }
    [Parameter] public EventCallback<int> OnPage { get; set; }
    [Parameter] public EventCallback<TItem> OnRowClick { get; set; }

    /// <summary>Clique com o botão direito em uma linha (para um menu de contexto próprio).</summary>
    [Parameter] public EventCallback<(TItem Item, MouseEventArgs Args)> OnRowContextMenu { get; set; }

    [Parameter(CaptureUnmatchedValues = true)]
    public Dictionary<string, object>? AdditionalAttributes { get; set; }

    private IReadOnlyList<TItem> SourceItems => Items ?? Array.Empty<TItem>();

    private bool IsSingleSelection => Selection && string.Equals(SelectionMode, "single", StringComparison.OrdinalIgnoreCase);
    private bool IsMultipleSort => string.Equals(SortMode, "multiple", StringComparison.OrdinalIgnoreCase);
    private bool HasFilters => !Lazy && Columns.Any(c => c.Filter && !string.IsNullOrEmpty(c.Field));

    private static bool IsSelectionColumn(OpDataTableColumn col) => !string.IsNullOrEmpty(col.SelectionMode);
    private static bool IsSingleSelectionColumn(OpDataTableColumn col)
        => string.Equals(col.SelectionMode, "single", StringComparison.OrdinalIgnoreCase);
    private static bool IsMultipleSelectionColumn(OpDataTableColumn col)
        => string.Equals(col.SelectionMode, "multiple", StringComparison.OrdinalIgnoreCase);

    private int OrderOf(string field)
    {
        if (string.IsNullOrEmpty(field)) return 0;
        for (var i = 0; i < _sortMeta.Count; i++)
        {
            if (string.Equals(_sortMeta[i].Field, field, StringComparison.OrdinalIgnoreCase)) return _sortMeta[i].Order;
        }

        return 0;
    }

    private int SortOrderOf(string field)
    {
        if (string.IsNullOrEmpty(field)) return 0;
        for (var i = 0; i < _sortMeta.Count; i++)
        {
            if (string.Equals(_sortMeta[i].Field, field, StringComparison.OrdinalIgnoreCase)) return i + 1;
        }

        return 0;
    }

    private bool ShowSkeleton => Loading && string.Equals(LoadingMode, "skeleton", StringComparison.OrdinalIgnoreCase);

    private int SkeletonRowCount => SkeletonRows > 0 ? SkeletonRows : _rows;

    private int DisplayCount
    {
        get
        {
            EnsureComputed();
            return _displayCount;
        }
    }

    private int PageCount => Paginator && _rows > 0
        ? Math.Max(1, (int)Math.Ceiling(DisplayCount / (double)_rows))
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
    // Items/filtro/sort/página.
    private void EnsureComputed()
    {
        if (_computed)
        {
            return;
        }

        if (Lazy)
        {
            // Items já é a página corrente; não há filtro/sort/paginação client-side.
            _sortedCache = SourceItems.ToList();
            _pageCache = _sortedCache;
            _displayCount = TotalRecords;
            _computed = true;
            return;
        }

        var list = ApplyFilters(SourceItems);

        if (_sortMeta.Count > 0)
        {
            list.Sort(CompareItems);
        }

        _sortedCache = list;
        _displayCount = list.Count;

        if (Paginator && _rows > 0)
        {
            var skip = Math.Max(0, _first);
            _pageCache = list.Skip(skip).Take(_rows).ToList();
        }
        else
        {
            _pageCache = list;
        }

        _computed = true;
    }

    private List<TItem> ApplyFilters(IReadOnlyList<TItem> source)
    {
        IEnumerable<TItem> query = source;

        foreach (var col in Columns)
        {
            if (!col.Filter || string.IsNullOrEmpty(col.Field)) continue;
            if (!_columnFilters.TryGetValue(col.Field, out var value) || IsEmptyFilter(value)) continue;
            query = FilterService.Filter(query, new[] { col.Field }, value, col.FilterMatchMode);
        }

        if (!string.IsNullOrEmpty(_globalFilter))
        {
            var fields = GlobalFilterFields
                ?? Columns.Select(c => c.Field).Where(f => !string.IsNullOrEmpty(f)).ToList();
            query = FilterService.Filter(query, fields, _globalFilter, OpFilterMatchMode.Contains);
        }

        return query.ToList();
    }

    private static bool IsEmptyFilter(object? value)
        => value is null || (value is string text && text.Trim().Length == 0);

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

        if (GlobalFilter != _lastGlobalFilterParam)
        {
            _lastGlobalFilterParam = GlobalFilter;
            _globalFilter = GlobalFilter;
        }

        if (!string.Equals(SortField, _lastSortFieldParam, StringComparison.Ordinal) || SortOrder != _lastSortOrderParam)
        {
            _lastSortFieldParam = SortField;
            _lastSortOrderParam = SortOrder;
            _sortMeta.Clear();
            if (!string.IsNullOrEmpty(SortField) && SortOrder != 0)
            {
                _sortMeta.Add(new OpDataTableSortMeta { Field = SortField, Order = SortOrder > 0 ? 1 : -1 });
            }
        }

        if (!ReferenceEquals(MultiSortMeta, _lastMultiSortMetaParam))
        {
            _lastMultiSortMetaParam = MultiSortMeta;
            if (MultiSortMeta is { Count: > 0 })
            {
                _sortMeta.Clear();
                _sortMeta.AddRange(MultiSortMeta
                    .Where(m => !string.IsNullOrEmpty(m.Field) && m.Order != 0)
                    .Select(m => new OpDataTableSortMeta { Field = m.Field, Order = m.Order > 0 ? 1 : -1 }));
            }
        }

        if (_rows <= 0)
        {
            _rows = 1;
        }

        // Ajusta a página quando Items/Rows/filtro mudam: evita "No results found" numa página que
        // deixou de existir.
        var maxFirst = Math.Max(0, (PageCount - 1) * _rows);
        if (_first > maxFirst) _first = maxFirst;
        if (_first < 0) _first = 0;
        _computed = false;
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender && Lazy)
        {
            await RaiseLazyLoadAsync();
        }
    }

    private int CompareItems(TItem a, TItem b)
    {
        foreach (var meta in _sortMeta)
        {
            var cmp = SafeCompare(GetFieldValue(a, meta.Field), GetFieldValue(b, meta.Field));
            if (cmp != 0)
            {
                return meta.Order == 1 ? cmp : -cmp;
            }
        }

        return 0;
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

    private bool AllSelected
    {
        get
        {
            EnsureComputed();
            var universe = SelectionPageOnly ? PageItems : (_sortedCache ?? PageItems);
            return universe.Count > 0 && universe.All(IsSelected);
        }
    }

    private TItem? SelectedSingle
    {
        get
        {
            var selected = Selected;
            return selected.Count > 0 ? selected[0] : default;
        }
    }

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
        OrderOf(col.Field) != 0 ? "p-datatable-column-sorted" : null,
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
        var order = OrderOf(col.Field);
        if (order == 0) return "sort-alt";
        return order == 1 ? "sort-amount-up-alt" : "sort-amount-down";
    }

    private bool IsSameRow(TItem a, TItem b)
    {
        if (!string.IsNullOrEmpty(DataKey))
        {
            return Equals(GetFieldValue(a, DataKey), GetFieldValue(b, DataKey));
        }

        return EqualityComparer<TItem>.Default.Equals(a, b);
    }

    private bool IsSelected(TItem item)
    {
        var sel = SelectedItems;
        if (sel is null) return false;

        foreach (var s in sel)
        {
            if (IsSameRow(s, item)) return true;
        }

        return false;
    }

    private string? SortAria(OpDataTableColumn col)
    {
        if (!Sortable || !col.Sortable) return null;
        var order = OrderOf(col.Field);
        if (order != 0)
        {
            return order == 1 ? "ascending" : "descending";
        }

        return "none";
    }

    private string? ColumnFilterValue(string field)
        => _columnFilters.TryGetValue(field, out var value) ? value?.ToString() : null;

    private Task OnHeaderKeydown(KeyboardEventArgs e, OpDataTableColumn col)
        => e.Key is "Enter" or " " or "Spacebar" ? ToggleSortAsync(col, e.CtrlKey || e.MetaKey) : Task.CompletedTask;

    private async Task ToggleSortAsync(OpDataTableColumn col, bool multiKey)
    {
        if (!Sortable || !col.Sortable || string.IsNullOrEmpty(col.Field)) return;

        var index = _sortMeta.FindIndex(m => string.Equals(m.Field, col.Field, StringComparison.OrdinalIgnoreCase));

        if (IsMultipleSort && multiKey)
        {
            if (index < 0)
            {
                _sortMeta.Add(new OpDataTableSortMeta { Field = col.Field, Order = 1 });
            }
            else if (_sortMeta[index].Order == 1)
            {
                _sortMeta[index] = new OpDataTableSortMeta { Field = col.Field, Order = -1 };
            }
            else if (RemovableSort)
            {
                _sortMeta.RemoveAt(index);
            }
            else
            {
                _sortMeta[index] = new OpDataTableSortMeta { Field = col.Field, Order = 1 };
            }
        }
        else
        {
            // Solteira (ou múltipla sem ⌘/Ctrl): a ordenação fica só nesta coluna.
            // Ciclo asc → desc → asc; com RemovableSort o terceiro clique remove a ordenação.
            var current = _sortMeta.Count == 1 ? OrderOf(col.Field) : 0;
            _sortMeta.Clear();
            if (current != -1 || !RemovableSort)
            {
                _sortMeta.Add(new OpDataTableSortMeta { Field = col.Field, Order = current == 1 ? -1 : 1 });
            }
        }

        _first = 0;
        _computed = false;
        await OnSort.InvokeAsync(new OpDataTableSortEvent
        {
            Field = col.Field,
            Order = OrderOf(col.Field),
            MultiSortMeta = _sortMeta.ToList()
        });
        await RaiseLazyLoadAsync();
    }

    private async Task OnColumnFilterChanged(string field, string? value)
    {
        if (string.IsNullOrEmpty(value))
        {
            _columnFilters.Remove(field);
        }
        else
        {
            _columnFilters[field] = value;
        }

        _first = 0;
        _computed = false;
        await OnFilter.InvokeAsync(new Dictionary<string, object?>(_columnFilters, StringComparer.OrdinalIgnoreCase));
        await RaiseLazyLoadAsync();
    }

    /// <summary>Filtro global imperativo (como o <c>filterGlobal</c> do upstream).</summary>
    public async Task FilterGlobalAsync(string? value, OpFilterMatchMode matchMode = OpFilterMatchMode.Contains)
    {
        _globalFilter = value;
        _lastGlobalFilterParam = value;
        _first = 0;
        _computed = false;

        if (GlobalFilterChanged.HasDelegate)
        {
            await GlobalFilterChanged.InvokeAsync(value);
        }

        await RaiseLazyLoadAsync();
        await InvokeAsync(StateHasChanged);
    }

    private async Task RaiseLazyLoadAsync()
    {
        if (!Lazy)
        {
            return;
        }

        var first = _sortMeta.Count > 0 ? _sortMeta[0] : null;
        await OnLazyLoad.InvokeAsync(new OpDataTableLazyLoadEvent
        {
            First = _first,
            Rows = _rows,
            SortField = first?.Field,
            SortOrder = first?.Order ?? 0,
            MultiSortMeta = _sortMeta.Count > 0 ? _sortMeta.ToList() : null,
            Filters = new Dictionary<string, object?>(_columnFilters, StringComparer.OrdinalIgnoreCase),
            GlobalFilter = _globalFilter
        });
    }

    private async Task ToggleRowSelectionAsync(TItem item, bool metaKey = false)
    {
        if (IsSingleSelection)
        {
            var single = IsSelected(item) ? new List<TItem>() : new List<TItem> { item };
            await SelectedItemsChanged.InvokeAsync(single);
            return;
        }

        if (MetaKeySelection && !metaKey)
        {
            // Como no upstream: sem ⌘/Ctrl a seleção passa a ser apenas esta linha.
            await SelectedItemsChanged.InvokeAsync(new List<TItem> { item });
            return;
        }

        var list = Selected;
        var idx = list.FindIndex(s => IsSameRow(s, item));
        if (idx >= 0) list.RemoveAt(idx);
        else list.Add(item);
        await SelectedItemsChanged.InvokeAsync(list);
    }

    /// <summary>Seleciona a linha programaticamente (ex.: botão dentro de um template de célula).</summary>
    public async Task SelectRowAsync(TItem item)
    {
        if (IsSingleSelection)
        {
            await SelectedItemsChanged.InvokeAsync(new List<TItem> { item });
            return;
        }

        var list = Selected;
        if (list.All(s => !IsSameRow(s, item)))
        {
            list.Add(item);
        }

        await SelectedItemsChanged.InvokeAsync(list);
    }

    /// <summary>Remove a linha da seleção.</summary>
    public async Task UnselectRowAsync(TItem item)
    {
        var list = Selected;
        list.RemoveAll(s => IsSameRow(s, item));
        await SelectedItemsChanged.InvokeAsync(list);
    }

    private async Task ToggleAllSelection()
    {
        EnsureComputed();
        var universe = SelectionPageOnly ? PageItems : (_sortedCache ?? PageItems);

        var list = Selected;
        if (AllSelected)
        {
            foreach (var item in universe)
            {
                list.RemoveAll(s => IsSameRow(s, item));
            }
        }
        else
        {
            foreach (var item in universe)
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
        await RaiseLazyLoadAsync();
    }

    private async Task OnRowClicked(TItem item, MouseEventArgs e)
    {
        if (Selection)
        {
            await ToggleRowSelectionAsync(item, e.CtrlKey || e.MetaKey);
        }

        await OnRowClick.InvokeAsync(item);
    }

    /// <summary>
    /// Exporta os dados processados (filtrados e ordenados, sem paginação) em CSV.
    /// No modo lazy exporta apenas a página corrente (<c>Items</c>).
    /// </summary>
    public string ExportCsv()
    {
        EnsureComputed();

        var columns = Columns.Where(c => !IsSelectionColumn(c) && !string.IsNullOrEmpty(c.Field)).ToList();
        var sb = new StringBuilder();
        sb.Append(string.Join(',', columns.Select(c => CsvEscape(c.Header ?? c.Field))));
        sb.Append("\r\n");

        foreach (var item in _sortedCache ?? new List<TItem>())
        {
            sb.Append(string.Join(',', columns.Select(c => CsvEscape(GetFieldValue(item, c.Field)?.ToString() ?? ""))));
            sb.Append("\r\n");
        }

        return sb.ToString();
    }

    private static string CsvEscape(string value)
        => value.IndexOfAny(new[] { ',', '"', '\n', '\r' }) >= 0
            ? "\"" + value.Replace("\"", "\"\"") + "\""
            : value;

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
