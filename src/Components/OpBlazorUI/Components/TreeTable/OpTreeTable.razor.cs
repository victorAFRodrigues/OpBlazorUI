using System.Globalization;
using System.Reflection;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using OpBlazorUI.Components.Common;
using OpBlazorUI.Components.Paginator;
using OpBlazorUI.Globalization;
using OpBlazorUI.Models;
using OpBlazorUI.Services;

namespace OpBlazorUI.Components.TreeTable;

/// <summary>Meta de ordenação do <c>OpTreeTable</c> (mesmo contrato do <c>OpDataTableSortMeta</c>).</summary>
public sealed class OpTreeTableSortMeta
{
    public string Field { get; init; } = "";

    /// <summary>1 = ascendente, -1 = descendente.</summary>
    public int Order { get; init; }
}

/// <summary>Evento de mudança de ordenação do <c>OpTreeTable</c>.</summary>
public sealed class OpTreeTableSortEvent
{
    public string Field { get; init; } = "";
    public int Order { get; init; }
    public IReadOnlyList<OpTreeTableSortMeta> MultiSortMeta { get; init; } = Array.Empty<OpTreeTableSortMeta>();
}

/// <summary>
/// Evento do modo lazy (paginação/ordenação/filtro) do <c>OpTreeTable</c> — mesmo contrato do
/// <c>OpDataTableLazyLoadEvent</c>. A carga de filhos de um nó continua via <c>OnNodeExpand</c>.
/// </summary>
public sealed class OpTreeTableLazyLoadEvent
{
    public int First { get; init; }
    public int Rows { get; init; }
    public string? SortField { get; init; }
    public int SortOrder { get; init; }
    public IReadOnlyList<OpTreeTableSortMeta>? MultiSortMeta { get; init; }
    public IReadOnlyDictionary<string, object?>? Filters { get; init; }
    public string? GlobalFilter { get; init; }
}

/// <summary>Célula de um cabeçalho agrupado (linhas de <c>HeaderGroups</c>).</summary>
public sealed class OpTreeTableHeaderGroup
{
    public string Header { get; set; } = "";
    public int ColSpan { get; set; } = 1;
    public int RowSpan { get; set; } = 1;
    public string? StyleClass { get; set; }
}

public partial class OpTreeTable : OpComponentBase
{
    [Inject] private OpTranslationService TranslationService { get; set; } = default!;
    [Inject] private OpFilterService FilterService { get; set; } = default!;

    private OpTranslation Tr => TranslationService.Translation;

    private ElementReference _tableRoot;

    // ------------------------------------------------------------------ params
    [Parameter] public IReadOnlyList<OpTreeNode>? Nodes { get; set; }

    [Parameter] public IReadOnlyList<OpTreeTableColumn> Columns { get; set; } = Array.Empty<OpTreeTableColumn>();

    /// <summary>Modo de seleção: <c>single</c>, <c>multiple</c> ou <c>checkbox</c>.</summary>
    [Parameter] public string? SelectionMode { get; set; }

    /// <summary>
    /// Seleção atual. Em <c>single</c> é um <see cref="OpTreeNode"/>; em <c>multiple</c>/<c>checkbox</c>
    /// é um <see cref="IReadOnlyDictionary{TKey,TValue}"/> de <c>Key</c> do nó para <c>bool</c>.
    /// </summary>
    [Parameter] public object? Selection { get; set; }

    [Parameter] public EventCallback<object?> SelectionChanged { get; set; }

    [Parameter] public bool Loading { get; set; }

    /// <summary>Modo de carregamento: <c>mask</c> (padrão) mostra a máscara; <c>skeleton</c> mostra linhas esqueleto.</summary>
    [Parameter] public string LoadingMode { get; set; } = "mask";

    /// <summary>Número de linhas esqueleto quando <see cref="LoadingMode"/> é <c>skeleton</c> (padrão: <see cref="Rows"/>).</summary>
    [Parameter] public int SkeletonRows { get; set; }

    [Parameter] public string? EmptyMessage { get; set; }

    // ordenação -----------------------------------------------------------------
    /// <summary>Habilita a ordenação (combinado com <c>Sortable</c> de cada coluna).</summary>
    [Parameter] public bool Sortable { get; set; } = true;

    /// <summary><c>single</c> (padrão) ou <c>multiple</c> — no múltiplo o clique com ⌘/Ctrl monta a ordenação em camadas.</summary>
    [Parameter] public string SortMode { get; set; } = "single";

    /// <summary>Com <c>RemovableSort</c> o terceiro clique remove a ordenação da coluna (nosso equivalente ao customSort da demo).</summary>
    [Parameter] public bool RemovableSort { get; set; }

    /// <summary>Ordenação controlada pela página (pré-ordenação).</summary>
    [Parameter] public string? SortField { get; set; }

