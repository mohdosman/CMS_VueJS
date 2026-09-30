using System;
using System.Collections.Generic;

namespace CMS.Data.Models.Domain;

public sealed class Patient : AuditableEntity
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

    public ICollection<F2FAssessment> F2FAssessments { get; set; } = new List<F2FAssessment>();

    public ICollection<PatientAddress> PatientAddresses { get; set; } = new List<PatientAddress>();

    public ICollection<PhoneAssessment> PhoneAssessments { get; set; } = new List<PhoneAssessment>();

    public ICollection<Service> Services { get; set; } = new List<Service>();

    public Ethnicity? Ethnicity { get; set; }

    public Gender? Gender { get; set; }

    public Race? Race { get; set; }
}

