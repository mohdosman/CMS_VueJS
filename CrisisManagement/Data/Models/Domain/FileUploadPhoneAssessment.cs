using System;
using System.Collections.Generic;

namespace CrisisManagement.Data.Models.Domain;

public sealed class FileUploadPhoneAssessment : AuditableEntity
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

    public string? F2FAssessmentXML { get; set; }

    public int FileUploadId { get; set; }

    public bool IsProcessed { get; set; }

    public int? ReferenceId { get; set; }

    public bool HasError { get; set; }

    public bool IsImported { get; set; }

}

