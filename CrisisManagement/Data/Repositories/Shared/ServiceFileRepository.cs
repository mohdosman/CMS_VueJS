using System.Text;
using CrisisManagement.Data.Context;
using CrisisManagement.Data.Models.Domain;
using CrisisManagement.Data.Repositories.Interfaces;
using CrisisManagement.Features.Services.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace CrisisManagement.Data.Repositories;

public sealed class ServiceFileRepository(AppDbContext context) : Repository<ServiceFile>(context), IServiceFileRepository
{
    public Task<bool> PendingExistsAsync(string fileName, int providerId) =>
        context.ServiceFiles.AnyAsync(f => f.FileName == fileName && f.ProviderId == providerId && !f.IsProcessed);

    public Task<int?> GetProviderIdAsync(int id) =>
        context.ServiceFiles.AsNoTracking().Where(f => f.ServiceFileId == id).Select(f => (int?)f.ProviderId).FirstOrDefaultAsync();

    public async Task<ServiceFileRaw?> GetRawAsync(int id)
    {
        var f = await context.ServiceFiles.AsNoTracking().Where(x => x.ServiceFileId == id).Select(x => new { x.ServiceFileId, x.FileName, x.FileText }).FirstOrDefaultAsync();
        return f is null ? null : new ServiceFileRaw(f.ServiceFileId, f.FileName, Encoding.UTF8.GetString(f.FileText));
    }
}
