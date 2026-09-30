using System;
using System.Collections.Generic;

namespace CMS.Data.Models.Domain;

public sealed class Service : AuditableEntity
{
    public int ServiceId { get; set; }

    public int PatientId { get; set; }

    public int ProviderId { get; set; }

    public short CountyId { get; set; }

    public int ServiceCodeId { get; set; }

    public int PayorSourceId { get; set; }

    public int? PrimaryInsurerId { get; set; }

    public DateTime DOSAdmitDate { get; set; }

    public DateTime? DischargeDate { get; set; }

    public int? DurationHours { get; set; }

    public int? ServiceFileImportId { get; set; }

    public short ServiceCountyId { get; set; }

    public string? SessionId { get; set; }

    public County County { get; set; } = null!;

    public Patient Patient { get; set; } = null!;

    public PayorSource PayorSource { get; set; } = null!;

    public PayorSource? PrimaryInsurer { get; set; }

    public Provider Provider { get; set; } = null!;

    public ServiceCode ServiceCode { get; set; } = null!;

    public County ServiceCounty { get; set; } = null!;

    public ServiceFileImport? ServiceFileImport { get; set; }
}

