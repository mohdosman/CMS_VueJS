using System;
using System.Collections.Generic;

namespace CrisisManagement.Data.Models.Domain;

public sealed class FileUploadError : AuditableEntity
{
    public long FileUploadErrorId { get; set; }

    public int FileUploadId { get; set; }

    public byte FileUploadErrorTableId { get; set; }

    public int FileUploadErrorTableRecordId { get; set; }

    public int FileUploadErrorCodeId { get; set; }

    public FileUpload FileUpload { get; set; } = null!;

    public FileUploadErrorCode FileUploadErrorCode { get; set; } = null!;

    public FileUploadErrorTable FileUploadErrorTable { get; set; } = null!;
}

