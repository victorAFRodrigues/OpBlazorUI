using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using OpBlazorUI.Base.Components.Common;

namespace OpBlazorUI.Base.Components.Editor;

public partial class OpEditor : OpComponentBase
{
    private string _id = "";
    private ElementReference _contentRef;
    private ElementReference _toolbarRef;
    private DotNetObjectReference<OpEditor>? _selfRef;
    private bool _initialized;
    private bool _loadFailed;
    private bool _appliedReadOnly;
    private string? _lastValue;

    // ---------------------------------------------------------------- params
    [Parameter] public string? Value { get; set; }
    [Parameter] public EventCallback<string?> ValueChanged { get; set; }
    [Parameter] public string? Placeholder { get; set; }
    [Parameter] public bool Readonly { get; set; }
    [Parameter] public bool Disabled { get; set; }
    [Parameter] public bool Invalid { get; set; }
    [Parameter] public string? Style { get; set; }
    [Parameter] public IReadOnlyList<string>? Formats { get; set; }

    // events
    [Parameter] public EventCallback<string?> OnChange { get; set; }
    [Parameter] public EventCallback<string?> OnTextChange { get; set; }
    [Parameter] public EventCallback OnFocus { get; set; }
    [Parameter] public EventCallback OnBlur { get; set; }

    // templates
    [Parameter] public RenderFragment? HeaderTemplate { get; set; }

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
        if (!_initialized) return;

        if (!string.Equals(Value, _lastValue, StringComparison.Ordinal))
        {
            _lastValue = Value;
            await Interop.InvokeVoidAsync(OpInterop.EditorInterop, "setHtml", _id, Value);
        }

        var readOnly = Readonly || Disabled;
        if (readOnly != _appliedReadOnly)
        {
            _appliedReadOnly = readOnly;
            await Interop.InvokeVoidAsync(OpInterop.EditorInterop, "setReadOnly", _id, readOnly);
        }
    }

    // ------------------------------------------------------------ computed
    private string RootClass => Class(
        "p-editor p-component",
        Disabled ? "p-disabled" : null,
        Invalid ? "p-invalid" : null,
        StyleClass);

    // ------------------------------------------------------------ interop
    private async Task InitAsync()
    {
        _selfRef = DotNetObjectReference.Create(this);

        var config = new Dictionary<string, object?>
        {
            ["placeholder"] = Placeholder
        };
        if (Formats is { Count: > 0 })
        {
            config["formats"] = Formats;
        }

        // false: Quill ausente (erro já registrado no log pelo OpInterop) ou init abortado
        // porque o componente foi descartado enquanto aguardava o Quill.
        var ok = await Interop.InvokeAsync<bool>(OpInterop.EditorInterop, "init", _selfRef, _id, _contentRef, _toolbarRef, config);
        if (!ok)
        {
            if (!Interop.IsDisposed)
            {
                _loadFailed = true;
                StateHasChanged();
            }

            return;
        }

        _initialized = true;
        _appliedReadOnly = Readonly || Disabled;
        _lastValue = Value;

        if (_appliedReadOnly)
        {
            await Interop.InvokeVoidAsync(OpInterop.EditorInterop, "setReadOnly", _id, true);
        }

        if (!string.IsNullOrEmpty(Value))
        {
            await Interop.InvokeVoidAsync(OpInterop.EditorInterop, "setHtml", _id, Value);
        }
    }

    [JSInvokable]
    public async Task NotifyTextChange(string html)
    {
        var value = NormalizeHtml(html);
        _lastValue = value;
        Value = value;
        await ValueChanged.InvokeAsync(value);
        await OnChange.InvokeAsync(value);
        await OnTextChange.InvokeAsync(value);
    }

    // O editor envia markup "vazio" (ex.: <p><br></p>); normaliza para null como no upstream.
    private static readonly Regex EmptyHtml = new(
        @"^\s*(<p>\s*(<br\s*/?>\s*)*</p>|<br\s*/?>|&nbsp;|\s)*\s*$",
        RegexOptions.IgnoreCase | RegexOptions.Compiled);

    private static string? NormalizeHtml(string? html) =>
        string.IsNullOrEmpty(html) || EmptyHtml.IsMatch(html) ? null : html;

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
    public override async ValueTask DisposeAsync()
    {
        // Também sem init concluído: o destroy marca o id e um init pendente é abortado no JS.
        if (_selfRef is not null)
        {
            await Interop.InvokeVoidAsync(OpInterop.EditorInterop, "destroy", _id);
        }

        _selfRef?.Dispose();
        await base.DisposeAsync();
    }
}
