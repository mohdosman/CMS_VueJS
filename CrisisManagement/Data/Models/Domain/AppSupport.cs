using System;
using System.Collections.Generic;

namespace CrisisManagement.Data.Models.Domain;

public sealed class AppSupport : AuditableEntity
{
    public int AppSupportId { get; set; }

    public int ContactId { get; set; }

    public Contact Contact { get; set; } = null!;
}

