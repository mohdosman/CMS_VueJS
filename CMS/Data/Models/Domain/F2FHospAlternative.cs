using System;
using System.Collections.Generic;

namespace CMS.Data.Models.Domain;

public sealed class F2FHospAlternative : AuditableEntity
{
    public int F2FHospAlternativeId { get; set; }

    public int F2FAssessmentId { get; set; }

    public int HospAltDispositionId { get; set; }

    public F2FAssessment F2FAssessment { get; set; } = null!;

    public HospAltDisposition HospAltDisposition { get; set; } = null!;
}

