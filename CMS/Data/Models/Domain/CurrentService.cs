using System;
using System.Collections.Generic;

namespace CMS.Data.Models.Domain;

public sealed class CurrentService : AuditableEntity
{
    public int CurrentServicesId { get; set; }

    public string CurrentServices { get; set; } = null!;

    public ICollection<F2FAssessment> F2FAssessments { get; set; } = new List<F2FAssessment>();
}

