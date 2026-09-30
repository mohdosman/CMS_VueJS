using System;
using System.Collections.Generic;

namespace CMS.Data.Models.Domain;

public sealed class F2FEvaluation : AuditableEntity
{
    public int F2FEvaluationId { get; set; }

    public int F2FAssessmentId { get; set; }

    public int EvaluationTypeId { get; set; }

    public byte EvaluationValue { get; set; }

    public EvaluationType EvaluationType { get; set; } = null!;

    public F2FAssessment F2FAssessment { get; set; } = null!;
}

