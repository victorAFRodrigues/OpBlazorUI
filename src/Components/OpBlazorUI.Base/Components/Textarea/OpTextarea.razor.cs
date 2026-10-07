using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;
using OpBlazorUI.Base.Components.Forms;

namespace OpBlazorUI.Base.Components.Textarea;

public partial class OpTextarea : OpInputBase<string>
{
    private ElementReference _element;

    [Parameter] public string? Placeholder { get; set; }
    [Parameter] public string Variant { get; set; } = "outlined";
    [Parameter] public string? Size { get; set; }
    [Parameter] public bool Fluid { get; set; }
    [Parameter] public bool Readonly { get; set; }
    [Parameter] public bool AutoResize { get; set; }
    [Parameter] public string? Id { get; set; }
    [Parameter] public string? Name { get; set; }
    [Parameter] public string? AriaLabel { get; set; }
    [Parameter] public int? MaxLength { get; set; }
    [Parameter] public string? Autocomplete { get; set; }

    [Parameter] public EventCallback<string?> OnInput { get; set; }
    [Parameter] public EventCallback<string?> OnChange { get; set; }
    [Parameter] public EventCallback<FocusEventArgs> OnFocus { get; set; }
    [Parameter] public EventCallback<FocusEventArgs> OnBlur { get; set; }

    private string RootClass => OpCss.BuildClass(
        "p-textarea p-component",
        !string.IsNullOrEmpty(CurrentValue) ? "p-filled" : null,
        Size == "small" ? "p-textarea-sm" : null,
        Size == "large" ? "p-textarea-lg" : null,
        Variant == "filled" ? "p-variant-filled" : null,
        Fluid ? "p-textarea-fluid" : null,
        AutoResize ? "p-textarea-resizable" : null,
        IsInvalid ? "p-invalid" : null,
        StyleClass);

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (AutoResize)
        {
            await AdjustHeightAsync();
        }
    }

    private async Task HandleInput(ChangeEventArgs e)
    {
        CurrentValue = e.Value?.ToString() ?? string.Empty;
        await OnInput.InvokeAsync(CurrentValue);

        if (AutoResize)
        {
            await AdjustHeightAsync();
        }
    }

    private async Task HandleChange(ChangeEventArgs e)
    {
        await OnChange.InvokeAsync(e.Value?.ToString());
    }

    private async Task HandleBlur(FocusEventArgs e)
    {
        await OnBlur.InvokeAsync(e);
    }

    private async ValueTask AdjustHeightAsync()
    {
        try
        {
            await Interop.InvokeVoidAsync(OpInterop.TextareaInterop, "adjustHeight", _element);
        }
        catch (JSDisconnectedException)
        {
            // componente descartado durante a chamada
        }
    }
}
