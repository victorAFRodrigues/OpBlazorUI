using Microsoft.AspNetCore.Components;
using OpBlazorUI.Base.Components.Common;

namespace OpBlazorUI.Base.Components.PickList;

public partial class OpPickList<TItem> : OpComponentBase
{
    private List<TItem> _source = new();
    private List<TItem> _target = new();
    private List<TItem> _selectedSource = new();
    private List<TItem> _selectedTarget = new();

    private IReadOnlyList<TItem>? _lastSource;
    private IReadOnlyList<TItem>? _lastTarget;
    private IReadOnlyList<TItem>? _lastParameterSelectedSource;
    private IReadOnlyList<TItem>? _lastParameterSelectedTarget;

    private string _sourceId = string.Empty;
    private string _targetId = string.Empty;

    [Parameter] public IReadOnlyList<TItem> Source { get; set; } = Array.Empty<TItem>();
    [Parameter] public IReadOnlyList<TItem> Target { get; set; } = Array.Empty<TItem>();
    [Parameter] public EventCallback<IReadOnlyList<TItem>> SourceChanged { get; set; }
    [Parameter] public EventCallback<IReadOnlyList<TItem>> TargetChanged { get; set; }

    [Parameter] public IReadOnlyList<TItem>? SelectedSource { get; set; }
    [Parameter] public EventCallback<IReadOnlyList<TItem>> SelectedSourceChanged { get; set; }
    [Parameter] public IReadOnlyList<TItem>? SelectedTarget { get; set; }
    [Parameter] public EventCallback<IReadOnlyList<TItem>> SelectedTargetChanged { get; set; }

    [Parameter] public string? OptionLabel { get; set; }
    [Parameter] public bool Filter { get; set; }
    [Parameter] public bool FilterSource { get; set; } = true;
    [Parameter] public bool FilterTarget { get; set; } = true;
    [Parameter] public string? FilterBy { get; set; }
    [Parameter] public bool ShowSourceControls { get; set; } = true;
    [Parameter] public bool ShowTargetControls { get; set; } = true;
    [Parameter] public string? ScrollHeight { get; set; } = "14rem";
    [Parameter] public string? EmptyMessage { get; set; } = "No results found";
    [Parameter] public bool Disabled { get; set; }
    [Parameter] public string? AriaLabel { get; set; }
    [Parameter] public RenderFragment<TItem>? ItemTemplate { get; set; }
    [Parameter] public string? SourceHeader { get; set; }
    [Parameter] public string? TargetHeader { get; set; }

    private bool SourceFilterEnabled => Filter && FilterSource;
    private bool TargetFilterEnabled => Filter && FilterTarget;

    private bool SourceMoveDisabled => _selectedSource.Count == 0;
    private bool TargetMoveDisabled => _selectedTarget.Count == 0;
    private bool MoveRightDisabled => _selectedSource.Count == 0;
    private bool MoveLeftDisabled => _selectedTarget.Count == 0;
    private bool MoveAllRightDisabled => _source.Count == 0;
    private bool MoveAllLeftDisabled => _target.Count == 0;

    private string RootClass => Class("p-picklist", StyleClass);

    protected override void OnInitialized()
    {
        var baseId = string.IsNullOrEmpty(Id) ? $"op-picklist-{Guid.NewGuid():N}" : Id!;
        _sourceId = $"{baseId}_source";
        _targetId = $"{baseId}_target";
    }

    protected override void OnParametersSet()
    {
        if (!ReferenceEquals(_lastSource, Source))
        {
            _lastSource = Source;
            _source = Source is null ? new List<TItem>() : new List<TItem>(Source);
        }

        if (!ReferenceEquals(_lastTarget, Target))
        {
            _lastTarget = Target;
            _target = Target is null ? new List<TItem>() : new List<TItem>(Target);
        }

        if (!ReferenceEquals(_lastParameterSelectedSource, SelectedSource))
        {
            _lastParameterSelectedSource = SelectedSource;
            _selectedSource = SelectedSource is null ? new List<TItem>() : new List<TItem>(SelectedSource);
        }

        if (!ReferenceEquals(_lastParameterSelectedTarget, SelectedTarget))
        {
            _lastParameterSelectedTarget = SelectedTarget;
            _selectedTarget = SelectedTarget is null ? new List<TItem>() : new List<TItem>(SelectedTarget);
        }
    }

