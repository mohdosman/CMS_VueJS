using System;
using System.Collections.Generic;

namespace CrisisManagement.Data.Models.Domain;

public sealed class Contact : AuditableEntity
{
    public int ContactId { get; set; }

    public string? LastName { get; set; }

    public string? FirstName { get; set; }

    public string? Title { get; set; }

    public string? EmailAddress { get; set; }

    public string? Phone { get; set; }

    public string? WirelessPhone { get; set; }

    public ICollection<Address> Addresses { get; set; } = new List<Address>();
}

