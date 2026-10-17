using Microsoft.AspNetCore.Components;
using OpBlazorUI.Services;

namespace OpBlazorUI.Models;

/// <summary>
/// Definição de coluna do <c>OpDataTable</c>. Para templates tipados de célula
/// (<c>BodyTemplate</c>) use <see cref="OpDataTableColumn{TItem}"/>.
/// </summary>
public class OpDataTableColumn
{
    /// <summary>Nome da propriedade pública lida por reflexão (case-insensitive) quando não há template.</summary>
    public string Field { get; set; } = "";

    /// <summary>Título textual exibido no cabeçalho.</summary>
    public string Header { get; set; } = "";

    /// <summary>Se a coluna participa da ordenação (combinado com o <c>Sortable</c> global).</summary>
    public bool Sortable { get; set; } = true;

    /// <summary>
    /// Renderiza um controle de seleção nesta coluna: <c>multiple</c> (checkbox, com select-all no
    /// cabeçalho) ou <c>single</c> (radio). Coluna comum quando nulo (padrão).
    /// </summary>
    public string? SelectionMode { get; set; }

    /// <summary>Largura aplicada como <c>style="width:..."</c> no <c>th</c>. Use valores em <c>px</c>
    /// quando a coluna for congelada (o deslocamento do sticky é calculado a partir deles).</summary>
    public string? Width { get; set; }

    /// <summary>Congela a coluna na borda do contêiner: <c>left</c> ou <c>right</c>.</summary>
    public string? Frozen { get; set; }

    /// <summary>Omite o <c>th</c> desta coluna na linha de cabeçalho (para cabeçalhos agrupados).</summary>
    public bool HideHeader { get; set; }

    /// <summary>Renderiza o puxador de reordenação de linhas nesta coluna (com <c>ReorderableRows</c>).</summary>
    public bool RowReorder { get; set; }

    /// <summary>Classes extras no contêiner do cabeçalho e nas células (<c>th</c>/<c>td</c>).</summary>
    public string? StyleClass { get; set; }

    /// <summary>Classes extras apenas no <c>th</c>.</summary>
    public string? HeaderStyleClass { get; set; }

    /// <summary>Classes extras apenas nos <c>td</c>.</summary>
    public string? BodyStyleClass { get; set; }

    /// <summary>Classes extras no <c>td</c> do rodapé.</summary>
    public string? FooterStyleClass { get; set; }

    /// <summary>Texto do rodapé desta coluna (renderiza um <c>tfoot</c> com todas as colunas).</summary>
    public string? Footer { get; set; }

    /// <summary>Exibe um campo de filtro no cabeçalho desta coluna.</summary>
    public bool Filter { get; set; }

    /// <summary>Modo de comparação do filtro da coluna (padrão <see cref="OpFilterMatchMode.Contains"/>).</summary>
    public OpFilterMatchMode FilterMatchMode { get; set; } = OpFilterMatchMode.Contains;

    /// <summary>Placeholder (e rótulo ARIA) do campo de filtro da coluna.</summary>
    public string? FilterPlaceholder { get; set; }

    /// <summary>Classes extras no contêiner do filtro da coluna.</summary>
    public string? FilterStyleClass { get; set; }
}

/// <summary>Coluna com templates tipados para <typeparamref name="TItem"/>.</summary>
public sealed class OpDataTableColumn<TItem> : OpDataTableColumn
{
    /// <summary>Template do conteúdo do cabeçalho (substitui <see cref="OpDataTableColumn.Header"/>).</summary>
    public RenderFragment? HeaderTemplate { get; set; }

    /// <summary>Template do conteúdo da célula (substitui a leitura por reflexão).</summary>
    public RenderFragment<TItem>? BodyTemplate { get; set; }

    /// <summary>Template do conteúdo do rodapé (substitui <see cref="OpDataTableColumn.Footer"/>).</summary>
    public RenderFragment? FooterTemplate { get; set; }

    /// <summary>Template de edição da célula (usado com <c>EditMode</c> na tabela).</summary>
    public RenderFragment<TItem>? EditorTemplate { get; set; }
}
