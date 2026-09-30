using CrisisManagement.Data.Context;
using CrisisManagement.Data.Models.Domain;
using CrisisManagement.Data.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace CrisisManagement.Data.Repositories;

public sealed class NotificationRepository(AppDbContext context) : Repository<AppNotification>(context), INotificationRepository
{
    public Task<List<AppNotification>> GetLatestAsync(int take) =>
        _entities.AsNoTracking().OrderByDescending(n => n.CreatedOn).Take(take).ToListAsync();
}

public sealed class SupportRepository(AppDbContext context) : Repository<AppSupport>(context), ISupportRepository
{
    public Task<List<AppSupport>> GetWithContactsAsync(int take) =>
        _entities.AsNoTracking().Include(s => s.Contact).OrderBy(s => s.AppSupportId).Take(take).ToListAsync();
}
