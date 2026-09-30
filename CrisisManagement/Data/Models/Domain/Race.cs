using System;
using System.Collections.Generic;

namespace CrisisManagement.Data.Models.Domain;

public sealed class Race : AuditableEntity
{
    public byte RaceId { get; set; }

    public string NOMSId { get; set; } = null!;

    public string RaceDescription { get; set; } = null!;

    public ICollection<Patient> Patients { get; set; } = new List<Patient>();
}

