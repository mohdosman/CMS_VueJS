namespace CrisisManagement.Shared.Constants;

/// <summary>
/// Column lengths for a service. Verified against INFORMATION_SCHEMA rather than the EF mapping.
/// The patient fields a service writes live in <c>PatientFieldLimits</c>, since CMS_Patient is
/// shared with the assessment screens.
/// </summary>
public static class ServiceFieldLimits
{
    /// <summary>CMS_Service.SessionId, varchar(250).</summary>
    public const int MaxSessionIdLength = 250;
}
