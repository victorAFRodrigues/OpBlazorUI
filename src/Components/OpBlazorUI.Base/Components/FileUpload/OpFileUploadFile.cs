namespace OpBlazorUI.Base.Components.FileUpload;

/// <summary>Arquivo selecionado no <see cref="OpFileUpload"/>, com metadados e o <c>IBrowserFile</c> original.</summary>
public sealed class OpFileUploadFile
{
    public OpFileUploadFile(Microsoft.AspNetCore.Components.Forms.IBrowserFile file, bool isUploaded = false, byte[]? content = null)
    {
        File = file;
        Name = file.Name;
        Size = file.Size;
        ContentType = file.ContentType;
        IsUploaded = isUploaded;
        Content = content;
    }

    public Microsoft.AspNetCore.Components.Forms.IBrowserFile File { get; }

    public string Name { get; }

    public long Size { get; }

    public string ContentType { get; }

    public bool IsUploaded { get; }

    /// <summary>
    /// Conteúdo lido para memória na seleção. Mantém o arquivo legível mesmo quando uma nova
    /// seleção reaproveita o mesmo <c>InputFile</c> (o Blazor zera o mapa de arquivos no <c>change</c>).
    /// </summary>
    public byte[]? Content { get; }

    public bool IsImage => ContentType.StartsWith("image/", StringComparison.OrdinalIgnoreCase);

    /// <summary>Abre o fluxo do arquivo. Usa o conteúdo em memória quando disponível.</summary>
    public Stream OpenReadStream(long maxAllowedSize = long.MaxValue)
    {
        if (Content is null)
        {
            return File.OpenReadStream(maxAllowedSize);
        }

        return new MemoryStream(Content, writable: false);
    }
}
