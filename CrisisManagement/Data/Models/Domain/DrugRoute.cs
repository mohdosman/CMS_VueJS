using System;
using System.Collections.Generic;

namespace CrisisManagement.Data.Models.Domain;

public sealed class DrugRoute : AuditableEntity
{
    public int DrugRouteId { get; set; }

    public string? NOMSId { get; set; }

    public string DrugRouteDescription { get; set; } = null!;

    public ICollection<F2FDrug> F2FDrugs { get; set; } = new List<F2FDrug>();
}

