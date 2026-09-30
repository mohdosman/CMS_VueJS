using CMS.Data.Models.Domain;
using CMS.Data.Models.Identity;
using CMS.Data.Repositories.Interfaces;

namespace CMS.Data.Repositories.Interfaces;

public interface IPermissionRepository : IRepository<Permission>
{
    Task<List<string>> GetAllValuesAsync();
    // Tracked, for rename.
    Task<Permission?> GetByValueAsync(string value);
}
