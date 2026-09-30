using System;
using System.Collections.Generic;

namespace CMS.Data.Models.Domain;

public sealed class FileUploadF2FDrug : AuditableEntity
{
    public int F2FDrugId { get; set; }

    public int F2FAssessmentId { get; set; }

    public int DrugId { get; set; }

    public int? DrugRouteId { get; set; }

    public int? DrugFrequencyId { get; set; }

}

