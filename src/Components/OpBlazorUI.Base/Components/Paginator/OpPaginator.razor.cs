using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;

namespace OpBlazorUI.Base.Components.Paginator;

public sealed record OpPaginatorState(int First, int Rows, int Page, int PageCount);

public partial class OpPaginator : ComponentBase
{
    private string _jumpToPageText = "";

    [Parameter] public int First { get; set; }

    [Parameter] public int Rows { get; set; }

    [Parameter] public int TotalRecords { get; set; }

    [Parameter] public IReadOnlyList<int>? RowsPerPageOptions { get; set; }

    [Parameter] public int PageLinkSize { get; set; } = 5;

    [Parameter] public bool AlwaysShow { get; set; } = true;

    [Parameter] public bool ShowFirstLastIcon { get; set; } = true;

    [Parameter] public bool ShowPageLinks { get; set; } = true;

    [Parameter] public bool ShowCurrentPageReport { get; set; }

    [Parameter] public string CurrentPageReportTemplate { get; set; } = "{currentPage} of {totalPages}";

    [Parameter] public bool ShowJumpToPageInput { get; set; }

    [Parameter] public string? StyleClass { get; set; }

    [Parameter] public EventCallback<OpPaginatorState> OnPageChange { get; set; }

    [Parameter] public RenderFragment? TemplateLeft { get; set; }

    [Parameter] public RenderFragment? TemplateRight { get; set; }

    [Parameter] public RenderFragment? FirstPageLinkIconTemplate { get; set; }

    [Parameter] public RenderFragment? PreviousPageLinkIconTemplate { get; set; }

    [Parameter] public RenderFragment? NextPageLinkIconTemplate { get; set; }

    [Parameter] public RenderFragment? LastPageLinkIconTemplate { get; set; }

    [Parameter(CaptureUnmatchedValues = true)]
    public Dictionary<string, object>? AdditionalAttributes { get; set; }

    private int PageCount => TotalRecords > 0 && Rows > 0
        ? (int)Math.Ceiling((double)TotalRecords / Rows)
        : 0;

    private int CurrentPage => TotalRecords > 0 && Rows > 0 ? First / Rows : 0;

    private bool IsFirstPageDisabled => CurrentPage <= 0;

    private bool IsLastPageDisabled => CurrentPage >= PageCount - 1;

    private int[] PageLinks
    {
        get
        {
            if (PageCount <= 0) return [];
            var half = PageLinkSize / 2;
            var start = Math.Max(0, CurrentPage - half);
            var end = Math.Min(PageCount, start + PageLinkSize);
            if (end - start < PageLinkSize)
            {
                start = Math.Max(0, end - PageLinkSize);
            }

            var links = new List<int>();
            for (var i = start; i < end; i++)
            {
                links.Add(i);
            }

            return links.ToArray();
        }
    }

    private string CurrentPageReport => CurrentPageReportTemplate
        .Replace("{currentPage}", (CurrentPage + 1).ToString())
        .Replace("{totalPages}", PageCount.ToString())
        .Replace("{rows}", Rows.ToString())
        .Replace("{first}", (First + 1).ToString())
        .Replace("{last}", Math.Min(First + Rows, TotalRecords).ToString())
        .Replace("{totalRecords}", TotalRecords.ToString());

    private string RootClass => OpCss.BuildClass("p-paginator p-component", StyleClass);

    private string PageClass(int page) => OpCss.BuildClass(
        "p-paginator-page",
        page == CurrentPage ? "p-paginator-page-selected" : null);

    private async Task ChangeToFirstPage() => await NavigateAsync(0);

    private async Task ChangeToPrevPage() => await NavigateAsync(Math.Max(0, First - Rows));

    private async Task ChangeToNextPage() => await NavigateAsync(Math.Min((PageCount - 1) * Rows, First + Rows));

    private async Task ChangeToLastPage() => await NavigateAsync((PageCount - 1) * Rows);

    private async Task ChangeToPage(int page) => await NavigateAsync(page * Rows);

    private async Task NavigateAsync(int first)
    {
        await OnPageChange.InvokeAsync(new OpPaginatorState(first, Rows, first / Rows, PageCount));
    }

    private async Task OnRowsPerPageChange(ChangeEventArgs e)
    {
        if (int.TryParse(e.Value?.ToString(), out var rows) && rows > 0)
        {
            await OnPageChange.InvokeAsync(new OpPaginatorState(0, rows, 0, (int)Math.Ceiling((double)TotalRecords / rows)));
        }
    }

    private void OnJumpToPageInput(ChangeEventArgs e)
    {
        _jumpToPageText = e.Value?.ToString() ?? string.Empty;
    }

    private async Task OnJumpToPageKeydown(KeyboardEventArgs e)
    {
        if (e.Key == "Enter" && int.TryParse(_jumpToPageText, out var page) && page >= 1)
        {
            var target = Math.Min(PageCount - 1, page - 1);
            await NavigateAsync(target * Rows);
        }
    }
}
