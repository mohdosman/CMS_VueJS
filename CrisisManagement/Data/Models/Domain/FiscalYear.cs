using System;
using System.Collections.Generic;

namespace CrisisManagement.Data.Models.Domain;

public sealed class FiscalYear : AuditableEntity
{
    public int FiscalYearId { get; set; }

    public short FiscalYearDescription { get; set; }

    public DateTime StartDate { get; set; }

    public DateTime EndDate { get; set; }

    public ICollection<Contract> Contracts { get; set; } = new List<Contract>();

    public ICollection<DivisionMaxLiability> DivisionMaxLiabilities { get; set; } = new List<DivisionMaxLiability>();

    public ICollection<ProgramMaxLiability> ProgramMaxLiabilities { get; set; } = new List<ProgramMaxLiability>();
}

