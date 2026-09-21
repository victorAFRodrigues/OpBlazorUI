using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace OpBlazorUI.Base.Components.Editor;

public partial class OpEditor : ComponentBase, IAsyncDisposable
{
    private string _id = "";
    private ElementReference _contentRef;
    private ElementReference _toolbarRef;
    private IJSObjectReference? _module;
    private DotNetObjectReference<OpEditor>? _selfRef;
    private bool _initialized;
    private bool _appliedReadOnly;
    private string? _lastValue;

    [Inject] private IJSRuntime Js { get; set; } = default!;

    // ---------------------------------------------------------------- params
    [Parameter] public string? Value { get; set; }
    [Parameter] public EventCallback<string?> ValueChanged { get; set; }
    [Parameter] public string? Placeholder { get; set; }
    [Parameter] public bool Readonly { get; set; }
    [Parameter] public bool Disabled { get; set; }
    [Parameter] public bool Invalid { get; set; }
    [Parameter] public string? Style { get; set; }
    [Parameter] public string? StyleClass { get; set; }
    [Parameter] public IReadOnlyList<string>? Formats { get; set; }

    // events
    [Parameter] public EventCallback<string?> OnChange { get; set; }
    [Parameter] public EventCallback<string?> OnTextChange { get; set; }
    [Parameter] public EventCallback OnFocus { get; set; }
    [Parameter] public EventCallback OnBlur { get; set; }

    // templates
    [Parameter] public RenderFragment? HeaderTemplate { get; set; }

    [Parameter(CaptureUnmatchedValues = true)]
    public Dictionary<string, object>? AdditionalAttributes { get; set; }

    // ------------------------------------------------------------ lifecycle
    protected override void OnInitialized()
    {
        _id = $"op-editor-{Guid.NewGuid():N}";
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            await InitAsync();
        }
    }

    protected override async Task OnParametersSetAsync()
    {
        if (!_initialized || _module is null) return;

        if (!string.Equals(Value, _lastValue, StringComparison.Ordinal))
        {
            _lastValue = Value;
            await _module.InvokeVoidAsync("setHtml", _id, Value);
        }

        var readOnly = Readonly || Disabled;
        if (readOnly != _appliedReadOnly)
        {
            _appliedReadOnly = readOnly;
            await _module.InvokeVoidAsync("setReadOnly", _id, readOnly);
        }
    }

    // ------------------------------------------------------------ computed
    private string RootClass => BuildClass(
        "p-editor p-component",
        Disabled ? "p-disabled" : null,
        Invalid ? "p-invalid" : null,
        StyleClass);

    // ------------------------------------------------------------ interop
    private async Task InitAsync()
    {
        _module = await Js.InvokeAsync<IJSObjectReference>("import",
            "./_content/OpBlazorUI.Base/editor.interop.js");
        _selfRef = DotNetObjectReference.Create(this);

        var config = new Dictionary<string, object?>
        {
            ["placeholder"] = Placeholder
        };
        if (Formats is { Count: > 0 })
        {
            config["formats"] = Formats;
        }

        await _module.InvokeVoidAsync("init", _selfRef, _id, _contentRef, _toolbarRef, config);
        _initialized = true;
        _appliedReadOnly = Readonly || Disabled;
        _lastValue = Value;

        if (_appliedReadOnly)
        {
            await _module.InvokeVoidAsync("setReadOnly", _id, true);
        }

        if (!string.IsNullOrEmpty(Value))
        {
            await _module.InvokeVoidAsync("setHtml", _id, Value);
        }
    }

    [JSInvokable]
    public async Task NotifyTextChange(string html)
    {
        _lastValue = html;
        Value = html;
        await ValueChanged.InvokeAsync(html);
        await OnChange.InvokeAsync(html);
        await OnTextChange.InvokeAsync(html);
    }

    [JSInvokable]
    public async Task NotifyFocus()
    {
        await OnFocus.InvokeAsync();
    }

    [JSInvokable]
    public async Task NotifyBlur()
    {
        await OnBlur.InvokeAsync();
    }

    // ------------------------------------------------------------ dispose
    public async ValueTask DisposeAsync()
    {
        if (_initialized && _module is not null)
        {
            try
            {
                await _module.InvokeVoidAsync("destroy", _id);
            }
            catch
            {
                // ignore
            }
        }

        _selfRef?.Dispose();
    }

    private static string BuildClass(params string?[] classes)
        => string.Join(' ', classes.Where(c => !string.IsNullOrWhiteSpace(c)));
}
