using System;
using System.Collections.Generic;

namespace CMS.Data.Models.Domain;

public sealed class FileUploadPatient : AuditableEntity
{
    public int PatientId { get; set; }

    public string? ProviderPatientNo { get; set; }

    public string? SSN { get; set; }

    public string LastName { get; set; } = null!;

    public string? FirstName { get; set; }

    public DateTime? DOB { get; set; }

    public byte? GenderId { get; set; }

    public byte? RaceId { get; set; }

    public byte? EthnicityId { get; set; }

    public int FileUploadId { get; set; }

    public string? AssessmentXML { get; set; }

    public bool IsImported { get; set; }

}

