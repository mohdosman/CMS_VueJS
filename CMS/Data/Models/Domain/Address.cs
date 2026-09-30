using System;
using System.Collections.Generic;

namespace CMS.Data.Models.Domain;

public sealed class Address : AuditableEntity
{
    public int AddressId { get; set; }

    public string? AddressLine1 { get; set; }

    public string? AddressLine2 { get; set; }

    public string? City { get; set; }

    public short? StateId { get; set; }

    public short? CountyId { get; set; }

    public string? Zipcode { get; set; }

    public string? ZipExtension { get; set; }

    public bool? IsActive { get; set; }

    public int? ContactId { get; set; }

    public ICollection<PatientAddress> PatientAddresses { get; set; } = new List<PatientAddress>();

    public ICollection<ProviderAddress> ProviderAddresses { get; set; } = new List<ProviderAddress>();

    public Contact? Contact { get; set; }

    public County? County { get; set; }

    public State? State { get; set; }
}

