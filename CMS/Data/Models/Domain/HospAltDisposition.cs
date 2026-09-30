using System;
using System.Collections.Generic;

namespace CMS.Data.Models.Domain;

public sealed class HospAltDisposition : AuditableEntity
{
    public int HospAltDispositionId { get; set; }

    public int HospitalizationAlternativeId { get; set; }

    public int HospAltDispositionListId { get; set; }

    public ICollection<F2FHospAlternative> F2FHospAlternatives { get; set; } = new List<F2FHospAlternative>();

    public HospAltDispositionList HospAltDispositionList { get; set; } = null!;

    public HospitalizationAlternative HospitalizationAlternative { get; set; } = null!;
}

