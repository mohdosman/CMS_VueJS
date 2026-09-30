using System;
using System.Collections.Generic;

namespace CMS.Data.Models.Domain;

public sealed class Program : AuditableEntity
{
    public int ProgramId { get; set; }

    public string ProgramDescription { get; set; } = null!;

    public string ProgramCode { get; set; } = null!;

    public string? EdisonCategoryNumber { get; set; }

    public string? EdisonDepartmentNumber { get; set; }

    public string? EdisonAccountNumber { get; set; }

    public ICollection<Contract> Contracts { get; set; } = new List<Contract>();

    public ICollection<ProgramMaxLiability> ProgramMaxLiabilities { get; set; } = new List<ProgramMaxLiability>();
}

