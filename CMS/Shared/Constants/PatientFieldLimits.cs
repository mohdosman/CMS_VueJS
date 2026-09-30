namespace CMS.Shared.Constants;

/// <summary>
/// Column lengths for CMS_Patient, which both the assessment and the service screens write to.
/// </summary>
public static class PatientFieldLimits
{
    /// <summary>CMS_Patient.FirstName / LastName, both varchar(150).</summary>
    public const int MaxNameLength = 150;

    /// <summary>CMS_Patient.ProviderPatientNo, varchar(50).</summary>
    public const int MaxProviderPatientNoLength = 50;
}
