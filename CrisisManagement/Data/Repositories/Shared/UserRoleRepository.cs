using CrisisManagement.Data.Context;
using CrisisManagement.Data.Models.Identity;
using CrisisManagement.Data.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace CrisisManagement.Data.Repositories;

public sealed class UserRoleRepository(AppDbContext context) : Repository<ApplicationUserRole>(context), IUserRoleRepository
{
    private readonly AppDbContext _db = context;

    public Task<List<IdName>> GetRolesForUserAsync(int userId) =>
        (from ur in _entities.AsNoTracking()
         join r in _db.Roles on ur.RoleId equals r.Id
         where ur.UserId == userId
         orderby r.Name
         select new IdName(r.Id, r.Name!)).ToListAsync();

    public Task<int> CountForRoleAsync(int roleId) => _entities.CountAsync(x => x.RoleId == roleId);

    public Task<List<ApplicationUserRole>> GetForUserAsync(int userId) =>
        _entities.Where(x => x.UserId == userId).ToListAsync();
}
