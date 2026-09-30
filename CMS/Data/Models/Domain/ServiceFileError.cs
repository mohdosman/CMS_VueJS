using System;
using System.Collections.Generic;

namespace CMS.Data.Models.Domain;

public sealed class ServiceFileError : AuditableEntity
{
    public int ServiceFileErrorId { get; set; }

    public int ServiceFileImportId { get; set; }

    public int ServiceFileErrorCodeId { get; set; }

    public ServiceFileErrorCode ServiceFileErrorCode { get; set; } = null!;

    public ServiceFileImport ServiceFileImport { get; set; } = null!;
}

