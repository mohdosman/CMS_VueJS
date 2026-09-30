using CrisisManagement.Data.Context;
using CrisisManagement.Data.Repositories;
using CrisisManagement.Data.Repositories.Interfaces;
using CrisisManagement.Features.Users.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace CrisisManagement.Data;

// Ported from SafetyNet (Data/UnitOfWork/UnitOfWork.cs): repositories are created lazily, one per context.
public class UnitOfWork(AppDbContext context) : IUnitOfWork
{
    private readonly AppDbContext _context = context;

    private IUserRepository? _users;
    private IRoleRepository? _roles;
    private IUserRoleRepository? _userRoles;
    private IProviderRepository? _providers;
    private IProviderUserRepository? _providerUsers;
    private ILogonRepository? _logons;
    private IPasswordChangeLogRepository? _passwordChangeLogs;
    private IFacilityUserRepository? _facilityUsers;
    private IDocumentRepository? _documents;
    private IMenuRepository? _menuItems;
    private IPermissionRepository? _permissions;
    private IPermissionGroupRepository? _permissionGroups;
    private IRoleClaimRepository? _roleClaims;
    private INotificationRepository? _notifications;
    private ISupportRepository? _supports;
    private IStoredProcedureRepository? _storedProcedures;
    private IFileUploadRepository? _fileUploads;
    private IAssessmentRepository? _assessments;

    public IUserRepository Users => _users ??= new UserRepository(_context);
    public IRoleRepository Roles => _roles ??= new RoleRepository(_context);
    public IUserRoleRepository UserRoles => _userRoles ??= new UserRoleRepository(_context);
    public IProviderRepository Providers => _providers ??= new ProviderRepository(_context);
    public IProviderUserRepository ProviderUsers => _providerUsers ??= new ProviderUserRepository(_context);
    public ILogonRepository Logons => _logons ??= new LogonRepository(_context);
    public IPasswordChangeLogRepository PasswordChangeLogs => _passwordChangeLogs ??= new PasswordChangeLogRepository(_context);
    public IFacilityUserRepository FacilityUsers => _facilityUsers ??= new FacilityUserRepository(_context);
    public IDocumentRepository Documents => _documents ??= new DocumentRepository(_context);
    public IMenuRepository MenuItems => _menuItems ??= new MenuRepository(_context);
    public IPermissionRepository Permissions => _permissions ??= new PermissionRepository(_context);
    public IPermissionGroupRepository PermissionGroups => _permissionGroups ??= new PermissionGroupRepository(_context);
    public IRoleClaimRepository RoleClaims => _roleClaims ??= new RoleClaimRepository(_context);
    public INotificationRepository Notifications => _notifications ??= new NotificationRepository(_context);
    public ISupportRepository Supports => _supports ??= new SupportRepository(_context);
    public IStoredProcedureRepository StoredProcedures => _storedProcedures ??= new StoredProcedureRepository(_context);
    public IFileUploadRepository FileUploads => _fileUploads ??= new FileUploadRepository(_context);
    public IAssessmentRepository Assessments => _assessments ??= new AssessmentRepository(_context);

    public void SetCommandTimeout(int seconds) => _context.Database.SetCommandTimeout(seconds);

    public int SaveChanges() => _context.SaveChanges();

    public async Task<int> SaveChangesAsync() => await _context.SaveChangesAsync();

    public Task<IDbContextTransaction> BeginTransactionAsync(CancellationToken ct = default) =>
        _context.Database.BeginTransactionAsync(ct);
}
