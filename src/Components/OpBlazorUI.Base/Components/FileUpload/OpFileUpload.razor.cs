using System.Globalization;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Components.Web;
using OpBlazorUI.Base.Components.Common;

namespace OpBlazorUI.Base.Components.FileUpload;

public partial class OpFileUpload : OpComponentBase
{
    private readonly List<OpFileUploadFile> _files = new();
    private readonly List<OpFileUploadFile> _uploadedFiles = new();
    private readonly List<(string Severity, string Text)> _messages = new();
    private int _progress;
    private bool _dropHighlight;

    [Parameter] public string Mode { get; set; } = "advanced";
    [Parameter] public string? Name { get; set; }
    [Parameter] public string? Url { get; set; }
    [Parameter] public string Method { get; set; } = "post";
    [Parameter] public bool Multiple { get; set; }
    [Parameter] public bool Disabled { get; set; }
    [Parameter] public string? Accept { get; set; }
    [Parameter] public bool Auto { get; set; }
    [Parameter] public bool CustomUpload { get; set; }
    [Parameter] public long? MaxFileSize { get; set; }
    [Parameter] public int? FileLimit { get; set; }

    [Parameter] public string ChooseLabel { get; set; } = "Escolher";
    [Parameter] public string UploadLabel { get; set; } = "Enviar";
    [Parameter] public string CancelLabel { get; set; } = "Cancelar";
    [Parameter] public string ChooseIcon { get; set; } = "pi pi-plus";
    [Parameter] public string UploadIcon { get; set; } = "pi pi-upload";
    [Parameter] public string CancelIcon { get; set; } = "pi pi-times";
    [Parameter] public bool ShowUploadButton { get; set; } = true;
    [Parameter] public bool ShowCancelButton { get; set; } = true;

    [Parameter] public RenderFragment? HeaderTemplate { get; set; }
    [Parameter] public RenderFragment? ContentTemplate { get; set; }
    [Parameter] public RenderFragment? ToolbarTemplate { get; set; }
    [Parameter] public RenderFragment? EmptyTemplate { get; set; }
    [Parameter] public RenderFragment<OpFileUploadFile>? FileTemplate { get; set; }

    [Parameter] public EventCallback<IReadOnlyList<OpFileUploadFile>> OnSelect { get; set; }
    [Parameter] public EventCallback<OpFileUploadFile> OnRemove { get; set; }
    [Parameter] public EventCallback OnClear { get; set; }
    [Parameter] public EventCallback<IReadOnlyList<OpFileUploadFile>> OnUpload { get; set; }
    [Parameter] public EventCallback<int> OnProgress { get; set; }
    [Parameter] public EventCallback<IReadOnlyList<OpFileUploadFile>> OnError { get; set; }
    [Parameter] public EventCallback<IReadOnlyList<OpFileUploadFile>> UploadHandler { get; set; }

    public IReadOnlyList<OpFileUploadFile> Files => _files;

    public IReadOnlyList<OpFileUploadFile> UploadedFiles => _uploadedFiles;

    private bool HasFiles => _files.Count > 0;

    private bool ChooseDisabled => FileLimit is int limit && _files.Count >= limit;

    private string RootClass => Class(
        "p-fileupload p-component",
        Mode == "basic" ? "p-fileupload-basic" : "p-fileupload-advanced",
        Disabled ? "p-disabled" : null,
        StyleClass);

    private string ChooseClass => Class(
        "p-button p-component op-fileupload-choose",
        Disabled || ChooseDisabled ? "p-disabled" : null);

    private string ContentClass => Class(
        "p-fileupload-content",
        _dropHighlight ? "p-fileupload-highlight" : null);

    private string BasicChosenLabel
    {
        get
        {
            if (Auto)
            {
                return ChooseLabel;
            }

            if (_files.Count == 1)
            {
                return _files[0].Name;
            }

            return _files.Count > 1 ? $"{_files.Count} arquivos" : "Nenhum arquivo escolhido";
        }
    }

