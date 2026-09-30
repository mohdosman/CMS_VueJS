namespace CMS.Data.Models;

// CMS_Provider: read-only here. CMS_ProviderUser: users are assigned to providers by editing them.
public sealed class Provider
{
    public int ProviderId { get; set; }
    public string Name { get; set; } = "";
    public string? Abbreviation { get; set; }
}

public sealed class ProviderUser : IAudited
{
    public int ProviderUserId { get; set; }
    public int ProviderId { get; set; }
    public int UserId { get; set; }
    public int CreatedBy { get; set; }
    public int UpdatedBy { get; set; }
    public DateTime CreatedOn { get; set; }
    public DateTime UpdatedOn { get; set; }
}

public sealed class Logon
{
    public int LogonId { get; set; }
    public int UserId { get; set; }
    public DateTime LogOnDateTime { get; set; }
}

// RBS_PasswordChangeLog: hashes of the last passwords an admin set, for the reuse check.
public sealed class PasswordChangeLog : IAudited
{
    public int PasswordChangeLogId { get; set; }
    public int UserId { get; set; }
    public string Password { get; set; } = "";   // legacy NOT NULL column, unused
    public string? PasswordHash { get; set; }
    public int CreatedBy { get; set; }
    public int UpdatedBy { get; set; }
    public DateTime CreatedOn { get; set; }
    public DateTime UpdatedOn { get; set; }
}
