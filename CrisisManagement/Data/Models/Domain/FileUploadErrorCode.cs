using System;
using System.Collections.Generic;

namespace CrisisManagement.Data.Models.Domain;

public sealed class FileUploadErrorCode : AuditableEntity
{
    public int FileUploadErrorCodeId { get; set; }

    public string FileUploadErrorCodeDescription { get; set; } = null!;

    public ICollection<FileUploadError> FileUploadErrors { get; set; } = new List<FileUploadError>();
}

