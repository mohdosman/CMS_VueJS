using System;
using System.Collections.Generic;

namespace CMS.Data.Models.Domain;

public sealed class AssessmentType : AuditableEntity
{
    public int AssessmentTypeId { get; set; }

    public string AssessmentTypeDescription { get; set; } = null!;

    public ICollection<F2FAssessment> F2FAssessments { get; set; } = new List<F2FAssessment>();
}

