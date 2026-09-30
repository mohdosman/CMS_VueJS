using System;
using System.Collections.Generic;

namespace CrisisManagement.Data.Models.Domain;

public sealed class DivisionMaxLiability : AuditableEntity
{
    public int DivisionMaxLiabilityId { get; set; }

    public int FiscalYearId { get; set; }

    public decimal MaxLiability { get; set; }

    public FiscalYear FiscalYear { get; set; } = null!;
}

