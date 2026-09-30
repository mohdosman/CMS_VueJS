namespace CrisisManagement.Features.Users.ViewModels;

public enum YesNoFilter { All = 0, Yes = 1, No = 2 }

public sealed class UserSearchRequest
{
    public string? UserName { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? Email { get; set; }
    public List<int> RoleIds { get; set; } = [];
    public List<int> ProviderIds { get; set; } = [];
    public YesNoFilter IsEnabled { get; set; } = YesNoFilter.Yes;
    public YesNoFilter IsADAccount { get; set; } = YesNoFilter.All;
    public YesNoFilter IsLockedOut { get; set; } = YesNoFilter.All;
    public int PageIndex { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    public string? SortBy { get; set; }
    public bool SortDesc { get; set; }
}

public sealed class UserListItem
{
    public Guid UserKey { get; set; }
    public string UserName { get; set; } = "";
    public string FirstName { get; set; } = "";
    public string LastName { get; set; } = "";
    public string Email { get; set; } = "";
    public bool IsEnabled { get; set; }
    public bool IsADAccount { get; set; }
    public bool IsLockedOut { get; set; }
}

public sealed class PagedResult<T>
{
    public IReadOnlyList<T> Items { get; init; } = [];
    public int TotalCount { get; init; }
}

public sealed record UserDocumentItem(int DocumentId, string FileName, DateTime CreatedOn, long FileSize);

public sealed record LookupItem(int Id, string Label, string? Short = null);

public sealed class UserDetail
{
    public Guid UserKey { get; set; }
    public string RowVersion { get; set; } = "";   // base64, echoed back on save for optimistic concurrency
    public string UserName { get; set; } = "";
    public string FirstName { get; set; } = "";
    public string LastName { get; set; } = "";
    public string Email { get; set; } = "";
    public string? PhoneNumber { get; set; }
    public string Notes { get; set; } = "";
    public bool IsEnabled { get; set; }
    public bool IsADAccount { get; set; }
    public bool IsLockedOut { get; set; }
    public bool TwoFactorEnabled { get; set; }
    public DateTime? LastLoginAt { get; set; }
    public DateTime LastPasswordChangedDate { get; set; }
    public DateTime CreatedOn { get; set; }
    public DateTime UpdatedOn { get; set; }
    public IReadOnlyList<string> Roles { get; set; } = [];
    public IReadOnlyList<int> RoleIds { get; set; } = [];
    public IReadOnlyList<string> Providers { get; set; } = [];
    public IReadOnlyList<int> ProviderIds { get; set; } = [];
}

// Body of POST (create) and PUT (update). UserName is only used for a new AD account; a local
// account signs in with its email.
public sealed class UserEditRequest
{
    public string? RowVersion { get; set; }
    public string? UserName { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? Email { get; set; }
    public string? PhoneNumber { get; set; }
    public string? Password { get; set; }
    public string? ConfirmPassword { get; set; }
    public string? Notes { get; set; }
    public bool IsEnabled { get; set; } = true;
    public bool IsADAccount { get; set; }
    public bool TwoFactorEnabled { get; set; }
    public List<int> RoleIds { get; set; } = [];
    public List<int> ProviderIds { get; set; } = [];
}

public sealed class SetPasswordRequest
{
    public string? Password { get; set; }
    public string? ConfirmPassword { get; set; }
}

// What the form needs to describe the rules the server enforces.
// Password rules as data, so the screen can tick them off while the user types; PasswordRules is the same text as a list.
public sealed record PasswordRequirements(int MinLength, bool RequireLowercase, bool RequireUppercase, bool RequireDigit, bool RequireSpecial, int UniqueChars);

public sealed record UserPolicy(string[] PasswordRules, string AdUserNameRule, PasswordRequirements Password);

public sealed class UserNamePolicyOptions
{
    public int MinLength { get; set; } = 7;
    public int MaxLength { get; set; } = 15;
    public string[] AdAccountPrefixes { get; set; } = ["ci", "ag"];
}
