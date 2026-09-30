using System;
using System.Collections.Generic;

namespace CMS.Data.Models.Domain;

public sealed class ServiceFileImport : AuditableEntity
{
    public int ServiceFileImportId { get; set; }

    public string? ProviderPatientNo { get; set; }

    public string? SSN { get; set; }

    public string? LastName { get; set; }

    public string? FirstName { get; set; }

    public string? DOB { get; set; }

    public string? Gender { get; set; }

    public string? County { get; set; }

    public string? ServiceCode { get; set; }

    public string? DOSAdmitDate { get; set; }

    public string? DischargeDate { get; set; }

    public string? DurationHours { get; set; }

    public string? PayorSource { get; set; }

    public string? PrimaryInsurer { get; set; }

    public string? ServiceCounty { get; set; }

    public bool IsProcessed { get; set; }

    public int ServiceFileId { get; set; }

    public ICollection<ServiceFileError> ServiceFileErrors { get; set; } = new List<ServiceFileError>();

    public ICollection<Service> Services { get; set; } = new List<Service>();

    public ServiceFile ServiceFile { get; set; } = null!;
}