    private async Task OnSourceSelectionChanged(IReadOnlyList<TItem>? values)
    {
        _selectedSource = values is null ? new List<TItem>() : new List<TItem>(values);
        await SelectedSourceChanged.InvokeAsync(_selectedSource);
    }

    private async Task OnTargetSelectionChanged(IReadOnlyList<TItem>? values)
    {
        _selectedTarget = values is null ? new List<TItem>() : new List<TItem>(values);
        await SelectedTargetChanged.InvokeAsync(_selectedTarget);
    }

    private Task MoveItemUp(bool source) => ApplyReorder(source, "up");

    private Task MoveItemTop(bool source) => ApplyReorder(source, "top");

    private Task MoveItemDown(bool source) => ApplyReorder(source, "down");

    private Task MoveItemBottom(bool source) => ApplyReorder(source, "bottom");

    private async Task ApplyReorder(bool source, string direction)
    {
        var list = source ? _source : _target;
        var selection = source ? _selectedSource : _selectedTarget;

        if (selection.Count == 0)
        {
            return;
        }

        Reorder(list, selection, direction);
        await NotifyListChanged(source);
    }

    private async Task MoveItemToTarget(TItem? item)
    {
        if (Disabled || item is null || IndexOf(_target, item) >= 0)
        {
            return;
        }

        var index = IndexOf(_source, item);
        if (index < 0)
        {
            return;
        }

        _source.RemoveAt(index);
        _target.Add(item);
        _selectedSource.RemoveAll(v => Equals(v, item));
        await NotifyListChanged(true);
        await NotifyListChanged(false);
    }

    private async Task MoveItemToSource(TItem? item)
    {
        if (Disabled || item is null || IndexOf(_source, item) >= 0)
        {
            return;
        }

        var index = IndexOf(_target, item);
        if (index < 0)
        {
            return;
        }

        _target.RemoveAt(index);
        _source.Add(item);
        _selectedTarget.RemoveAll(v => Equals(v, item));
        await NotifyListChanged(true);
        await NotifyListChanged(false);
    }

    private async Task MoveRight()
    {
        if (_selectedSource.Count == 0)
        {
            return;
        }

        foreach (var item in _selectedSource.ToList())
        {
            if (IndexOf(_target, item) >= 0)
            {
                continue;
            }

            var index = IndexOf(_source, item);
            if (index >= 0)
            {
                _source.RemoveAt(index);
                _target.Add(item);
            }
        }

        _selectedSource.Clear();
        await NotifyListChanged(true);
        await NotifyListChanged(false);
    }

    private async Task MoveAllRight()
    {
        if (_source.Count == 0)
        {
            return;
        }

        foreach (var item in _source.ToList())
        {
            if (IndexOf(_target, item) < 0)
            {
                _target.Add(item);
            }
        }

        _source.Clear();
        _selectedSource.Clear();
        await NotifyListChanged(true);
        await NotifyListChanged(false);
    }

    private async Task MoveLeft()
    {
        if (_selectedTarget.Count == 0)
        {
            return;
        }

        foreach (var item in _selectedTarget.ToList())
        {
            if (IndexOf(_source, item) >= 0)
            {
                continue;
            }

            var index = IndexOf(_target, item);
            if (index >= 0)
            {
                _target.RemoveAt(index);
                _source.Add(item);
            }
        }

        _selectedTarget.Clear();
        await NotifyListChanged(true);
        await NotifyListChanged(false);
    }

    private async Task MoveAllLeft()
    {
        if (_target.Count == 0)
        {
            return;
        }

        foreach (var item in _target.ToList())
        {
            if (IndexOf(_source, item) < 0)
            {
                _source.Add(item);
            }
        }

        _target.Clear();
        _selectedTarget.Clear();
        await NotifyListChanged(true);
        await NotifyListChanged(false);
    }

    private Task NotifyListChanged(bool source) =>
        source ? SourceChanged.InvokeAsync(_source.ToList()) : TargetChanged.InvokeAsync(_target.ToList());

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