    /// <summary>1 = ascendente, -1 = descendente, 0 = sem ordenação (com <see cref="SortField"/>).</summary>
    [Parameter] public int SortOrder { get; set; }

    /// <summary>Ordenação múltipla controlada pela página (usa a ordem da lista).</summary>
    [Parameter] public IReadOnlyList<OpTreeTableSortMeta>? MultiSortMeta { get; set; }

    [Parameter] public EventCallback<OpTreeTableSortEvent> OnSort { get; set; }

    // filtro --------------------------------------------------------------------
    /// <summary><c>lenient</c> (padrão) mantém a subárvore inteira de um nó que casa; <c>strict</c> continua filtrando os descendentes.</summary>
    [Parameter] public string FilterMode { get; set; } = "lenient";

    [Parameter] public string? GlobalFilter { get; set; }

    [Parameter] public EventCallback<string?> GlobalFilterChanged { get; set; }

    [Parameter] public IReadOnlyList<string>? GlobalFilterFields { get; set; }

    [Parameter] public EventCallback<IReadOnlyDictionary<string, object?>> OnFilter { get; set; }

    // layout --------------------------------------------------------------------
    [Parameter] public bool RowHover { get; set; } = true;

    [Parameter] public bool ShowGridlines { get; set; }

    /// <summary>Tamanho: <c>small</c> ou <c>large</c>.</summary>
    [Parameter] public string? Size { get; set; }

    /// <summary>Altura máxima do viewport de dados (rolagem vertical com cabeçalho fixo); <c>flex</c> preenche a altura do pai.</summary>
    [Parameter] public string? ScrollHeight { get; set; }

    [Parameter] public bool StickyHeader { get; set; }

    /// <summary>Estilo inline da <c>table</c> (equivalente ao <c>tableStyle</c> do upstream; ex.: <c>min-width: 50rem</c>).</summary>
    [Parameter] public string? TableStyle { get; set; }

    // paginação -----------------------------------------------------------------
    [Parameter] public bool Paginator { get; set; }

    [Parameter] public int Rows { get; set; } = 10;

    [Parameter] public EventCallback<int> RowsChanged { get; set; }

    [Parameter] public int First { get; set; }

    [Parameter] public EventCallback<int> FirstChanged { get; set; }

    [Parameter] public int TotalRecords { get; set; }

    [Parameter] public bool Lazy { get; set; }

    /// <summary>Posição do paginador: <c>top</c>, <c>bottom</c> ou <c>both</c>.</summary>
    [Parameter] public string PaginatorPosition { get; set; } = "bottom";

    [Parameter] public int PageLinks { get; set; } = 5;

    [Parameter] public bool AlwaysShowPaginator { get; set; } = true;

    [Parameter] public IReadOnlyList<int>? RowsPerPageOptions { get; set; }

    /// <summary>Conteúdo à esquerda dos links do paginador (equivalente ao <c>paginatorleft</c> do upstream).</summary>
    [Parameter] public RenderFragment? PaginatorLeftTemplate { get; set; }

    /// <summary>Conteúdo à direita dos links do paginador (equivalente ao <c>paginatorright</c> do upstream).</summary>
    [Parameter] public RenderFragment? PaginatorRightTemplate { get; set; }

    [Parameter] public EventCallback<OpPaginatorState> OnPageChange { get; set; }

    [Parameter] public EventCallback<OpTreeTableLazyLoadEvent> OnLazyLoad { get; set; }

    // colunas -------------------------------------------------------------------
    /// <summary>Linhas de cabeçalho agrupado; as células declaram colSpan/rowSpan (equivale ao #header em camadas do upstream).</summary>
    [Parameter] public IReadOnlyList<IReadOnlyList<OpTreeTableHeaderGroup>>? HeaderGroups { get; set; }

    [Parameter] public bool ReorderableColumns { get; set; }

    [Parameter] public EventCallback<(int From, int To)> OnColumnReorder { get; set; }

    // linhas --------------------------------------------------------------------
    /// <summary>Classe condicional por linha (equivale ao ngClass na linha da demo de estilo condicional do upstream).</summary>
    [Parameter] public Func<OpTreeNode, string?>? RowClass { get; set; }

    [Parameter] public EventCallback<(OpTreeNode Item, MouseEventArgs Args)> OnRowContextMenu { get; set; }

    // events
    [Parameter] public EventCallback<OpTreeNode> OnNodeExpand { get; set; }
    [Parameter] public EventCallback<OpTreeNode> OnNodeCollapse { get; set; }
    [Parameter] public EventCallback<OpTreeNode> OnNodeSelect { get; set; }
    [Parameter] public EventCallback<OpTreeNode> OnNodeUnselect { get; set; }

