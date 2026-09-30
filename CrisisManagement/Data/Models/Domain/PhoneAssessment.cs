using System;
using System.Collections.Generic;

namespace CrisisManagement.Data.Models.Domain;

public sealed class PhoneAssessment : AuditableEntity
{
    public int PhoneAssessmentId { get; set; }

    public string? ProviderPhoneAssessmentId { get; set; }

    public int PatientId { get; set; }

    public int ProviderId { get; set; }

    public DateTime CallEnded { get; set; }

    public int DispositionId { get; set; }

    public string? DispositionOther { get; set; }

    public DateTime? DispositionDispatchTime { get; set; }

    public string? Comment { get; set; }

    public ICollection<F2FAssessment> F2FAssessments { get; set; } = new List<F2FAssessment>();

    public Disposition Disposition { get; set; } = null!;

    public Patient Patient { get; set; } = null!;

    public Provider Provider { get; set; } = null!;
}

