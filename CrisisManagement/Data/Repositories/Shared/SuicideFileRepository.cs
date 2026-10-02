using System.Data;
using CrisisManagement.Data.Context;
using CrisisManagement.Data.Repositories.Interfaces;
using CrisisManagement.Features.Suicides.ViewModels;
using CrisisManagement.Shared.Common;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace CrisisManagement.Data.Repositories;

public sealed class SuicideFileRepository(AppDbContext context) : ISuicideFileRepository
{
    public async Task<SuicideFileDownload?> GetFileAsync(int id)
    {
        var f = await context.SuicideFiles.AsNoTracking().Where(x => x.SuicideFileId == id).Select(x => new { x.FileName, x.FileText }).FirstOrDefaultAsync();
        return f is null ? null : new SuicideFileDownload(f.FileName, f.FileText);
    }

    public Task<bool> FileNameExistsAsync(string fileName) => context.SuicideFiles.AsNoTracking().AnyAsync(x => x.FileName == fileName);

    public async Task<PagedResult<SuicideImportItem>> SearchImportsAsync(int fileId, int page, int size, string? sortBy, bool desc)
    {
        var query = context.SuicideFileImports.AsNoTracking().Where(x => x.SuicideFileId == fileId);
        var total = await query.CountAsync();

        query = (sortBy?.ToLowerInvariant(), desc) switch
        {
            ("lastname", true) => query.OrderByDescending(x => x.DNameLast).ThenBy(x => x.SuicideFileImportId),
            ("lastname", false) => query.OrderBy(x => x.DNameLast).ThenBy(x => x.SuicideFileImportId),
            ("firstname", true) => query.OrderByDescending(x => x.DNameFirst).ThenBy(x => x.SuicideFileImportId),
            ("firstname", false) => query.OrderBy(x => x.DNameFirst).ThenBy(x => x.SuicideFileImportId),
            ("ssn", true) => query.OrderByDescending(x => x.DSSN).ThenBy(x => x.SuicideFileImportId),
            ("ssn", false) => query.OrderBy(x => x.DSSN).ThenBy(x => x.SuicideFileImportId),
            _ => query.OrderBy(x => x.SuicideFileImportId)
        };

        var rows = await query.Skip((Math.Max(1, page) - 1) * size).Take(size).ToListAsync();
        return new PagedResult<SuicideImportItem>
        {
            TotalCount = total,
            Items = rows.Select(x => new SuicideImportItem(x.SuicideFileImportId, x.DNameLast, x.DNameFirst, x.DSex, x.DSSN,
                Join(x.DDOBMo, x.DDOBDay, x.DDOBYr), Join(x.DDODMo, x.DDODDay, x.DDODYr),
                x.DDeathStateCountry, x.DResCounty, x.DResStateCountry, x.DDeathManner, x.DUSArmedForces, x.Provider)).ToList()
        };
    }

    private static string? Join(string? month, string? day, string? year) =>
        new[] { month, day, year }.Where(p => !string.IsNullOrWhiteSpace(p)).ToArray() is { Length: > 0 } parts ? string.Join("/", parts) : null;

    public async Task<int> InsertAsync(string fileName, byte[] content, string recordsJson, int recordCount, int userId)
    {
        var id = new SqlParameter("@SuicideFileId", SqlDbType.Int) { Direction = ParameterDirection.Output };
        await context.Database.ExecuteSqlRawAsync(
            "EXEC dbo.usp_CMS_SuicideFileInsert @SuicideFileId = @SuicideFileId OUTPUT, @FileName = @FileName, @FileText = @FileText, " +
            "@FileImportJSON = @FileImportJSON, @FileSize = @FileSize, @RecordCount = @RecordCount, @CreatedBy = @CreatedBy",
            id,
            new SqlParameter("@FileName", SqlDbType.VarChar, 500) { Value = fileName },
            new SqlParameter("@FileText", SqlDbType.VarBinary, -1) { Value = content },
            new SqlParameter("@FileImportJSON", SqlDbType.NVarChar, -1) { Value = recordsJson },
            new SqlParameter("@FileSize", SqlDbType.Int) { Value = content.Length },
            new SqlParameter("@RecordCount", SqlDbType.Int) { Value = recordCount },
            new SqlParameter("@CreatedBy", SqlDbType.Int) { Value = userId });
        return (int)id.Value;
    }
}
