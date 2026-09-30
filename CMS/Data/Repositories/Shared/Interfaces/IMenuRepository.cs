using CMS.Data.Models.Domain;
using CMS.Data.Models.Identity;
using CMS.Data.Repositories.Interfaces;

namespace CMS.Data.Repositories.Interfaces;

public interface IMenuRepository : IRepository<MenuItem>
{
    Task<List<MenuItem>> GetEnabledWithPermissionsAsync();
}
