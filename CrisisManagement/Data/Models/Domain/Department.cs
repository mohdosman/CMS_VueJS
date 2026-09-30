using System;
using System.Collections.Generic;

namespace CrisisManagement.Data.Models.Domain;

public sealed class Department : AuditableEntity
{
    public int DepartmentId { get; set; }

    public string? DepartmentName { get; set; }

    public string? DivisionName { get; set; }

    public string? AddressLine1 { get; set; }

    public string? AddressLine2 { get; set; }

    public string? City { get; set; }

    public string? State { get; set; }

    public string? Zip { get; set; }

    public string? ZipExtension { get; set; }

    public string? Notes { get; set; }

}

