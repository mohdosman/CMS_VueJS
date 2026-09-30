namespace CrisisManagement.Data.Models.Domain;

public sealed class FileUpload : AuditableEntity
{
    public int FileUploadId { get; set; }

    public string FileName { get; set; } = string.Empty;

    public string FileTextXML { get; set; } = string.Empty;

    public bool IsInProcess { get; set; }

    public bool IsProcessed { get; set; }

    public string? ProviderNPI { get; set; }

    public ICollection<FileUploadError> FileUploadErrors { get; set; } = new List<FileUploadError>();

}
