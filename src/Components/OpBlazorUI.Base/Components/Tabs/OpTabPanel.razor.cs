using Microsoft.AspNetCore.Components;

namespace OpBlazorUI.Base.Components.Tabs;

/// <summary>
/// Painel de abas. Não renderiza markup próprio: o host (<see cref="OpTabs"/> ou
/// <see cref="OpTabView"/>) coleta o cabeçalho e o conteúdo deste painel.
/// </summary>
public partial class OpTabPanel : ComponentBase, IDisposable
{
    [CascadingParameter] internal IOpTabHost? Host { get; set; }

    [Parameter] public string? Header { get; set; }
    [Parameter] public string? Icon { get; set; }
    [Parameter] public bool Disabled { get; set; }
    [Parameter] public bool Closable { get; set; }
    [Parameter] public RenderFragment? HeaderTemplate { get; set; }
    [Parameter] public RenderFragment? ChildContent { get; set; }

    internal int Index { get; set; }

    protected override void OnInitialized() => Host?.Register(this);

    public void Dispose() => Host?.Unregister(this);
}
