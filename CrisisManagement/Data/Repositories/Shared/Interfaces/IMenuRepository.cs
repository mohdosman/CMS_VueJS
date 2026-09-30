using CrisisManagement.Data.Models.Domain;
using CrisisManagement.Data.Models.Identity;
using CrisisManagement.Data.Repositories.Interfaces;

namespace CrisisManagement.Data.Repositories.Interfaces;

public interface IMenuRepository : IRepository<MenuItem>
{
    Task<List<MenuItem>> GetEnabledWithPermissionsAsync();
    // Untracked, every item: enough to order permissions the way the menu is laid out.
    Task<List<MenuItem>> GetAllAsync();
    Task<MenuItem?> GetByNameAsync(string menuItemName);
    // Tracked, with its permissions, for update and delete.
    Task<MenuItem?> GetTrackedAsync(int id);
    Task<bool> NameExistsAsync(string name, int exceptId);
}