    // templates
    [Parameter] public RenderFragment? HeaderTemplate { get; set; }
    [Parameter] public RenderFragment? FooterTemplate { get; set; }
    [Parameter] public RenderFragment? EmptyTemplate { get; set; }
    [Parameter] public RenderFragment? LoadingTemplate { get; set; }
    [Parameter] public RenderFragment? CaptionTemplate { get; set; }

    // ------------------------------------------------------------------ estado
    private readonly List<OpTreeTableSortMeta> _sortMeta = new();
    private string? _lastSortFieldParam;
    private int _lastSortOrderParam;
    private IReadOnlyList<OpTreeTableSortMeta>? _lastMultiSortMetaParam;

    private string? _globalFilter;
    private string? _lastGlobalFilterParam;
    private readonly Dictionary<string, object?> _columnFilters = new(StringComparer.OrdinalIgnoreCase);

    private List<OpTreeNode>? _viewRoots;
    private Dictionary<OpTreeNode, List<OpTreeNode>>? _viewChildren;
    private bool _isFiltering;
    private bool _viewDirty = true;

    private int _dragColumnIndex = -1;

    // Estado interno de paginação (como no OpDataTable): callbacks FirstChanged/RowsChanged avisam a página.
    private int _first;
    private int _rows = 10;
    private int _lastFirstParam = int.MinValue;
    private int _lastRowsParam = int.MinValue;

    // ------------------------------------------------------------------ helpers
    private IReadOnlyList<OpTreeNode> SourceNodes => Nodes ?? Array.Empty<OpTreeNode>();

    private bool IsCheckbox => SelectionMode == "checkbox";

    private bool IsMultiple => SelectionMode == "multiple" || IsCheckbox;

    private bool IsSelectable => SelectionMode is "single" or "multiple" or "checkbox";

    private bool ShowSelectionColumn => IsCheckbox;

    private int ColumnSpan => Math.Max(1, Columns.Count + (ShowSelectionColumn ? 1 : 0));

    private bool IsMultipleSort => string.Equals(SortMode, "multiple", StringComparison.OrdinalIgnoreCase);

    private bool IsStrictFilter => string.Equals(FilterMode, "strict", StringComparison.OrdinalIgnoreCase);

    private bool HasFilters => !Lazy && Columns.Any(c => c.Filter && !string.IsNullOrEmpty(c.Field));

    private bool HasHeaderGroups => HeaderGroups is { Count: > 0 };

    private bool HasFooter => FooterTemplate is not null
        || Columns.Any(c => c.Footer is not null || c.FooterTemplate is not null);

    private string RootClass => Class(
        "p-treetable p-component",
        RowHover || IsSelectable ? "p-treetable-hoverable" : null,
        ShowGridlines ? "p-treetable-gridlines" : null,
        Size == "small" ? "p-treetable-sm" : null,
        Size == "large" ? "p-treetable-lg" : null,
        HasScrollableLayout ? "p-treetable-scrollable" : null,
        // Paridade com scrollHeight="flex": a raiz vira flex column de 100% e o contêiner preenche o pai.
        ScrollHeight == "flex" ? "p-treetable-flex-scrollable" : null,
        StyleClass);

    // O tema só torna colunas congeladas sticky e pinta o cabeçalho no modo "scrollable" do upstream.
    private bool HasScrollableLayout => ScrollHeight is not null || Columns.Any(IsFrozen);

    private OpTreeTableColumn? ExpandableColumn =>
        Columns.FirstOrDefault(c => c.Expandable) ?? Columns.FirstOrDefault();

    private bool IsExpandableColumn(OpTreeTableColumn column) => ReferenceEquals(column, ExpandableColumn);

    private string TableContainerStyle => ScrollHeight is null or "flex"
        ? "overflow: auto;"
        : $"overflow: auto; max-height: {ScrollHeight};";

    private string TheadStyle => StickyHeader ? "position: sticky; top: 0; z-index: 1;" : "position: sticky;";

    // ------------------------------------------------------------------ view (filtro + ordenação sem clonar nós)
    /// <summary>Total exibido no paginador: raízes da view (cliente) ou <see cref="TotalRecords"/> (lazy).</summary>
    private int DisplayCount
    {
        get
        {
            if (Lazy)
            {
                return TotalRecords;
            }

            EnsureView();
            return _viewRoots!.Count;
        }
    }

    private int PageCount => Paginator && _rows > 0
        ? Math.Max(1, (int)Math.Ceiling(DisplayCount / (double)_rows))
        : 1;

    private bool HasActiveColumnFilters => _columnFilters.Values.Any(v => !IsEmptyFilter(v));

    // O filtro global vale mesmo sem colunas de filtro (ao contrário da linha de filtros, que depende de HasFilters).
    private bool IsFiltering => !Lazy && (HasActiveColumnFilters || !string.IsNullOrEmpty(_globalFilter));

