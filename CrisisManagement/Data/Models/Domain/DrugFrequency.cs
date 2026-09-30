using System;
using System.Collections.Generic;

namespace CrisisManagement.Data.Models.Domain;

public sealed class DrugFrequency : AuditableEntity
{
    public int DrugFrequencyId { get; set; }

    public string? NOMSId { get; set; }

    public string DrugFrequencyDescription { get; set; } = null!;

    public ICollection<F2FDrug> F2FDrugs { get; set; } = new List<F2FDrug>();
}

