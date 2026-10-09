using Microsoft.AspNetCore.Components;
using OpBlazorUI.Base.Components.Common;

namespace OpBlazorUI.Base.Components.Splitter;

public partial class OpSplitterPanel : OpComponentBase, IDisposable
{
    [CascadingParameter] private OpSplitter? Splitter { get; set; }

    [Parameter] public RenderFragment? ChildContent { get; set; }
    [Parameter] public RenderFragment? Template { get; set; }

    internal int Index { get; set; }

    private string PanelClass => Class("p-splitterpanel", Splitter?.PanelStyleClass, StyleClass);

    protected override void OnInitialized() => Splitter?.Register(this);

    public void Dispose() => Splitter?.Unregister(this);
}
