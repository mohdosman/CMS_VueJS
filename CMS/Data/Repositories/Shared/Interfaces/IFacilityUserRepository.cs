using CMS.Data.Models.Domain;
using CMS.Data.Repositories.Interfaces;

namespace CMS.Data.Repositories.Interfaces;

public interface IFacilityUserRepository : IRepository<FacilityUser>
{
    Task<bool> AnyForUserAsync(int userId);
}
