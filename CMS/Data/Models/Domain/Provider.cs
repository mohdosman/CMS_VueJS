using System;
using System.Collections.Generic;

namespace CMS.Data.Models.Domain;

public sealed class Provider : AuditableEntity
{
    public int ProviderId { get; set; }

    public string Name { get; set; } = null!;

    public string Abbreviation { get; set; } = null!;

    public string? Npi { get; set; }

    public string? EdisonNumber { get; set; }

    public int? BCMSAgencyId { get; set; }

    public bool IsActive { get; set; }

    public ICollection<Contract> Contracts { get; set; } = new List<Contract>();

    public ICollection<F2FAssessment> F2FAssessments { get; set; } = new List<F2FAssessment>();

    public ICollection<PhoneAssessment> PhoneAssessments { get; set; } = new List<PhoneAssessment>();

    public ICollection<ProviderAddress> ProviderAddresses { get; set; } = new List<ProviderAddress>();

    public ICollection<ProviderUser> ProviderUsers { get; set; } = new List<ProviderUser>();

    public ICollection<ServiceFile> ServiceFiles { get; set; } = new List<ServiceFile>();

    public ICollection<Service> Services { get; set; } = new List<Service>();
}

