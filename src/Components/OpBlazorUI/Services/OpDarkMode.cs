namespace OpBlazorUI.Services;

/// <summary>Modo inicial do tema, configurado em <c>AddOpBlazorUI(o =&gt; ...)</c>.</summary>
public enum OpDarkMode
{
    /// <summary>Sempre claro. É o padrão da biblioteca.</summary>
    Light,

    /// <summary>Sempre escuro.</summary>
    Dark,

    /// <summary>Segue o <c>prefers-color-scheme</c> do sistema operacional.</summary>
    System
}
