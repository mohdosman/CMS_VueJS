using System;
using System.Collections.Generic;

namespace CMS.Data.Models.Domain;

public sealed class Hospitalization : AuditableEntity
{
    public int HospitalizationId { get; set; }

    public string Abbreviation { get; set; } = null!;

    public string HospitalizationDescription { get; set; } = null!;

    public byte? AvatarFacilityId { get; set; }

    public string? NPI { get; set; }

    public ICollection<F2FHospitalization> F2FHospitalizations { get; set; } = new List<F2FHospitalization>();
}

