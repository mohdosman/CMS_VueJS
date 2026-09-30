using System;
using System.Collections.Generic;

namespace CMS.Data.Models.Domain;

public sealed class FileUploadF2FHospitalization : AuditableEntity
{
    public int F2FHospitalizationId { get; set; }

    public int F2FAssessmentId { get; set; }

    public int HospitalizationId { get; set; }

    public int HospitalizationDispositionId { get; set; }

}

