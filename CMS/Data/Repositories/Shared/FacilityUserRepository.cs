using CMS.Data.Context;
using CMS.Data.Models.Domain;
using CMS.Data.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace CMS.Data.Repositories;

public sealed class FacilityUserRepository(AppDbContext context) : Repository<FacilityUser>(context), IFacilityUserRepository
{
    public Task<bool> AnyForUserAsync(int userId) => _entities.AnyAsync(f => f.UserId == userId);
}
