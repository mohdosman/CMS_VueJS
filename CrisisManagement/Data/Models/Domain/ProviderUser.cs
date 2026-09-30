using CrisisManagement.Data.Models.Identity;
using System;
using System.Collections.Generic;

namespace CrisisManagement.Data.Models.Domain;

public sealed class ProviderUser : AuditableEntity
{
    public int ProviderUserId { get; set; }

    public int ProviderId { get; set; }

    public int UserId { get; set; }

    public Provider Provider { get; set; } = null!;

    public ApplicationUser User { get; set; } = null!;
}

