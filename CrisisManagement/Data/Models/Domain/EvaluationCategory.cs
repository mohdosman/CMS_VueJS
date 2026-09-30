using System;
using System.Collections.Generic;

namespace CrisisManagement.Data.Models.Domain;

public sealed class EvaluationCategory : AuditableEntity
{
    public int EvaluationCategoryId { get; set; }

    public string EvaluationCategoryDescription { get; set; } = null!;

    public ICollection<EvaluationType> EvaluationTypes { get; set; } = new List<EvaluationType>();
}

