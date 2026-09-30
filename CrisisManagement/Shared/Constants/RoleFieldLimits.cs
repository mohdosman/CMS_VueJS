namespace CrisisManagement.Shared.Constants;

/// <summary>
/// Column lengths for a role's fields. The database is the authority, so the EF configuration is
/// built from these too; a validator that allows more than the column holds fails at SQL rather
/// than telling the user which field is too long.
/// </summary>
public static class RoleFieldLimits
{
    /// <summary>RBS_Role.RoleName, nvarchar(256).</summary>
    public const int MaxNameLength = 256;
}
