using CMS.Data.Context;
using CMS.Features.PublicFiles.Repositories;

namespace CMS.Data.UnitOfWork;

public sealed class UnitOfWork(AppDbContext context) : IUnitOfWork
{
    private IPublicFilesRepository? _publicFiles;

    public IPublicFilesRepository PublicFiles => _publicFiles ??= new PublicFilesRepository(context);

    public Task<int> SaveChangesAsync(CancellationToken ct = default) => context.SaveChangesAsync(ct);
}