    private void EnsureView()
    {
        if (!_viewDirty)
        {
            return;
        }

        _viewDirty = false;
        _isFiltering = IsFiltering;

        var children = new Dictionary<OpTreeNode, List<OpTreeNode>>();
        var roots = new List<OpTreeNode>();

        if (Lazy)
        {
            // No modo lazy a página atual já vem pronta do servidor: sem filtro/sort client-side.
            foreach (var root in SourceNodes)
            {
                roots.Add(root);
                MapSubtree(root, children);
            }
        }
        else
        {
            foreach (var root in SourceNodes)
            {
                if (IncludeNode(root, children))
                {
                    roots.Add(root);
                }
            }

            if (_sortMeta.Count > 0)
            {
                // Sort estável: empates não trocam de posição entre renders.
                roots = roots.OrderBy(x => x, NodeComparer).ToList();
            }
        }

        _viewChildren = children;
        _viewRoots = roots;
    }

    private IComparer<OpTreeNode> NodeComparer => Comparer<OpTreeNode>.Create(CompareNodes);

    /// <summary>
    /// Monta a view (filtro first, sort por nível depois) sem alterar os nós: os filhos visíveis vão para
    /// um mapa por nó e o desenho usa esse mapa — assim os eventos continuam recebendo os nós reais.
    /// </summary>
    private bool IncludeNode(OpTreeNode node, Dictionary<OpTreeNode, List<OpTreeNode>> children)
    {
        if (!_isFiltering)
        {
            MapSubtree(node, children);
            return true;
        }

        var matched = MatchesFilters(node);
        if (matched && !IsStrictFilter)
        {
            MapSubtree(node, children);
            return true;
        }

        var kept = new List<OpTreeNode>();
        if (node.Children is not null)
        {
            foreach (var child in node.Children)
            {
                if (IncludeNode(child, children))
                {
                    kept.Add(child);
                }
            }
        }

        if (kept.Count > 0)
        {
            children[node] = OrderChildren(kept);
            return true;
        }

        if (matched)
        {
            // Modo estrito: o nó casa, mas nenhum descendente casa — fica sem filhos na view.
            children[node] = kept;
            return true;
        }

        return false;
    }

    private void MapSubtree(OpTreeNode node, Dictionary<OpTreeNode, List<OpTreeNode>> children)
    {
        if (node.Children is { Count: > 0 })
        {
            children[node] = OrderChildren(node.Children);
            foreach (var child in node.Children)
            {
                MapSubtree(child, children);
            }
        }
    }

    private List<OpTreeNode> OrderChildren(List<OpTreeNode> source)
    {
        if (_sortMeta.Count == 0)
        {
            return source;
        }

        return source.OrderBy(x => x, NodeComparer).ToList();
    }

    private int CompareNodes(OpTreeNode a, OpTreeNode b)
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

    private static bool IsEmptyFilter(object? value)
        => value is null || (value is string text && text.Trim().Length == 0);

    private bool MatchesFilters(OpTreeNode node)
    {
        foreach (var col in Columns)
        {
            if (!col.Filter || string.IsNullOrEmpty(col.Field))
            {
                continue;
            }

            if (!_columnFilters.TryGetValue(col.Field, out var value) || IsEmptyFilter(value))
            {
                continue;
            }

            if (!MatchesField(node, col.Field, value, col.FilterMatchMode))
            {
                return false;
            }
        }

        if (!string.IsNullOrEmpty(_globalFilter))
        {
            var fields = GlobalFilterFields is { Count: > 0 }
                ? GlobalFilterFields
                : Columns.Select(c => c.Field).Where(f => !string.IsNullOrEmpty(f)).ToList();
            if (!fields.Any(field => MatchesField(node, field, _globalFilter, OpFilterMatchMode.Contains)))
            {
                return false;
            }
        }

        return true;
    }

    private bool MatchesField(OpTreeNode node, string field, object? value, OpFilterMatchMode matchMode)
        => FilterService.IsMatch(GetFieldValue(node, field), value, matchMode);

    private List<OpTreeTableFlatNode> VisibleNodes
    {
        get
        {
            EnsureView();
            var list = new List<OpTreeTableFlatNode>();

            var roots = !Paginator || Lazy
                ? _viewRoots!
                : _viewRoots!.Skip(Math.Max(0, _first)).Take(Math.Max(1, _rows));

            foreach (var root in roots)
            {
                AppendFlat(root, 0, list);
            }

            return list;
        }
    }

    private void AppendFlat(OpTreeNode node, int level, List<OpTreeTableFlatNode> list)
    {
        list.Add(new OpTreeTableFlatNode(node, level));
        if (!IsNodeExpanded(node) || ChildrenOf(node) is not { Count: > 0 } children)
        {
            return;
        }

        foreach (var child in children)
        {
            AppendFlat(child, level + 1, list);
        }
    }

