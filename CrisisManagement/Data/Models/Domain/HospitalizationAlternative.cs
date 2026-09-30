using System;
using System.Collections.Generic;

namespace CrisisManagement.Data.Models.Domain;

public sealed class HospitalizationAlternative : AuditableEntity
{
    public int HospitalizationAlternativeId { get; set; }

    public string Abbreviation { get; set; } = null!;

    public string HospitalizationAlternativeDescription { get; set; } = null!;

    public int? Hierarchy { get; set; }

    public ICollection<HospAltDisposition> HospAltDispositions { get; set; } = new List<HospAltDisposition>();
}

