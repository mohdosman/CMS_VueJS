using System;
using System.Collections.Generic;

namespace CrisisManagement.Data.Models.Domain;

public sealed class ResidentialStatus : AuditableEntity
{
    public int ResidentialStatusId { get; set; }

    public string NOMSId { get; set; } = null!;

    public string ResidentialStatusDescription { get; set; } = null!;

    public ICollection<F2FAssessment> F2FAssessments { get; set; } = new List<F2FAssessment>();
}

