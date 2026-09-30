using System;
using System.Collections.Generic;

namespace CMS.Data.Models.Domain;

public sealed class PayorSource : AuditableEntity
{
    public int PayorSourceId { get; set; }

    public string Abbreviation { get; set; } = null!;

    public string PayorSourceDescription { get; set; } = null!;

    public ICollection<F2FAssessment> F2FAssessmentPayorSources { get; set; } = new List<F2FAssessment>();

    public ICollection<F2FAssessment> F2FAssessmentSecondaryPayorSources { get; set; } = new List<F2FAssessment>();

    public ICollection<Service> ServicePayorSources { get; set; } = new List<Service>();

    public ICollection<Service> ServicePrimaryInsurers { get; set; } = new List<Service>();
}

