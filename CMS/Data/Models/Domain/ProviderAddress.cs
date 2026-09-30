using System;
using System.Collections.Generic;

namespace CMS.Data.Models.Domain;

public sealed class ProviderAddress : AuditableEntity
{
    public int ProviderAddressId { get; set; }

    public int ProviderId { get; set; }

    public int AddressId { get; set; }

    public short AddressTypeId { get; set; }

    public Address Address { get; set; } = null!;

    public AddressType AddressType { get; set; } = null!;

    public Provider Provider { get; set; } = null!;
}

