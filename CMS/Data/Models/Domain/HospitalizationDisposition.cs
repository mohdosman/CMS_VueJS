using System;
using System.Collections.Generic;

namespace CMS.Data.Models.Domain;

public sealed class HospitalizationDisposition : AuditableEntity
{
    public int HospitalizationDispositionId { get; set; }

    public string Abbreviation { get; set; } = null!;

    public string HospitalizationDispositionDescription { get; set; } = null!;

    public bool IsFinalDisposition { get; set; }

    public ICollection<F2FHospitalization> F2FHospitalizations { get; set; } = new List<F2FHospitalization>();
}

