using CMS.Data.Models.Identity;
using CMS.Data.Repositories.Interfaces;

namespace CMS.Data.Repositories.Interfaces;

public interface IPermissionGroupRepository : IRepository<PermissionGroup>
{
    // Untracked, groups by name with their permissions.
    Task<List<PermissionGroup>> GetAllWithPermissionsAsync();
    Task<PermissionGroup?> GetByNameAsync(string groupName);
}
