using System.Net;
using System.Text.RegularExpressions;
using CrisisManagement.Data;
using CrisisManagement.Data.Models.Identity;
using CrisisManagement.Features.Menus;
using CrisisManagement.Features.Reports.ViewModels;
using CrisisManagement.Infrastructure.Authorization;
using CrisisManagement.Infrastructure.Security;
using CrisisManagement.Shared.Common;
using CrisisManagement.Shared.Constants;
using CrisisManagement.Shared.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;

namespace CrisisManagement.Features.Reports.Services;

// Report Listing. A report is a Crystal Reports file on the report server. Who may see or run one is decided by two
// permissions per report, reports.view.<slug> and reports.manage.<slug> (manage implies view); the slug is the report name
// reduced to letters, digits, "_" and ".", lower case. Creating a report creates its two permissions and deleting it removes
// them and the role claims that granted them. Running a report never hands the signed token to the browser directly: the
// browser gets a one-use link, bound to the signed-in user, that opens a page posting the token to the report server.
public sealed partial class ReportService(
    IUnitOfWork uow,
    IHttpContextAccessor http,
    IMemoryCache cache,
    IOptions<ReportServerOptions> options,
    ReportTokenIssuer issuer,
    ILogger<ReportService> log)
{
    private const string ViewPrefix = "reports.view", ManagePrefix = "reports.manage", MenuName = "Report Listing", GroupName = "Report Listing Permissions";
    private const string OpenCachePrefix = "report-open:";
    private static readonly TimeSpan OpenLinkLifetime = TimeSpan.FromMinutes(2);
    private static readonly string[] ExportOptions = ["PDF", "CSV", "MSExcel", "TXT"];

    private System.Security.Claims.ClaimsPrincipal User => http.HttpContext!.User;
    private bool IsAdmin => User.IsInRole(AppRoles.Admin);
    private ReportServerOptions Server => options.Value;

    [GeneratedRegex("[^a-zA-Z0-9_.]+")] private static partial Regex SlugChars();
    private static string Slug(string name) => SlugChars().Replace(name, "").ToLowerInvariant();

    private bool Holds(string permission) => IsAdmin || User.HasClaim(AppClaimTypes.Permission, permission);
    private bool CanView(string name) => Holds($"{ViewPrefix}.{Slug(name)}") || Holds($"{ManagePrefix}.{Slug(name)}");
    private bool CanManage(string name) => Holds($"{ManagePrefix}.{Slug(name)}");

    private void Invalidate()
    {
        cache.Remove(PermissionCatalog.CacheKey);
        cache.Remove(MenuService.CacheKey);
    }

    // ---------------------------------------------------------------- list and read

    // Only the reports the caller may view (an administrator sees all).
    public async Task<PagedResult<ReportListItem>> SearchAsync(ReportSearchRequest r)
    {
        var errors = new ErrorBag();
        errors.Optional("reportName", "Report Name", r.ReportName, ReportFieldLimits.MaxReportNameLength);
        errors.Optional("description", "Description", r.Description, ReportFieldLimits.MaxDescriptionLength);
        errors.ThrowIfAny();

        var visible = (await uow.Reports.SearchAsync(r.ReportName, r.Description)).Where(x => CanView(x.ReportName));
        visible = (r.SortBy?.ToLowerInvariant(), r.SortDesc) switch
        {
            ("description", false) => visible.OrderBy(x => x.Description),
            ("description", true) => visible.OrderByDescending(x => x.Description),
            ("exportoption", false) => visible.OrderBy(x => x.ExportOption),
            ("exportoption", true) => visible.OrderByDescending(x => x.ExportOption),
            (_, true) => visible.OrderByDescending(x => x.ReportName),
            _ => visible.OrderBy(x => x.ReportName)
        };
        var all = visible.ToList();
        var size = Math.Clamp(r.PageSize, 1, 200);
        return new PagedResult<ReportListItem>
        {
            TotalCount = all.Count,
            Items = all.Skip((Math.Max(1, r.PageIndex) - 1) * size).Take(size)
                .Select(x => new ReportListItem(x.ReportId, x.ReportKey, x.ReportName, x.FileName, x.Description, x.ExportOption, x.UpdatedOn)).ToList()
        };
    }

    // Report files on the server that are not defined yet.
    public async Task<List<string>> GetAvailableAsync()
    {
        var path = Server.ReportFilesPath;
        if (string.IsNullOrWhiteSpace(path) || !Directory.Exists(path))
        {
            log.LogWarning("The report files folder '{Path}' is not available", path);
            return [];
        }
        var used = (await uow.Reports.GetNamesAsync()).Select(x => x.Name).ToHashSet(StringComparer.OrdinalIgnoreCase);
        return Directory.EnumerateFiles(path, "*.rpt").Select(f => Path.GetFileNameWithoutExtension(f))
            .Where(n => n.Length is > 0 and <= ReportFieldLimits.MaxReportNameLength && !used.Contains(n)).Order(StringComparer.OrdinalIgnoreCase).ToList();
    }

    // Null = no such report (or one the caller may not view: the two look the same).
    public async Task<ReportEditModel?> GetAsync(int id)
    {
        var report = await uow.Reports.GetAsync(id);
        return report is not null && CanView(report.ReportName) ? ToModel(report) : null;
    }

    private static ReportEditModel ToModel(Report r) => new()
    {
        Id = r.ReportId, RowVersion = Convert.ToBase64String(r.Version), ReportKey = r.ReportKey, ReportName = r.ReportName, FileName = r.FileName,
        Description = r.Description, ExportOption = r.ExportOption, CreatedOn = r.CreatedOn, UpdatedOn = r.UpdatedOn
    };

    // ---------------------------------------------------------------- change

    // The report and its two permissions are saved together.
    public async Task<ReportEditModel> CreateAsync(ReportEditModel m)
    {
        var errors = new ErrorBag();
        var name = errors.Required("reportName", "Report name", m.ReportName, ReportFieldLimits.MaxReportNameLength);
        ValidateDetails(m, errors);
        errors.ThrowIfAny();

        // The file has to be one the report server has, so a report cannot be defined for an arbitrary file name.
        if (!(await GetAvailableAsync()).Contains(name, StringComparer.OrdinalIgnoreCase))
            errors.Add("reportName", "Report name must be one of the available report files.");
        var slug = Slug(name);
        if (slug.Length == 0) errors.Add("reportName", "Report name must contain letters or digits.");
        var existing = await uow.Reports.GetNamesAsync();
        if (existing.Any(x => x.Name.Equals(name, StringComparison.OrdinalIgnoreCase))) errors.Add("reportName", "Report name already exists.");
        else if (existing.Any(x => Slug(x.Name) == slug)) errors.Add("reportName", "Another report has a name that gives the same permission.");
        var fileName = name + ".rpt";
        if (await uow.Reports.FileNameExistsAsync(fileName, 0)) errors.Add("fileName", "File name already exists.");
        errors.ThrowIfAny();

        var menu = await uow.MenuItems.GetByNameAsync(MenuName);
        var group = await uow.PermissionGroups.GetByNameAsync(GroupName);
        if (menu is null || group is null) throw new InvalidOperationException($"The '{MenuName}' menu or its '{GroupName}' permission group is missing.");

        var report = new Report { ReportName = name, FileName = fileName, Description = m.Description!.Trim(), ExportOption = m.ExportOption!.Trim() };
        uow.Reports.Add(report);
        foreach (var (prefix, label, verb) in new[] { (ViewPrefix, "View", "view"), (ManagePrefix, "Manage", "manage") })
            uow.Permissions.Add(new Permission
            {
                Name = $"{label}:{name}", Value = $"{prefix}.{slug}", Description = $"Permission to {verb} [{name}] report",
                PermissionGroupNameId = group.PermissionGroupId, MenuItemId = menu.MenuItemId
            });
        await uow.SaveChangesAsync();
        Invalidate();
        return ToModel(report);
    }

    // Only the description and the export option can change. Null = no such report.
    public async Task<ReportEditModel?> UpdateAsync(int id, ReportEditModel m)
    {
        var report = await uow.Reports.GetAsync(id);
        if (report is null) return null;
        RequireManage(report);

        var errors = new ErrorBag();
        ValidateDetails(m, errors);
        errors.ThrowIfAny();
        if (!string.IsNullOrEmpty(m.RowVersion) && !report.Version.AsSpan().SequenceEqual(Convert.FromBase64String(m.RowVersion)))
            throw new ConflictException("This report was changed by someone else. Reload the page and try again.");

        report.Description = m.Description!.Trim();
        report.ExportOption = m.ExportOption!.Trim();
        await uow.SaveChangesAsync();
        return ToModel(report);
    }

    // The permissions and the role claims that granted them go with the report; the log of past runs stays.
    public async Task<bool> DeleteAsync(int id)
    {
        var report = await uow.Reports.GetAsync(id);
        if (report is null) return false;
        RequireManage(report);

        var slug = Slug(report.ReportName);
        foreach (var value in new[] { $"{ViewPrefix}.{slug}", $"{ManagePrefix}.{slug}" })
        {
            if (await uow.Permissions.GetByValueAsync(value) is { } permission) uow.Permissions.Remove(permission);
            uow.RoleClaims.RemoveRange(await uow.RoleClaims.GetPermissionClaimsAsync(value: value));
        }
        uow.Reports.Remove(report);
        await uow.SaveChangesAsync();
        Invalidate();
        return true;
    }

    // A caller with reports.edit may still only change the reports they manage.
    private void RequireManage(Report report)
    {
        if (!CanManage(report.ReportName)) throw new ForbiddenAccessException("You do not have permission to manage this report.");
    }

    private static void ValidateDetails(ReportEditModel m, ErrorBag e)
    {
        e.Required("description", "Description", m.Description, ReportFieldLimits.MaxDescriptionLength);
        var option = m.ExportOption?.Trim() ?? "";
        if (option.Length == 0) e.Add("exportOption", "Export option is required");
        else if (!ExportOptions.Contains(option)) e.Add("exportOption", $"Export option must be one of: {string.Join(", ", ExportOptions)}");
    }

    // ---------------------------------------------------------------- run

    // Signs a token for the report, logs the run and returns the one-use link the browser window opens.
    public async Task<ReportRun> RunAsync(Guid reportKey, string openUrlPrefix)
    {
        var report = await uow.Reports.GetByKeyAsync(reportKey);
        if (report is null || !CanView(report.ReportName)) throw new ForbiddenAccessException("You do not have permission to run this report.");
        if (string.IsNullOrWhiteSpace(Server.ReportServerUrl)) throw new InvalidOperationException("ReportServer:ReportServerUrl is not configured.");

        var userId = User.GetUserId();
        var provider = await ScopedProviderNameAsync(userId);
        var userData = provider.Length == 0 ? report.ExportOption : $"{report.ExportOption}|{provider}";
        var token = issuer.Issue(User.Identity?.Name ?? "Unknown", userData, report.FileName);

        uow.Reports.AddLog(new ReportLog { ReportId = report.ReportId, ReportName = report.FileName, SecurityToken = token });
        await uow.SaveChangesAsync();

        var linkId = Guid.NewGuid().ToString("N");
        var expires = DateTime.UtcNow.Add(OpenLinkLifetime);
        cache.Set(OpenCachePrefix + linkId, new OpenLink(userId, token, Server.ReportEnvironment, report.FileName), OpenLinkLifetime);
        log.LogInformation("User {UserId} ran report '{Report}'", userId, report.ReportName);
        return new ReportRun(openUrlPrefix + linkId, expires);
    }

    // The provider name a scoped user's report is limited to; empty for everyone else. Fails closed: a scoped user whose
    // provider cannot be found is refused, because the report server gives an unrestricted provider list to anyone who
    // arrives without a name.
    private async Task<string> ScopedProviderNameAsync(int userId)
    {
        if (!Server.ProviderScopedRoles.Any(User.IsInRole)) return "";
        var name = (await uow.ProviderUsers.GetProvidersForUserAsync(userId)).Select(p => p.Name).FirstOrDefault();
        if (string.IsNullOrWhiteSpace(name))
        {
            log.LogWarning("User {UserId} holds a provider-scoped role but has no provider; the report was refused", userId);
            throw new ForbiddenAccessException("Your account is restricted to a provider, but no provider is associated with it. Contact an administrator.");
        }
        return name;
    }

    // The page that posts the token to the report server; null when the link is unknown, used, expired or another user's.
    public string? ConsumeOpenLink(string linkId)
    {
        if (!cache.TryGetValue(OpenCachePrefix + linkId, out OpenLink? link) || link is null) return null;
        cache.Remove(OpenCachePrefix + linkId);   // one use
        if (link.UserId != User.GetUserId()) return null;

        string H(string v) => WebUtility.HtmlEncode(v);
        return $$"""
            <!DOCTYPE html>
            <html lang="en">
            <head><meta charset="utf-8"><title>Opening Report...</title></head>
            <body onload="document.getElementById('reportForm').submit()" style="font-family: Arial, sans-serif; text-align: center; margin-top: 4rem">
              <h2>Opening Report...</h2>
              <p>Please wait while your report is being loaded.</p>
              <form id="reportForm" method="post" action="{{H(Server.ReportServerUrl.TrimEnd('/'))}}/Default.aspx">
                <input type="hidden" name="token" value="{{H(link.Token)}}" />
                <input type="hidden" name="system" value="{{H(link.System)}}" />
                <input type="hidden" name="id" value="{{H(link.Report)}}" />
                <noscript><button type="submit">Open the report</button></noscript>
              </form>
            </body>
            </html>
            """;
    }

    private sealed record OpenLink(int UserId, string Token, string System, string Report);
}
