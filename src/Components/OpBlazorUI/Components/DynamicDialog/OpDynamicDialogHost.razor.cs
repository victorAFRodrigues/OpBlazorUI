using Microsoft.AspNetCore.Components;
using OpBlazorUI.Services;

namespace OpBlazorUI.Components.DynamicDialog;

public partial class OpDynamicDialogHost : ComponentBase, IDisposable
{
    private IReadOnlyList<OpDynamicDialogInstance> _instances = Array.Empty<OpDynamicDialogInstance>();

    [Inject] private OpDialogService DialogService { get; set; } = default!;

    protected override void OnInitialized()
    {
        DialogService.Changed += OnChanged;
        Sync();
    }

    private void OnChanged()
        => _ = InvokeAsync(() =>
        {
            Sync();
            StateHasChanged();
        });

    private void Sync() => _instances = DialogService.Instances.ToList();

    private void Close(OpDynamicDialogInstance instance, object? result)
        => DialogService.Close(instance, result);

    private static string? DialogStyle(OpDynamicDialogConfig config)
    {
        var parts = new List<string>();
        if (!string.IsNullOrWhiteSpace(config.Width))
        {
            parts.Add($"width: {config.Width}");
        }

        if (!string.IsNullOrWhiteSpace(config.Style))
        {
            parts.Add(config.Style!);
        }

        return parts.Count > 0 ? string.Join("; ", parts) : null;
    }

    public void Dispose() => DialogService.Changed -= OnChanged;
}
