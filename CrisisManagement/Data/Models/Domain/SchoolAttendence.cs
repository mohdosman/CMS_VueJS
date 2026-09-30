using System;
using System.Collections.Generic;

namespace CrisisManagement.Data.Models.Domain;

public sealed class SchoolAttendence : AuditableEntity
{
    public int SchoolAttendenceId { get; set; }

    public string NOMSId { get; set; } = null!;

    public string SchoolAttendenceCode { get; set; } = null!;

    public string SchoolAttendenceDescription { get; set; } = null!;

}

