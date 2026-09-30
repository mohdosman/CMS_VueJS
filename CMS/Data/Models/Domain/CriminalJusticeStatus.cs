using System;
using System.Collections.Generic;

namespace CMS.Data.Models.Domain;

public sealed class CriminalJusticeStatus : AuditableEntity
{
    public int CriminalJusticeStatusId { get; set; }

    public string NOMSId { get; set; } = null!;

    public string CriminalJusticeStatusCode { get; set; } = null!;

    public string CriminalJusticeStatusDescription { get; set; } = null!;

}

