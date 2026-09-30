using System;
using System.Collections.Generic;

namespace CMS.Data.Models.Domain;

public sealed class MaritalStatus : AuditableEntity
{
    public int MaritalStatusId { get; set; }

    public string NOMSId { get; set; } = null!;

    public string MaritalStatusCode { get; set; } = null!;

    public string MaritalStatusDescription { get; set; } = null!;

    public ICollection<F2FAssessment> F2FAssessments { get; set; } = new List<F2FAssessment>();
}

