using System.Collections.Generic;

namespace OpBlazorUI.Base.Models;

public sealed class OpToastMessage
{
    public string? Key { get; set; }
    public string Severity { get; set; } = "info";
    public string? Summary { get; set; }
    public string? Detail { get; set; }
    public int? Life { get; set; }
    public bool Sticky { get; set; }
    public string? StyleClass { get; set; }
    public bool Closable { get; set; } = true;
    public string? Icon { get; set; }
    public object? Data { get; set; }

    public string? EffectiveIcon(string? defaultIcon = null) => Icon ?? defaultIcon;
}