using Microsoft.AspNetCore.Components;
using OpBlazorUI.Components.TreeTable;
using OpBlazorUI.Services;

namespace OpBlazorUI.Models;

/// <summary>
/// Definição de coluna do <c>OpTreeTable</c>, com o mesmo conjunto de recursos do
/// <see cref="OpDataTableColumn"/> (ordenação, filtro, congelamento e templates).
/// </summary>
public sealed class OpTreeTableColumn
{
    public string Field { get; set; } = "";
    public string Header { get; set; } = "";
    public string? Width { get; set; }
    public bool Sortable { get; set; }
    public string? StyleClass { get; set; }
    public RenderFragment<OpTreeTableCellContext>? BodyTemplate { get; set; }

    /// <summary>Coluna que contém o botão de expandir/recolher (padrão: a primeira).</summary>
    public bool Expandable { get; set; }

    /// <summary>
    /// Congela a coluna na borda do contêiner durante a rolagem horizontal: <c>left</c> ou <c>right</c>.
    /// O upstream usa <c>frozenColumns</c>/<c>frozenWidth</c> com uma view separada; aqui o congelamento é
    /// por coluna (sticky), como no <c>OpDataTable</c>. Use larguras em <c>px</c> (o deslocamento é somado a partir delas).
    /// </summary>
    public string? Frozen { get; set; }

    /// <summary>Classes extras apenas no <c>th</c>.</summary>
    public string? HeaderStyleClass { get; set; }

    /// <summary>Classes extras apenas nos <c>td</c>.</summary>
    public string? BodyStyleClass { get; set; }

    /// <summary>Texto do rodapé desta coluna (renderiza um <c>tfoot</c> com todas as colunas).</summary>
    public string? Footer { get; set; }

    /// <summary>Classes extras no <c>td</c> do rodapé.</summary>
    public string? FooterStyleClass { get; set; }

    /// <summary>Exibe um campo de filtro no cabeçalho desta coluna.</summary>
    public bool Filter { get; set; }

    /// <summary>Modo de comparação do filtro da coluna (padrão <see cref="OpFilterMatchMode.Contains"/>).</summary>
    public OpFilterMatchMode FilterMatchMode { get; set; } = OpFilterMatchMode.Contains;

    /// <summary>Placeholder (e rótulo ARIA) do campo de filtro da coluna.</summary>
    public string? FilterPlaceholder { get; set; }

    /// <summary>Classes extras no contêiner do filtro da coluna.</summary>
    public string? FilterStyleClass { get; set; }

    /// <summary>Template do conteúdo do cabeçalho (substitui <see cref="Header"/>).</summary>
    public RenderFragment? HeaderTemplate { get; set; }

    /// <summary>Template do conteúdo do rodapé desta coluna (substitui <see cref="Footer"/>).</summary>
    public RenderFragment? FooterTemplate { get; set; }

    /// <summary>Omite o <c>th</c> desta coluna na linha comum de cabeçalho (para cabeçalhos agrupados).</summary>
    public bool HideHeader { get; set; }
}
