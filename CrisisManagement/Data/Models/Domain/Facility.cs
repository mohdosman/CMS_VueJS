using System;
using System.Collections.Generic;

namespace CrisisManagement.Data.Models.Domain;

public sealed class Facility : AuditableEntity
{
    public int FacilityId { get; set; }

    public int AvatarFacilityId { get; set; }

    public string FacilityName { get; set; } = null!;

    public string? Abbreviation { get; set; }

    public int? CountyId { get; set; }

    public string? AddressLine1 { get; set; }

    public string? AddressLine2 { get; set; }

    public string? City { get; set; }

    public string? State { get; set; }

    public string? Zipcode { get; set; }

    public string? ZipExtension { get; set; }

    public bool IsActive { get; set; }

    public ICollection<FacilityUser> FacilityUsers { get; set; } = new List<FacilityUser>();
}

