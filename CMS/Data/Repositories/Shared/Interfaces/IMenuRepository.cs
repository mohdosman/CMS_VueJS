using CMS.Data.Models.Domain;
using CMS.Data.Models.Identity;
using CMS.Data.Repositories.Interfaces;

namespace CMS.Data.Repositories.Interfaces;

public interface IMenuRepository : IRepository<MenuItem>
{
    Task<List<MenuItem>> GetEnabledWithPermissionsAsync();
    // Untracked, every item: enough to order permissions the way the menu is laid out.
    Task<List<MenuItem>> GetAllAsync();
    Task<MenuItem?> GetByNameAsync(string menuItemName);
}
