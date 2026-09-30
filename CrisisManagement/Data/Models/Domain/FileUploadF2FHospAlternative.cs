using System;
using System.Collections.Generic;

namespace CrisisManagement.Data.Models.Domain;

public sealed class FileUploadF2FHospAlternative : AuditableEntity
{
    public int F2FHospAlternativeId { get; set; }

    public int F2FAssessmentId { get; set; }

    public int HospitalizationAlternativeId { get; set; }

    public int HospAltDispositionId { get; set; }

}

