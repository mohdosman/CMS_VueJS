using System;
using System.Collections.Generic;

namespace CrisisManagement.Data.Models.Domain;

public sealed class AssessmentLocation : AuditableEntity
{
    public int AssessmentLocationId { get; set; }

    public string AssessmentLocationDescription { get; set; } = null!;

    public ICollection<F2FAssessment> F2FAssessments { get; set; } = new List<F2FAssessment>();
}

