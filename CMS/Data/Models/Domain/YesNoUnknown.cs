using System;
using System.Collections.Generic;

namespace CMS.Data.Models.Domain;

public sealed class YesNoUnknown : AuditableEntity
{
    public byte YesNoUnknownId { get; set; }

    public string Response { get; set; } = null!;

    public string Abbreviation { get; set; } = null!;

    public short NumericValue { get; set; }

    public ICollection<F2FAssessment> F2FAssessmentDurablePOAs { get; set; } = new List<F2FAssessment>();

    public ICollection<F2FAssessment> F2FAssessmentFirstHospitalizations { get; set; } = new List<F2FAssessment>();

    public ICollection<F2FAssessment> F2FAssessmentIntellectualDisabilities { get; set; } = new List<F2FAssessment>();

    public ICollection<F2FAssessment> F2FAssessmentMHTreatmentDeclarations { get; set; } = new List<F2FAssessment>();

    public ICollection<F2FAssessment> F2FAssessmentMOTStatuses { get; set; } = new List<F2FAssessment>();

    public ICollection<F2FAssessment> F2FAssessmentMedicalInstabilities { get; set; } = new List<F2FAssessment>();

    public ICollection<F2FAssessment> F2FAssessmentMedicationIssues { get; set; } = new List<F2FAssessment>();

    public ICollection<F2FAssessment> F2FAssessmentPastTraumas { get; set; } = new List<F2FAssessment>();

    public ICollection<F2FAssessment> F2FAssessmentSchool3Months { get; set; } = new List<F2FAssessment>();

    public ICollection<F2FAssessment> F2FAssessmentSubstanceAbuses { get; set; } = new List<F2FAssessment>();
}

