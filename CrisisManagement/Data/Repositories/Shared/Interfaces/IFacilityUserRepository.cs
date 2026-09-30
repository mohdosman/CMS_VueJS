using CrisisManagement.Data.Models.Domain;
using CrisisManagement.Data.Repositories.Interfaces;

namespace CrisisManagement.Data.Repositories.Interfaces;

public interface IFacilityUserRepository : IRepository<FacilityUser>
{
    Task<bool> AnyForUserAsync(int userId);
}
