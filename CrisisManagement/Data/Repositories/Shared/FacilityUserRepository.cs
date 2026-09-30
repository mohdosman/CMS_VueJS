using CrisisManagement.Data.Context;
using CrisisManagement.Data.Models.Domain;
using CrisisManagement.Data.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace CrisisManagement.Data.Repositories;

public sealed class FacilityUserRepository(AppDbContext context) : Repository<FacilityUser>(context), IFacilityUserRepository
{
    public Task<bool> AnyForUserAsync(int userId) => _entities.AnyAsync(f => f.UserId == userId);
}
