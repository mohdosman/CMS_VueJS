namespace CMS.Shared.Constants;

/// <summary>
/// Column lengths for the user's own fields. The database is the authority, so the EF configuration
/// is built from these too: a validator that allows more than the column holds fails at SQL with a
/// truncation error instead of a field message.
/// </summary>
public static class UserFieldLimits
{
    /// <summary>RBS_User.FirstName / LastName, nvarchar(256).</summary>
    public const int MaxNameLength = 256;

    /// <summary>RBS_User.Email, varchar(100). Local accounts are capped tighter by the user ID column.</summary>
    public const int MaxEmailLength = 100;
}
