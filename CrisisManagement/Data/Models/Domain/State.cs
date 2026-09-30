using System;
using System.Collections.Generic;

namespace CrisisManagement.Data.Models.Domain;

public sealed class State : AuditableEntity
{
    public short StateId { get; set; }

    public string StateCode { get; set; } = null!;

    public string StateDescription { get; set; } = null!;

    public ICollection<Address> Addresses { get; set; } = new List<Address>();
}

