using System;
using System.Collections.Generic;

namespace CrisisManagement.Data.Models.Domain;

public sealed class County : AuditableEntity
{
    public short CountyId { get; set; }

    public string CountyCode { get; set; } = null!;

    public string CountyDescription { get; set; } = null!;

    public string? ServiceArea { get; set; }

    public byte? RegionId { get; set; }

    public byte? MHPlanningRegion { get; set; }

    public int? AdultPopulation { get; set; }

    public int? ChildPopulation { get; set; }

    public ICollection<Address> Addresses { get; set; } = new List<Address>();

    public ICollection<F2FAssessment> F2FAssessments { get; set; } = new List<F2FAssessment>();

    public ICollection<Service> ServiceCounties { get; set; } = new List<Service>();

    public ICollection<Service> ServiceServiceCounties { get; set; } = new List<Service>();
}

