using System;
using System.Collections.Generic;

namespace CrisisManagement.Data.Models.Domain;

public sealed class FileUploadErrorTable : AuditableEntity
{
    public byte FileUploadErrorTableId { get; set; }

    public string FileUploadErrorTableDescription { get; set; } = null!;

    public string Abbrev { get; set; } = null!;

    public ICollection<FileUploadError> FileUploadErrors { get; set; } = new List<FileUploadError>();
}

