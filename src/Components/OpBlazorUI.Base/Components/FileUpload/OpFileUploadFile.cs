namespace OpBlazorUI.Base.Components.FileUpload;

/// <summary>Arquivo selecionado no <see cref="OpFileUpload"/>, com metadados e o <c>IBrowserFile</c> original.</summary>
public sealed class OpFileUploadFile
{
    public OpFileUploadFile(Microsoft.AspNetCore.Components.Forms.IBrowserFile file, bool isUploaded = false)
    {
        File = file;
        Name = file.Name;
        Size = file.Size;
        ContentType = file.ContentType;
        IsUploaded = isUploaded;
    }

    public Microsoft.AspNetCore.Components.Forms.IBrowserFile File { get; }

    public string Name { get; }

    public long Size { get; }

    public string ContentType { get; }

    public bool IsUploaded { get; }

    public bool IsImage => ContentType.StartsWith("image/", StringComparison.OrdinalIgnoreCase);
}
