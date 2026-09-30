using System;
using System.Collections.Generic;

namespace CrisisManagement.Data.Models.Domain;

public sealed class RecommendedTransportMode : AuditableEntity
{
    public byte RecommendedTransportModeId { get; set; }

    public string RecommendedTransportModeDescription { get; set; } = null!;

    public string Abbreviation { get; set; } = null!;

    public ICollection<F2FAssessment> F2FAssessments { get; set; } = new List<F2FAssessment>();
}

