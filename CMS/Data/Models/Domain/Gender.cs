using System;
using System.Collections.Generic;

namespace CMS.Data.Models.Domain;

public sealed class Gender : AuditableEntity
{
    public byte GenderId { get; set; }

    public string NOMSId { get; set; } = null!;

    public string GenderDescription { get; set; } = null!;

    public ICollection<Patient> Patients { get; set; } = new List<Patient>();
}

