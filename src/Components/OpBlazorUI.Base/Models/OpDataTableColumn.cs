using Microsoft.AspNetCore.Components;

namespace OpBlazorUI.Base.Models;

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

    /// <summary>Largura aplicada como <c>style="width:..."</c> no <c>th</c>.</summary>
    public string? Width { get; set; }

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
}
