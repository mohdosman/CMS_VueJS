namespace CMS.Shared.Constants;

/// <summary>
/// Column lengths for the assessment's free-text fields. The database is the authority, so the EF
/// configuration is built from these too: a validator that allows more than the column holds fails
/// at SQL with a truncation error instead of a message on the field.
/// </summary>
public static class AssessmentFieldLimits
{
    /// <summary>CMS_F2FAssessment.CompletedByFirstName / CompletedByLastName, varchar(250).</summary>
    public const int MaxCompletedByNameLength = 250;

    /// <summary>CMS_PhoneAssessment.DispositionOther, varchar(150).</summary>
    public const int MaxDispositionOtherLength = 150;

    /// <summary>
    /// ContactAttempts, NumberInHousehold and Arrests30Days are all byte columns; without a bound
    /// the cast on save silently wraps.
    /// </summary>
    public const int MaxByteColumnValue = byte.MaxValue;
}
