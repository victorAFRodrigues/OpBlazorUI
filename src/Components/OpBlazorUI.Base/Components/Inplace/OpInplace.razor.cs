using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using OpBlazorUI.Base.Components.Common;

namespace OpBlazorUI.Base.Components.Inplace;

/// <summary>Contexto entregue a <see cref="OpInplace.ContentTemplate"/> com o callback de fechamento.</summary>
public sealed class OpInplaceContext
{
    public Func<Task> Close { get; init; } = () => Task.CompletedTask;
}

public partial class OpInplace : OpComponentBase
{
    [Parameter] public bool Active { get; set; }
    [Parameter] public bool Disabled { get; set; }
    [Parameter] public bool PreventClick { get; set; }
    [Parameter] public bool Closable { get; set; }
    [Parameter] public string? CloseIcon { get; set; }
    [Parameter] public string? CloseAriaLabel { get; set; }

    [Parameter] public RenderFragment? DisplayTemplate { get; set; }
    [Parameter] public RenderFragment<OpInplaceContext>? ContentTemplate { get; set; }

    [Parameter] public EventCallback<bool> ActiveChanged { get; set; }
    [Parameter] public EventCallback OnActivate { get; set; }
    [Parameter] public EventCallback OnDeactivate { get; set; }

    private string RootClass => Class("p-inplace", StyleClass);

    private string CloseIconClass => Class("pi pi-times", CloseIcon);

    private async Task ActivateAsync()
    {
        if (Disabled || PreventClick || Active)
        {
            return;
        }

        Active = true;
        await ActiveChanged.InvokeAsync(true);
        await OnActivate.InvokeAsync();
    }

    private async Task DeactivateAsync()
    {
        if (Disabled || !Active)
        {
            return;
        }

        Active = false;
        await ActiveChanged.InvokeAsync(false);
        await OnDeactivate.InvokeAsync();
    }

    private async Task HandleDisplayKeyDown(KeyboardEventArgs e)
    {
        if (e.Code == "Enter")
        {
            await ActivateAsync();
        }
    }
}
