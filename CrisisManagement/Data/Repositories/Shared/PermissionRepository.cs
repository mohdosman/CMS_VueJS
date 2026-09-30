using CrisisManagement.Data.Context;
using CrisisManagement.Data.Models.Domain;
using CrisisManagement.Data.Models.Identity;
using CrisisManagement.Data.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace CrisisManagement.Data.Repositories;

public sealed class PermissionRepository(AppDbContext context) : Repository<Permission>(context), IPermissionRepository
{
    public Task<List<string>> GetAllValuesAsync() => _entities.AsNoTracking().Select(p => p.Value).ToListAsync();

    public Task<Permission?> GetByValueAsync(string value) => _entities.FirstOrDefaultAsync(p => p.Value == value);
}
