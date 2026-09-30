using CrisisManagement.Data.Models.Domain;
using CrisisManagement.Data.Models.Identity;
using CrisisManagement.Data.Repositories.Interfaces;

namespace CrisisManagement.Data.Repositories.Interfaces;

public interface IPermissionRepository : IRepository<Permission>
{
    Task<List<string>> GetAllValuesAsync();
    // Tracked, for rename.
    Task<Permission?> GetByValueAsync(string value);

    // Menus screen: a menu's permissions with their group, ordered by group then value (untracked).
    Task<List<Permission>> GetForMenuAsync(int menuId);
    // Tracked, for update and delete.
    Task<Permission?> GetByIdAsync(int id);
    Task<bool> ValueExistsAsync(string value, int exceptId);
}
