namespace OpBlazorUI.Base.Models;

public sealed class OpMenuItem
{
    public string? Label { get; set; }
    public string? Icon { get; set; }
    public string? Url { get; set; }
    public string? Target { get; set; }
    public Action? Command { get; set; }
    public List<OpMenuItem>? Items { get; set; }
    public bool Disabled { get; set; }
    public bool Separator { get; set; }
    public bool Visible { get; set; } = true;
    public string? StyleClass { get; set; }

    public bool HasChildren => Items is { Count: > 0 };
}

public sealed class OpMenuGroup
{
    public string? Label { get; set; }
    public List<OpMenuItem> Items { get; set; } = new();
}