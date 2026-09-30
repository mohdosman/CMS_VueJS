using System;
using System.Collections.Generic;

namespace CrisisManagement.Data.Models.Domain;

public sealed class AddressType : AuditableEntity
{
    public short AddressTypeId { get; set; }

    public string AddressTypeDescription { get; set; } = null!;

    public ICollection<PatientAddress> PatientAddresses { get; set; } = new List<PatientAddress>();

    public ICollection<ProviderAddress> ProviderAddresses { get; set; } = new List<ProviderAddress>();
}

