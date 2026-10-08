using Microsoft.AspNetCore.Components;
using OpBlazorUI.Base.Components.Common;

namespace OpBlazorUI.Base.Components.OrderList;

public partial class OpOrderList<TItem> : OpComponentBase
{
    private List<TItem> _items = new();
    private List<TItem> _selection = new();
    private IReadOnlyList<TItem>? _lastItems;
    private IReadOnlyList<TItem>? _lastParameterSelection;
    private string _listId = string.Empty;

    [Parameter] public IReadOnlyList<TItem> Items { get; set; } = Array.Empty<TItem>();
    [Parameter] public EventCallback<IReadOnlyList<TItem>> ItemsChanged { get; set; }

    [Parameter] public IReadOnlyList<TItem>? SelectedValues { get; set; }
    [Parameter] public EventCallback<IReadOnlyList<TItem>> SelectedValuesChanged { get; set; }

    [Parameter] public string? OptionLabel { get; set; }
    [Parameter] public string? Header { get; set; }
    [Parameter] public bool Filter { get; set; }
    [Parameter] public string? FilterBy { get; set; }
    [Parameter] public string? ScrollHeight { get; set; } = "14rem";
    [Parameter] public string? EmptyMessage { get; set; } = "No results found";
    [Parameter] public bool Disabled { get; set; }
    [Parameter] public string? AriaLabel { get; set; }
    [Parameter] public RenderFragment<TItem>? ItemTemplate { get; set; }

    private bool MoveDisabled => _selection.Count == 0;

    private string RootClass => Class("p-orderlist", StyleClass);

    protected override void OnInitialized()
    {
        _listId = string.IsNullOrEmpty(Id) ? $"op-orderlist-{Guid.NewGuid():N}" : Id!;
    }

    protected override void OnParametersSet()
    {
        if (!ReferenceEquals(_lastItems, Items))
        {
            _lastItems = Items;
            _items = Items is null ? new List<TItem>() : new List<TItem>(Items);
        }

        if (!ReferenceEquals(_lastParameterSelection, SelectedValues))
        {
            _lastParameterSelection = SelectedValues;
            _selection = SelectedValues is null ? new List<TItem>() : new List<TItem>(SelectedValues);
        }
    }

    private async Task OnSelectionChanged(IReadOnlyList<TItem>? values)
    {
        _selection = values is null ? new List<TItem>() : new List<TItem>(values);
        await SelectedValuesChanged.InvokeAsync(_selection);
    }

    private Task MoveUp() => ApplyReorder("up");

    private Task MoveTop() => ApplyReorder("top");

    private Task MoveDown() => ApplyReorder("down");

    private Task MoveBottom() => ApplyReorder("bottom");

    private async Task ApplyReorder(string direction)
    {
        if (_selection.Count == 0)
        {
            return;
        }

        Reorder(_items, _selection, direction);
        await ItemsChanged.InvokeAsync(_items.ToList());
    }

    private static void Reorder(List<TItem> list, List<TItem> selection, string direction)
    {
        if (selection.Count == 0 || list.Count == 0)
        {
            return;
        }

        var selected = new List<TItem>();
        foreach (var item in list)
        {
            if (IsSelected(selection, item))
            {
                selected.Add(item);
            }
        }

        if (selected.Count == 0)
        {
            return;
        }

        switch (direction)
        {
            case "up":
                foreach (var item in selected)
                {
                    var index = IndexOf(list, item);
                    if (index > 0 && !IsSelected(selection, list[index - 1]))
                    {
                        (list[index - 1], list[index]) = (list[index], list[index - 1]);
                    }
                }
                break;

            case "top":
                list.RemoveAll(item => IsSelected(selection, item));
                list.InsertRange(0, selected);
                break;

            case "down":
                for (var i = selected.Count - 1; i >= 0; i--)
                {
                    var index = IndexOf(list, selected[i]);
                    if (index >= 0 && index < list.Count - 1 && !IsSelected(selection, list[index + 1]))
                    {
                        (list[index + 1], list[index]) = (list[index], list[index + 1]);
                    }
                }
                break;

            case "bottom":
                list.RemoveAll(item => IsSelected(selection, item));
                list.AddRange(selected);
                break;
        }
    }

    private static int IndexOf(IReadOnlyList<TItem> list, TItem item)
    {
        for (var i = 0; i < list.Count; i++)
        {
            if (ReferenceEquals(list[i], item) || Equals(list[i], item))
            {
                return i;
            }
        }

        return -1;
    }

    private static bool IsSelected(IReadOnlyList<TItem> selection, TItem item)
    {
        for (var i = 0; i < selection.Count; i++)
        {
            if (ReferenceEquals(selection[i], item) || Equals(selection[i], item))
            {
                return true;
            }
        }

        return false;
    }
}