    public async Task OnInputChangeAsync(InputFileChangeEventArgs e)
    {
        _messages.Clear();

        if (!Multiple)
        {
            _files.Clear();
        }

        IReadOnlyList<IBrowserFile> incoming = Multiple
            ? e.GetMultipleFiles(FileLimit ?? int.MaxValue)
            : new[] { e.File };

        foreach (var browserFile in incoming)
        {
            var candidate = new OpFileUploadFile(browserFile);

            if (!Validate(candidate))
            {
                continue;
            }

            if (_files.Any(f => SameFile(f, candidate)))
            {
                continue;
            }

            if (FileLimit is int limit && _files.Count >= limit)
            {
                _messages.Add(("error", $"Limite de {limit} arquivo(s) excedido."));
                break;
            }

            _files.Add(candidate);
        }

        await OnSelect.InvokeAsync(_files.ToList());

        if (Auto && HasFiles)
        {
            await UploadAsync();
        }

        StateHasChanged();
    }

    public async Task UploadAsync()
    {
        if (Disabled || !HasFiles)
        {
            return;
        }

        var pending = _files.ToList();

        if (UploadHandler.HasDelegate)
        {
            await UploadHandler.InvokeAsync(pending);
        }

        foreach (var file in pending)
        {
            _uploadedFiles.Add(new OpFileUploadFile(file.File, isUploaded: true));
        }

        _files.Clear();
        _progress = 0;
        await OnUpload.InvokeAsync(pending);
        StateHasChanged();
    }

    public void Clear()
    {
        _files.Clear();
        _messages.Clear();
        OnClear.InvokeAsync();
        StateHasChanged();
    }

    private void RemoveAt(bool uploaded, int index)
    {
        var list = uploaded ? _uploadedFiles : _files;
        if (index < 0 || index >= list.Count)
        {
            return;
        }

        var removed = list[index];
        list.RemoveAt(index);

        if (!uploaded)
        {
            OnRemove.InvokeAsync(removed);
        }

        StateHasChanged();
    }

    private void OnDragEnter(DragEventArgs e) => _dropHighlight = true;

    private void OnDragLeave(DragEventArgs e) => _dropHighlight = false;

    private Task OnDropAsync(DragEventArgs e)
    {
        _dropHighlight = false;
        return Task.CompletedTask;
    }

    private bool Validate(OpFileUploadFile file)
    {
        if (!string.IsNullOrWhiteSpace(Accept) && !IsTypeValid(file))
        {
            _messages.Add(("error", $"Tipo de arquivo inválido: {file.Name}. Tipos aceitos: {Accept}."));
            return false;
        }

        if (MaxFileSize is long max && file.Size > max)
        {
            _messages.Add(("error", $"Tamanho inválido: {file.Name}. O máximo é {FormatSize(max)}."));
            return false;
        }

        return true;
    }

    private bool IsTypeValid(OpFileUploadFile file)
    {
        foreach (var raw in Accept!.Split(','))
        {
            var type = raw.Trim();
            if (type.Length == 0)
            {
                continue;
            }

            if (type.Contains('*'))
            {
                var prefix = type[..type.IndexOf('/')];
                if (file.ContentType.StartsWith(prefix + "/", StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }
            else if (type.StartsWith('.'))
            {
                if (file.Name.EndsWith(type, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }
            else if (string.Equals(file.ContentType, type, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }

        return false;
    }

    private static bool SameFile(OpFileUploadFile a, OpFileUploadFile b)
        => string.Equals(a.Name, b.Name, StringComparison.Ordinal)
            && a.Size == b.Size
            && string.Equals(a.ContentType, b.ContentType, StringComparison.Ordinal);

    private static string FormatSize(long bytes)
    {
        string[] sizes = { "B", "KB", "MB", "GB", "TB" };
        if (bytes == 0)
        {
            return "0 B";
        }

        var index = Math.Min((int)Math.Floor(Math.Log(bytes) / Math.Log(1024)), sizes.Length - 1);
        var value = bytes / Math.Pow(1024, index);
        return value.ToString("0.###", CultureInfo.InvariantCulture) + " " + sizes[index];
    }
}
