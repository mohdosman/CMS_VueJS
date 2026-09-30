using Microsoft.AspNetCore.Identity;

namespace CMS.Data.Models;

// Stamped by AppDbContext.SaveChanges (same as the Blazor CMS AuditableEntity).
public interface IAudited
{
    int CreatedBy { get; set; }
    int UpdatedBy { get; set; }
    DateTime CreatedOn { get; set; }
    DateTime UpdatedOn { get; set; }
}

// Maps onto the existing RBS_User table.
public sealed class ApplicationUser : IdentityUser<int>, IAudited
{
    // SQL Server datetime cannot hold 0001-01-01; this is its minimum, used for "never happened".
    private static readonly DateTime NeverDate = new(1753, 1, 1);

    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public bool IsADAccount { get; set; }
    public bool IsActive { get; set; }
    public DateTime LastLoginDate { get; set; } = NeverDate;
    public DateTime LastLockoutDate { get; set; } = NeverDate;
    public Guid UserKey { get; set; } = Guid.NewGuid();
    public string? Comment { get; set; }
    public DateTime LastPasswordChangedDate { get; set; } = DateTime.Now;
    public bool IsTemporaryPassword { get; set; }
    public int FailedPasswordAttemptCount { get; set; } // legacy column, Identity uses AccessFailedCount

    // RBS_User.Password: encrypted legacy password, cleared once the user is rehashed to an Identity hash.
    public string LegacyPassword { get; set; } = string.Empty;
    // RBS_User.IsLockedOut: legacy flag, checked only at login.
    public bool LegacyIsLockedOut { get; set; }

    public int CreatedBy { get; set; }
    public int UpdatedBy { get; set; }
    public DateTime CreatedOn { get; set; }
    public DateTime UpdatedOn { get; set; }
    public byte[] Version { get; set; } = [];

    public string FullName => $"{FirstName} {LastName}".Trim() is { Length: > 0 } n ? n : UserName ?? "";
}

public sealed class ApplicationRole : IdentityRole<int> { }

// RBS_UserInRole. Rows are written directly (not through UserManager) so they do not depend on
// RBS_Role.NormalizedName being populated.
public sealed class ApplicationUserRole : IdentityUserRole<int>, IAudited
{
    public int UserInRoleId { get; set; }
    public int CreatedBy { get; set; }
    public int UpdatedBy { get; set; }
    public DateTime CreatedOn { get; set; }
    public DateTime UpdatedOn { get; set; }
}
