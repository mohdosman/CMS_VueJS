using CrisisManagement.Data.Models.Identity;
using CrisisManagement.Data.Repositories.Interfaces;
using CrisisManagement.Features.Menus.ViewModels;

namespace CrisisManagement.Data.Repositories.Interfaces;

public interface IPermissionGroupRepository : IRepository<PermissionGroup>
{
    // Untracked, groups by name with their permissions.
    Task<List<PermissionGroup>> GetAllWithPermissionsAsync();
    Task<PermissionGroup?> GetByNameAsync(string groupName);

    // Menus screen: every group with how many permissions it holds, ordered by name (untracked).
    Task<List<GroupRow>> GetRowsAsync();
    // Tracked, for update.
    Task<PermissionGroup?> GetByIdAsync(int id);
    Task<bool> NameExistsAsync(string name, int exceptId);
}
