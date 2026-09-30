using System;
using System.Collections.Generic;

namespace CMS.Data.Models.Domain;

public sealed class ProgramMaxLiability : AuditableEntity
{
    public int ProgramMaxLiabilityId { get; set; }

    public int ProgramId { get; set; }

    public int FiscalYearId { get; set; }

    public decimal? MaxLiability { get; set; }

    public FiscalYear FiscalYear { get; set; } = null!;

    public Program Program { get; set; } = null!;
}

