using System;
using System.Collections.Generic;

namespace CrisisManagement.Data.Models.Domain;

public sealed class F2FDrug : AuditableEntity
{
    public int F2FDrugId { get; set; }

    public int F2FAssessmentId { get; set; }

    public int DrugId { get; set; }

    public int? DrugRouteId { get; set; }

    public int? DrugFrequencyId { get; set; }

    public Drug Drug { get; set; } = null!;

    public DrugFrequency? DrugFrequency { get; set; }

    public DrugRoute? DrugRoute { get; set; }

    public F2FAssessment F2FAssessment { get; set; } = null!;
}

