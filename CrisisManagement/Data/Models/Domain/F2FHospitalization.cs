using System;
using System.Collections.Generic;

namespace CrisisManagement.Data.Models.Domain;

public sealed class F2FHospitalization : AuditableEntity
{
    public int F2FHospitalizationId { get; set; }

    public int F2FAssessmentId { get; set; }

    public int HospitalizationId { get; set; }

    public int HospitalizationDispositionId { get; set; }

    public F2FAssessment F2FAssessment { get; set; } = null!;

    public Hospitalization Hospitalization { get; set; } = null!;

    public HospitalizationDisposition HospitalizationDisposition { get; set; } = null!;
}

