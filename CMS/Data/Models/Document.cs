namespace CMS.Data.Models;

// CMS_Document: public help files (DocumentTypeId 2, no owning user) and per-user agreements (type 3).
public sealed class Document
{
    public const int HelpTypeId = 2;

    public int DocumentId { get; set; }
    public int DocumentTypeId { get; set; }
    public byte[] FileContent { get; set; } = [];
    public string FileName { get; set; } = "";
    public bool IsActive { get; set; }
    public string MIMEType { get; set; } = "";
    public int? UserId { get; set; }
    public DateTime CreatedOn { get; set; }
}
