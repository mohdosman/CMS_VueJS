using System;
using System.Collections.Generic;

namespace CrisisManagement.Data.Models.Domain;

public sealed class ServiceFileErrorCode : AuditableEntity
{
    public int ServiceFileErrorCodeId { get; set; }

    public string ServiceFileErrorCodeDescription { get; set; } = null!;

    public ICollection<ServiceFileError> ServiceFileErrors { get; set; } = new List<ServiceFileError>();
}

