using System;
using System.Collections.Generic;

namespace CMS.Data.Models.Domain;

public sealed class DocumentType : AuditableEntity
{
    public int DocumentTypeId { get; set; }

    public string DocumentTypeName { get; set; } = null!;

    public ICollection<Document> Documents { get; set; } = new List<Document>();
}

