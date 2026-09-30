using CrisisManagement.Data.Models.Identity;
using CrisisManagement.Data.Repositories.Interfaces;

namespace CrisisManagement.Data.Repositories.Interfaces;

public interface IPermissionGroupRepository : IRepository<PermissionGroup>
{
    // Untracked, groups by name with their permissions.
    Task<List<PermissionGroup>> GetAllWithPermissionsAsync();
    Task<PermissionGroup?> GetByNameAsync(string groupName);
}
