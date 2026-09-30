using System;
using System.Collections.Generic;

namespace CrisisManagement.Data.Models.Domain;

public sealed class EmploymentStatus : AuditableEntity
{
    public int EmploymentStatusId { get; set; }

    public string NOMSId { get; set; } = null!;

    public string EmploymentStatusDescription { get; set; } = null!;

    public ICollection<F2FAssessment> F2FAssessments { get; set; } = new List<F2FAssessment>();
}

