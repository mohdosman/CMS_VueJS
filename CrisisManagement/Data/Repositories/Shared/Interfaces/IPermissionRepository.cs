using CrisisManagement.Data.Models.Domain;
using CrisisManagement.Data.Models.Identity;
using CrisisManagement.Data.Repositories.Interfaces;

namespace CrisisManagement.Data.Repositories.Interfaces;

public interface IPermissionRepository : IRepository<Permission>
{
    Task<List<string>> GetAllValuesAsync();
    // Tracked, for rename.
    Task<Permission?> GetByValueAsync(string value);
}
