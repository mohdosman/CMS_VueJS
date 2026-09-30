using System;
using System.Collections.Generic;

namespace CMS.Data.Models.Domain;

public sealed class ServiceCode : AuditableEntity
{
    public int ServiceCodeId { get; set; }

    public string ServiceCodeName { get; set; } = null!;

    public string ServiceCodeAbbrev { get; set; } = null!;

    public bool IsDurationHoursRequired { get; set; }

    public bool IsDischargeDateRequired { get; set; }

    public ICollection<Service> Services { get; set; } = new List<Service>();
}

