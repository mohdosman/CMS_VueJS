using System;
using System.Collections.Generic;

namespace CrisisManagement.Data.Models.Domain;

public sealed class HospAltDispositionList : AuditableEntity
{
    public int HospAltDispositionListId { get; set; }

    public string Abbreviation { get; set; } = null!;

    public string HospAltDisposition { get; set; } = null!;

    public bool IsFinalDisposition { get; set; }

    public ICollection<HospAltDisposition> HospAltDispositions { get; set; } = new List<HospAltDisposition>();
}

