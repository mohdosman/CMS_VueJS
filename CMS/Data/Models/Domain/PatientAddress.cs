using System;
using System.Collections.Generic;

namespace CMS.Data.Models.Domain;

public sealed class PatientAddress : AuditableEntity
{
    public int PatientAddressId { get; set; }

    public int PatientId { get; set; }

    public int AddressId { get; set; }

    public short AddressTypeId { get; set; }

    public Address Address { get; set; } = null!;

    public AddressType AddressType { get; set; } = null!;

    public Patient Patient { get; set; } = null!;
}

