using System;
using System.Collections.Generic;

namespace CrisisManagement.Data.Models.Domain;

public sealed class EvaluationType : AuditableEntity
{
    public int EvaluationTypeId { get; set; }

    public string EvaluationTypeDescription { get; set; } = null!;

    public int EvaluationCategoryId { get; set; }

    public ICollection<F2FEvaluation> F2FEvaluations { get; set; } = new List<F2FEvaluation>();

    public EvaluationCategory EvaluationCategory { get; set; } = null!;
}

