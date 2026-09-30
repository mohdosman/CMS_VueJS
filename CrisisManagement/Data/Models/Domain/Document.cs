using System;
using System.Collections.Generic;

namespace CrisisManagement.Data.Models.Domain;

public sealed class Document : AuditableEntity
{
    public int DocumentId { get; set; }

    public int DocumentTypeId { get; set; }

    public byte[] FileContent { get; set; } = null!;

    public string FileName { get; set; } = null!;

    public bool IsActive { get; set; }

    public string MIMEType { get; set; } = null!;

    /// <summary>Owning user, when the document belongs to one. Null for public/help files.</summary>
    public int? UserId { get; set; }

    public DocumentType DocumentType { get; set; } = null!;
}

