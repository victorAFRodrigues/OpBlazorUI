namespace OpBlazorUI.Base.Components.Tabs;

/// <summary>Contrato interno compartilhado por <c>OpTabs</c> e <c>OpTabView</c> para registrar painéis.</summary>
public interface IOpTabHost
{
    void Register(OpTabPanel panel);
    void Unregister(OpTabPanel panel);
}