    private List<OpTreeNode>? ChildrenOf(OpTreeNode node)
        => _viewChildren is not null && _viewChildren.TryGetValue(node, out var list) ? list : node.Children;

    /// <summary>
    /// Estado de expansão para desenho: com filtro ativo, nós com filhos visíveis aparecem expandidos
    /// (como o filtro do upstream faz com os ancestrais dos resultados) sem alterar <c>Expanded</c>.
    /// </summary>
    private bool IsNodeExpanded(OpTreeNode node)
        => node.Expanded || (_isFiltering && ChildrenOf(node) is { Count: > 0 });

    private List<OpTreeNode> AllNodes()
    {
        var list = new List<OpTreeNode>();

        void Walk(OpTreeNode node)
        {
            list.Add(node);
            if (node.Children is not null)
            {
                foreach (var child in node.Children)
                {
                    Walk(child);
                }
            }
        }

        foreach (var root in SourceNodes)
        {
            Walk(root);
        }

        return list;
    }

    // ------------------------------------------------------------------ classes
    private int OrderOf(string field)
    {
        if (string.IsNullOrEmpty(field)) return 0;
        foreach (var meta in _sortMeta)
        {
            if (string.Equals(meta.Field, field, StringComparison.OrdinalIgnoreCase)) return meta.Order;
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

    private string SortIconName(OpTreeTableColumn col)
    {
        var order = OrderOf(col.Field);
        if (order == 0) return "sort-alt";
        return order == 1 ? "sort-amount-up-alt" : "sort-amount-down";
    }

    private string? SortAria(OpTreeTableColumn col)
    {
        if (!Sortable || !col.Sortable) return null;
        var order = OrderOf(col.Field);
        if (order != 0)
        {
            return order == 1 ? "ascending" : "descending";
        }

        return "none";
    }

    private string ThClass(OpTreeTableColumn column) => Class(
        "p-treetable-header-cell",
        column.Sortable && Sortable ? "p-treetable-sortable-column" : null,
        OrderOf(column.Field) != 0 ? "p-treetable-column-sorted" : null,
        IsFrozen(column) ? "p-treetable-frozen-column" : null,
        ReorderableColumns ? "p-treetable-reorderable-column" : null,
        column.StyleClass,
        column.HeaderStyleClass);

    private string? ThStyle(OpTreeTableColumn column)
    {
        var parts = new List<string>();
        if (!string.IsNullOrEmpty(column.Width)) parts.Add($"width: {column.Width}");
        var frozen = FrozenStyle(column);
        if (frozen is not null) parts.Add(frozen);
        return parts.Count > 0 ? string.Join("; ", parts) : null;
    }

    private string TrClass(OpTreeNode node) => Class(
        "p-treetable-row",
        IsSelectable && node.Selectable ? "p-treetable-selectable-row" : null,
        IsSelected(node) ? "p-treetable-row-selected" : null,
        RowClass?.Invoke(node));

    private string TdClass(OpTreeNode node, OpTreeTableColumn column) => Class(
        "p-treetable-body-cell",
        IsExpandableColumn(column) ? "p-treetable-toggler-column" : null,
        IsFrozen(column) ? "p-treetable-frozen-column" : null,
        column.StyleClass,
        column.BodyStyleClass);

    private string? TdStyle(OpTreeTableColumn column) => FrozenStyle(column);

    private string CellStyle(OpTreeTableFlatNode flat, OpTreeTableColumn column)
    {
        var indent = IsExpandableColumn(column) && flat.Level > 0
            ? $"padding-inline-start:calc({flat.Level} * var(--p-treetable-indent, 1rem));"
            : null;
        return indent ?? "";
    }

    private static bool IsFrozen(OpTreeTableColumn column) => !string.IsNullOrEmpty(column.Frozen);

    private static double ParsePx(string? width)
        => width is not null && width.EndsWith("px", StringComparison.OrdinalIgnoreCase)
           && double.TryParse(width[..^2], NumberStyles.Float, CultureInfo.InvariantCulture, out var value)
            ? value
            : 0;

    /// <summary>Deslocamento sticky da coluna congelada (soma das larguras congeladas anteriores).</summary>
    private string? FrozenStyle(OpTreeTableColumn column)
    {
        if (string.IsNullOrEmpty(column.Frozen)) return null;

        var right = string.Equals(column.Frozen, "right", StringComparison.OrdinalIgnoreCase);
        var frozenColumns = Columns
            .Where(c => string.Equals(c.Frozen, right ? "right" : "left", StringComparison.OrdinalIgnoreCase))
            .ToList();

        var offset = 0d;
        var index = frozenColumns.IndexOf(column);
        if (right)
        {
            for (var i = index + 1; i < frozenColumns.Count; i++) offset += ParsePx(frozenColumns[i].Width);
        }
        else
        {
            for (var i = 0; i < index; i++) offset += ParsePx(frozenColumns[i].Width);
        }

        var side = right ? "right" : "left";
        return $"{side}: {offset.ToString(CultureInfo.InvariantCulture)}px";
    }

    // ------------------------------------------------------------------ ordenação
    private Task OnHeaderKeydown(KeyboardEventArgs e, OpTreeTableColumn col)
        => e.Key is "Enter" or " " or "Spacebar" ? ToggleSortAsync(col, e.CtrlKey || e.MetaKey) : Task.CompletedTask;

    /// <summary>Alterna a ordenação da coluna (ciclo asc → desc; com <c>RemovableSort</c> o terceiro clique remove).</summary>
    public async Task ToggleSortAsync(OpTreeTableColumn col, bool multiKey)
    {
        if (!Sortable || !col.Sortable || string.IsNullOrEmpty(col.Field)) return;

        var index = _sortMeta.FindIndex(m => string.Equals(m.Field, col.Field, StringComparison.OrdinalIgnoreCase));

        if (IsMultipleSort && multiKey)
        {
            if (index < 0)
            {
                _sortMeta.Add(new OpTreeTableSortMeta { Field = col.Field, Order = 1 });
            }
            else if (_sortMeta[index].Order == 1)
            {
                _sortMeta[index] = new OpTreeTableSortMeta { Field = col.Field, Order = -1 };
            }
            else if (RemovableSort)
            {
                _sortMeta.RemoveAt(index);
            }
            else
            {
                _sortMeta[index] = new OpTreeTableSortMeta { Field = col.Field, Order = 1 };
            }
        }
        else
        {
            // Solteira (ou múltipla sem ⌘/Ctrl): a ordenação fica só nesta coluna.
            var current = _sortMeta.Count == 1 ? OrderOf(col.Field) : 0;
            _sortMeta.Clear();
            if (current != -1 || !RemovableSort)
            {
                _sortMeta.Add(new OpTreeTableSortMeta { Field = col.Field, Order = current == 1 ? -1 : 1 });
            }
        }

        _viewDirty = true;
        await ResetPageAsync();
        await OnSort.InvokeAsync(new OpTreeTableSortEvent
        {
            Field = col.Field,
            Order = OrderOf(col.Field),
            MultiSortMeta = _sortMeta.ToList()
        });
        await RaiseLazyLoadAsync();
        await InvokeAsync(StateHasChanged);
    }

    // ------------------------------------------------------------------ filtro
    private string? ColumnFilterValue(string field)
        => _columnFilters.TryGetValue(field, out var value) ? value?.ToString() : null;

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

        _viewDirty = true;
        await ResetPageAsync();
        await OnFilter.InvokeAsync(new Dictionary<string, object?>(_columnFilters, StringComparer.OrdinalIgnoreCase));
        await RaiseLazyLoadAsync();
        await InvokeAsync(StateHasChanged);
    }

    /// <summary>Filtro global imperativo (como o <c>filterGlobal</c> do upstream).</summary>
    public async Task FilterGlobalAsync(string? value, OpFilterMatchMode matchMode = OpFilterMatchMode.Contains)
    {
        _globalFilter = value;
        _lastGlobalFilterParam = value;
        _viewDirty = true;
        await ResetPageAsync();

        if (GlobalFilterChanged.HasDelegate)
        {
            await GlobalFilterChanged.InvokeAsync(value);
        }

        await RaiseLazyLoadAsync();
        await InvokeAsync(StateHasChanged);
    }

    private async Task ResetPageAsync()
    {
        if (_first != 0)
        {
            _first = 0;
            await FirstChanged.InvokeAsync(_first);
        }
    }

    // ------------------------------------------------------------------ colunas (reorder)
    private async Task OnDropColumnAsync(int index)
    {
        var from = _dragColumnIndex;
        _dragColumnIndex = -1;
        if (from < 0 || from == index) return;
        await OnColumnReorder.InvokeAsync((from, index));
    }

    // ------------------------------------------------------------------ selection
    private bool IsSelected(OpTreeNode node)
    {
        if (IsMultiple)
        {
            return Selection is IReadOnlyDictionary<string, bool> dict
                   && dict.TryGetValue(node.Key, out var value) && value;
        }

        return Selection is OpTreeNode selected
               && (ReferenceEquals(selected, node) || selected.Key == node.Key);
    }

    private bool IsPartial(OpTreeNode node)
    {
        if (!IsMultiple || IsSelected(node) || !node.HasChildren)
        {
            return false;
        }

        return HasSelectedDescendant(node);
    }

    private bool HasSelectedDescendant(OpTreeNode node)
    {
        if (node.Children is null)
        {
            return false;
        }

        foreach (var child in node.Children)
        {
            if (IsSelected(child) || HasSelectedDescendant(child))
            {
                return true;
            }
        }

        return false;
    }

    private bool AllSelected
    {
        get
        {
            var nodes = AllNodes().Where(n => n.Selectable).ToList();
            return nodes.Count > 0 && nodes.All(IsSelected);
        }
    }

    private bool AllPartial => AllNodes().Where(n => n.Selectable).Any(IsPartial);

    private static List<OpTreeNode> Descendants(OpTreeNode node)
    {
        var list = new List<OpTreeNode>();
        if (node.Children is null)
        {
            return list;
        }

        void Walk(OpTreeNode current)
        {
            list.Add(current);
            if (current.Children is not null)
            {
                foreach (var child in current.Children)
                {
                    Walk(child);
                }
            }
        }

        foreach (var child in node.Children)
        {
            Walk(child);
        }

        return list;
    }

    private void PropagateUp(OpTreeNode node, Dictionary<string, bool> dict)
    {
        var path = new List<OpTreeNode>();
        foreach (var root in SourceNodes)
        {
            if (FindPath(root, node, path))
            {
                break;
            }
        }

        for (var i = path.Count - 2; i >= 0; i--)
        {
            var ancestor = path[i];
            var children = ancestor.Children ?? new List<OpTreeNode>();
            var selectable = children.Where(c => c.Selectable).ToList();
            var allSelected = selectable.Count > 0
                              && selectable.All(c => dict.TryGetValue(c.Key, out var v) && v);
            if (allSelected)
            {
                dict[ancestor.Key] = true;
            }
            else
            {
                dict.Remove(ancestor.Key);
            }
        }
    }

    private static bool FindPath(OpTreeNode current, OpTreeNode target, List<OpTreeNode> path)
    {
        path.Add(current);
        if (ReferenceEquals(current, target))
        {
            return true;
        }

        if (current.Children is not null)
        {
            foreach (var child in current.Children)
            {
                if (FindPath(child, target, path))
                {
                    return true;
                }
            }
        }

        path.RemoveAt(path.Count - 1);
        return false;
    }

    // ------------------------------------------------------------------ interaction
    private async Task ToggleExpand(OpTreeNode node)
    {
        if (node.IsLeaf || node.Loading)
        {
            return;
        }

        if (node.HasChildren)
        {
            node.Expanded = !node.Expanded;
            if (node.Expanded)
            {
                await OnNodeExpand.InvokeAsync(node);
            }
            else
            {
                await OnNodeCollapse.InvokeAsync(node);
            }
        }
        else
        {
            node.Loading = true;
            node.Expanded = true;
            // Renderiza o spinner do nó antes de esperar a carga (caso lazy do upstream).
            StateHasChanged();
            await OnNodeExpand.InvokeAsync(node);
        }

        StateHasChanged();
    }

    private async Task OnRowClicked(OpTreeNode node)
    {
        if (!IsSelectable || !node.Selectable)
        {
            return;
        }

        if (IsCheckbox)
        {
            await ToggleCheckboxAsync(node);
        }
        else if (IsMultiple)
        {
            await ToggleKeyAsync(node);
        }
        else
        {
            await ToggleSingleAsync(node);
        }
    }

    private async Task ToggleSingleAsync(OpTreeNode node)
    {
        var isSelected = Selection is OpTreeNode selected
                         && (ReferenceEquals(selected, node) || selected.Key == node.Key);

        if (isSelected)
        {
            Selection = null;
            await SelectionChanged.InvokeAsync(null);
            await OnNodeUnselect.InvokeAsync(node);
        }
        else
        {
            Selection = node;
            await SelectionChanged.InvokeAsync(node);
            await OnNodeSelect.InvokeAsync(node);
        }
    }

    private async Task ToggleKeyAsync(OpTreeNode node)
    {
        var dict = new Dictionary<string, bool>(CurrentSelectionMap());
        var nowSelected = dict.TryGetValue(node.Key, out var value) && value;

        if (nowSelected)
        {
            dict.Remove(node.Key);
        }
        else
        {
            dict[node.Key] = true;
        }

        Selection = dict;
        await SelectionChanged.InvokeAsync(dict);

        if (nowSelected)
        {
            await OnNodeUnselect.InvokeAsync(node);
        }
        else
        {
            await OnNodeSelect.InvokeAsync(node);
        }
    }

    private async Task ToggleCheckboxAsync(OpTreeNode node)
    {
        if (!node.Selectable)
        {
            return;
        }

        var dict = new Dictionary<string, bool>(CurrentSelectionMap());
        var newValue = !IsSelected(node);

        if (newValue)
        {
            dict[node.Key] = true;
        }
        else
        {
            dict.Remove(node.Key);
        }

        foreach (var descendant in Descendants(node))
        {
            if (!descendant.Selectable)
            {
                continue;
            }

            if (newValue)
            {
                dict[descendant.Key] = true;
            }
            else
            {
                dict.Remove(descendant.Key);
            }
        }

        PropagateUp(node, dict);

        Selection = dict;
        await SelectionChanged.InvokeAsync(dict);

        if (newValue)
        {
            await OnNodeSelect.InvokeAsync(node);
        }
        else
        {
            await OnNodeUnselect.InvokeAsync(node);
        }
    }

    private async Task ToggleAllNodes()
    {
        var dict = new Dictionary<string, bool>(CurrentSelectionMap());
        var select = !AllSelected;

        foreach (var node in AllNodes())
        {
            if (!node.Selectable)
            {
                continue;
            }

            if (select)
            {
                dict[node.Key] = true;
            }
            else
            {
                dict.Remove(node.Key);
            }
        }

        Selection = dict;
        await SelectionChanged.InvokeAsync(dict);
    }

    private IReadOnlyDictionary<string, bool> CurrentSelectionMap() =>
        Selection as IReadOnlyDictionary<string, bool> ?? new Dictionary<string, bool>();

    // ------------------------------------------------------------------ paginação / lazy
    private async Task OnPaginatorChange(OpPaginatorState state)
    {
        _first = state.First;
        if (state.Rows != _rows && state.Rows > 0)
        {
            _rows = state.Rows;
            await RowsChanged.InvokeAsync(_rows);
        }

        await FirstChanged.InvokeAsync(_first);
        await OnPageChange.InvokeAsync(state);
        await RaiseLazyLoadAsync();
    }

    private async Task RaiseLazyLoadAsync()
    {
        if (!Lazy)
        {
            return;
        }

        var first = _sortMeta.Count > 0 ? _sortMeta[0] : null;
        await OnLazyLoad.InvokeAsync(new OpTreeTableLazyLoadEvent
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

    // ------------------------------------------------------------------ carga (skeleton)
    private bool ShowSkeleton => Loading && string.Equals(LoadingMode, "skeleton", StringComparison.OrdinalIgnoreCase);

    private int SkeletonRowCount => SkeletonRows > 0 ? SkeletonRows : Math.Max(1, _rows);

    // ------------------------------------------------------------------ cell context
    private OpTreeTableCellContext CellContext(OpTreeTableFlatNode flat, OpTreeTableColumn column) =>
        new(flat.Node, flat.Level, column.Field);

    private object? GetFieldValue(OpTreeNode node, string field)
    {
        if (string.IsNullOrEmpty(field))
        {
            return node.Data;
        }

        var property = typeof(OpTreeNode).GetProperty(
            field, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
        if (property is not null)
        {
            return property.GetValue(node);
        }

        if (node.Data is not null)
        {
            var dataProperty = node.Data.GetType().GetProperty(
                field, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
            return dataProperty?.GetValue(node.Data);
        }

        return null;
    }

    // ------------------------------------------------------------------ lifecycle
    protected override void OnParametersSet()
    {
        _viewDirty = true;

        if (First != _lastFirstParam)
        {
            _lastFirstParam = First;
            _first = First;
        }

        if (Rows != _lastRowsParam)
        {
            _lastRowsParam = Rows;
            _rows = Rows > 0 ? Rows : 1;
        }

        if (!string.Equals(SortField, _lastSortFieldParam, StringComparison.Ordinal) || SortOrder != _lastSortOrderParam)
        {
            _lastSortFieldParam = SortField;
            _lastSortOrderParam = SortOrder;
            _sortMeta.Clear();
            if (!string.IsNullOrEmpty(SortField) && SortOrder != 0)
            {
                _sortMeta.Add(new OpTreeTableSortMeta { Field = SortField, Order = SortOrder > 0 ? 1 : -1 });
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
                    .Select(m => new OpTreeTableSortMeta { Field = m.Field, Order = m.Order > 0 ? 1 : -1 }));
            }
        }

        if (GlobalFilter != _lastGlobalFilterParam)
        {
            _lastGlobalFilterParam = GlobalFilter;
            _globalFilter = GlobalFilter;
        }

        // Ajusta a página quando Nodes/Rows/filtro mudam: evita "No results found" numa página que deixou de existir.
        var maxFirst = Math.Max(0, (PageCount - 1) * Math.Max(1, _rows));
        if (_first > maxFirst) _first = maxFirst;
        if (_first < 0) _first = 0;
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender && Lazy)
        {
            await RaiseLazyLoadAsync();
        }

        // Teclado sobre as linhas visíveis (roving tabindex).
        await Interop.InvokeVoidAsync(OpInterop.TreeInterop, "initTreeTableKeyboard", _tableRoot);
        await Interop.InvokeVoidAsync(OpInterop.TreeInterop, "refreshTreeTableTabstop", _tableRoot);
    }
}

internal sealed record OpTreeTableFlatNode(OpTreeNode Node, int Level);

public sealed record OpTreeTableCellContext(OpTreeNode Node, int Level, string Field);
