using System;
using System.Collections.Generic;

namespace CMS.Data.Models.Domain;

public sealed class Drug : AuditableEntity
{
    public int DrugId { get; set; }

    public string? NOMSId { get; set; }

    public string DrugDescription { get; set; } = null!;

    public ICollection<F2FDrug> F2FDrugs { get; set; } = new List<F2FDrug>();
}

