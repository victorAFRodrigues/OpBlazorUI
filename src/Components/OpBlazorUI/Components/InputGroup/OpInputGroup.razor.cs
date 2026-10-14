using Microsoft.AspNetCore.Components;

namespace OpBlazorUI.Components.InputGroup;

public partial class OpInputGroup : ComponentBase
{
    [Parameter] public string? StyleClass { get; set; }

    [Parameter] public RenderFragment? ChildContent { get; set; }

    [Parameter(CaptureUnmatchedValues = true)]
    public Dictionary<string, object>? AdditionalAttributes { get; set; }

    private string RootClass => BuildClass("p-inputgroup", StyleClass);

    private static string BuildClass(params string?[] classes)
        => string.Join(' ', classes.Where(c => !string.IsNullOrWhiteSpace(c)));
}
