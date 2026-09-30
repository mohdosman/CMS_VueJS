using System;
using System.Collections.Generic;

namespace CrisisManagement.Data.Models.Domain;

public sealed class Ethnicity : AuditableEntity
{
    public byte EthnicityId { get; set; }

    public string NOMSID { get; set; } = null!;

    public string EthnicityDescription { get; set; } = null!;

    public ICollection<Patient> Patients { get; set; } = new List<Patient>();
}

