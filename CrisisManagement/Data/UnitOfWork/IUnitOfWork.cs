using CrisisManagement.Data.Repositories.Interfaces;
using CrisisManagement.Features.Users.Repositories;
using Microsoft.EntityFrameworkCore.Storage;

namespace CrisisManagement.Data;

// Ported from SafetyNet (Data/UnitOfWork/IUnitOfWork.cs). Services reach data only through this:
// one property per repository. Add a repository here as each feature is moved onto the pattern.
public interface IUnitOfWork
{
    IUserRepository Users { get; }
    IRoleRepository Roles { get; }
    IUserRoleRepository UserRoles { get; }
    IProviderRepository Providers { get; }
    IProviderUserRepository ProviderUsers { get; }
    ILogonRepository Logons { get; }
    IPasswordChangeLogRepository PasswordChangeLogs { get; }
    IFacilityUserRepository FacilityUsers { get; }
    IDocumentRepository Documents { get; }
    IMenuRepository MenuItems { get; }
    IPermissionRepository Permissions { get; }
    IPermissionGroupRepository PermissionGroups { get; }
    IRoleClaimRepository RoleClaims { get; }
    INotificationRepository Notifications { get; }
    ISupportRepository Supports { get; }

    void SetCommandTimeout(int seconds);
    int SaveChanges();
    Task<int> SaveChangesAsync();

    // Not in SafetyNet's interface: the user screens change several tables at once and need them atomic.
    Task<IDbContextTransaction> BeginTransactionAsync(CancellationToken ct = default);
}
