using System;
using System.Collections.Generic;

namespace CrisisManagement.Data.Models.Domain;

public sealed class Disposition : AuditableEntity
{
    public int DispositionId { get; set; }

    public string DispositionCode { get; set; } = null!;

    public string DispositionDescription { get; set; } = null!;

    public ICollection<PhoneAssessment> PhoneAssessments { get; set; } = new List<PhoneAssessment>();
}

