using System;
using System.Collections.Generic;

namespace CMS.Data.Models.Domain;

public sealed class PrimaryProblem : AuditableEntity
{
    public short PrimaryProblemId { get; set; }

    public string PrimaryProblemDescription { get; set; } = null!;

    public ICollection<F2FAssessment> F2FAssessments { get; set; } = new List<F2FAssessment>();
}

