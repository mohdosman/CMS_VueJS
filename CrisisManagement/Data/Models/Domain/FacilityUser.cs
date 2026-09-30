using CrisisManagement.Data.Models.Identity;
using System;
using System.Collections.Generic;

namespace CrisisManagement.Data.Models.Domain;

public sealed class FacilityUser : AuditableEntity
{
    public int FacilityUserId { get; set; }

    public int FacilityId { get; set; }

    public int UserId { get; set; }

    public Facility Facility { get; set; } = null!;

    public ApplicationUser User { get; set; } = null!;
}

