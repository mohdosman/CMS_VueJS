using System;
using System.Collections.Generic;

namespace CMS.Data.Models.Domain;

public sealed class Contract : AuditableEntity
{
    public int ContractId { get; set; }

    public string Name { get; set; } = null!;

    public string ContractNumber { get; set; } = null!;

    public int? ProgramId { get; set; }

    public int? ProviderId { get; set; }

    public int? FiscalYearId { get; set; }

    public string? LineNumber { get; set; }

    public int? MaximumLiability { get; set; }

    public DateTime? StartupPaidToDate { get; set; }

    public FiscalYear? FiscalYear { get; set; }

    public Program? Program { get; set; }

    public Provider? Provider { get; set; }
}

