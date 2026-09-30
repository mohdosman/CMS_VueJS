using System;
using System.Collections.Generic;

namespace CrisisManagement.Data.Models.Domain;

public sealed class EducationLevel : AuditableEntity
{
    public int EducationLevelId { get; set; }

    public string? NOMSId { get; set; }

    public string EducationLevelDescription { get; set; } = null!;

    public int? SortOrder { get; set; }

    public ICollection<F2FAssessment> F2FAssessments { get; set; } = new List<F2FAssessment>();
}

