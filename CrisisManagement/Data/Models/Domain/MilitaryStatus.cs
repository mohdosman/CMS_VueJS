using System;
using System.Collections.Generic;

namespace CrisisManagement.Data.Models.Domain;

public sealed class MilitaryStatus : AuditableEntity
{
    public int MilitaryStatusId { get; set; }

    public string MilitaryStatusCode { get; set; } = null!;

    public string MilitaryStatusDescription { get; set; } = null!;

    public ICollection<F2FAssessment> F2FAssessments { get; set; } = new List<F2FAssessment>();
}

