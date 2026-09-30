using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using CrisisManagement.Data;
using CrisisManagement.Data.Models.Domain;
using CrisisManagement.Features.Providers.ViewModels;
using CrisisManagement.Shared.Common;
using CrisisManagement.Shared.Constants;

namespace CrisisManagement.Features.Providers.Services;

// Port of the Blazor CMS ProviderService. Non-administrators are scoped to the providers in their provider_ids
// claim on every read and write, and cannot add providers. Data access goes through the unit of work.
public sealed class ProviderService(IUnitOfWork uow, IHttpContextAccessor http)
{
    // CMS_AddressType ids, fixed as in the legacy ManageProvider page. Descriptions vary between databases, so
    // matching on them finds nothing on some and fails every address save.
    private const short PhysicalAddressTypeId = 1, RemitAddressTypeId = 2;

    private ClaimsPrincipal Principal => http.HttpContext!.User;
    private bool IsAdmin => Principal.IsInRole(AppRoles.Admin);

    // null = unrestricted; otherwise the provider ids the caller may see (possibly none).
    private int[]? AllowedIds() => IsAdmin ? null :
        (Principal.FindFirstValue(AppClaimTypes.ProviderIds) ?? "")
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(v => int.TryParse(v, out var id) ? id : 0).Where(id => id > 0).ToArray();

    private bool InScope(int providerId) => AllowedIds() is not { } ids || ids.Contains(providerId);

    // ---------------------------------------------------------------- read

    public Task<PagedResult<ProviderListItem>> SearchAsync(ProviderSearchRequest req) =>
        uow.Providers.SearchAsync(req, AllowedIds());

    public async Task<ProviderLookups> GetLookupsAsync() =>
        new(await uow.Providers.GetStatesAsync(), await uow.Providers.GetCountiesAsync());

    // Detail null + Allowed true = not found; Allowed false = outside the caller provider scope.
    public async Task<(ProviderDetail? Detail, bool Allowed)> GetAsync(int id)
    {
        if (!InScope(id)) return (null, false);
        var p = await uow.Providers.GetAggregateAsync(id);
        return (p is null ? null : ToDetail(p), true);
    }

    // ---------------------------------------------------------------- write

    public async Task<ProviderDetail> CreateAsync(ProviderEditRequest r)
    {
        if (!IsAdmin) throw new ForbiddenAccessException("Only administrators can add providers.");

        var errors = Validate(r);
        await CheckUniqueAsync(r, 0, errors);
        Throw(errors);

        var p = new Provider { Name = r.Name!.Trim(), Abbreviation = r.Abbreviation!.Trim(), Npi = Norm(r.Npi), EdisonNumber = Norm(r.EdisonNumber), IsActive = true };
        SyncAddress(p, r.PhysicalAddress, PhysicalAddressTypeId);
        SyncAddress(p, r.RemitAddress, RemitAddressTypeId);
        uow.Providers.Add(p);
        await uow.SaveChangesAsync();

        return ToDetail((await uow.Providers.GetAggregateAsync(p.ProviderId))!);
    }

    // Null = provider not found.
    public async Task<ProviderDetail?> UpdateAsync(int id, ProviderEditRequest r)
    {
        if (!InScope(id)) throw new ForbiddenAccessException("You can only manage providers assigned to you.");
        var p = await uow.Providers.GetAggregateAsync(id);
        if (p is null) return null;

        var errors = Validate(r);
        await CheckUniqueAsync(r, id, errors);
        Throw(errors);

        // Compared here rather than via the tracked OriginalValue, which reloads overwrite.
        if (Has(r.RowVersion) && !p.Version.AsSpan().SequenceEqual(Convert.FromBase64String(r.RowVersion!)))
            throw new ConflictException("This provider was changed by someone else. Reload the page and try again.");

        p.Name = r.Name!.Trim();
        p.Abbreviation = r.Abbreviation!.Trim();
        p.Npi = Norm(r.Npi);
        p.EdisonNumber = Norm(r.EdisonNumber);
        SyncAddress(p, r.PhysicalAddress, PhysicalAddressTypeId);
        SyncAddress(p, r.RemitAddress, RemitAddressTypeId);
        await uow.SaveChangesAsync();

        return ToDetail((await uow.Providers.GetAggregateAsync(id))!);
    }

    // False = provider not found.
    public async Task<bool> DeleteAsync(int id)
    {
        if (!InScope(id)) throw new ForbiddenAccessException("You can only manage providers assigned to you.");
        var p = await uow.Providers.GetAggregateAsync(id);
        if (p is null) return false;

        // The database would refuse anyway; say why instead of failing with a foreign-key error.
        if (await uow.Providers.HasReferencesAsync(id))
            throw new ConflictException("This provider is still used by users, contracts, assessments or services. Remove those links before deleting it.");

        foreach (var pa in p.ProviderAddresses.ToList()) uow.Providers.RemoveAddressGraph(pa);
        uow.Providers.Remove(p);
        await uow.SaveChangesAsync();
        return true;
    }

    // ---------------------------------------------------------------- validation (camelCase field keys)

    private static Dictionary<string, List<string>> Validate(ProviderEditRequest r)
    {
        var e = new Dictionary<string, List<string>>();

        var name = r.Name?.Trim() ?? "";
        if (name.Length == 0) Add(e, "name", "Provider name is required.");
        else if (name.Length <= 10) Add(e, "name", "Provider name must be more than 10 characters.");   // Blazor CMS rule
        else if (name.Length > ProviderFieldLimits.MaxNameLength) Add(e, "name", $"Name cannot exceed {ProviderFieldLimits.MaxNameLength} characters.");

        var abbreviation = r.Abbreviation?.Trim() ?? "";
        if (abbreviation.Length == 0) Add(e, "abbreviation", "Provider abbreviation is required.");
        else if (abbreviation.Length > ProviderFieldLimits.MaxAbbreviationLength) Add(e, "abbreviation", $"Abbreviation cannot exceed {ProviderFieldLimits.MaxAbbreviationLength} characters.");

        var edison = r.EdisonNumber?.Trim() ?? "";
        if (edison.Length == 0) Add(e, "edisonNumber", "Edison number is required.");
        else if (edison.Length != 10 || !edison.All(char.IsAsciiDigit)) Add(e, "edisonNumber", "Edison number must be exactly 10 digits.");

        var npi = r.Npi?.Trim() ?? "";
        if (npi.Length > 0 && (npi.Length != ProviderFieldLimits.NpiLength || !npi.All(char.IsAsciiDigit)))
            Add(e, "npi", $"NPI must be exactly {ProviderFieldLimits.NpiLength} digits.");

        ValidateAddress(e, "physicalAddress", "Physical address", r.PhysicalAddress);
        ValidateAddress(e, "remitAddress", "Remit address", r.RemitAddress);
        return e;
    }

    private static void ValidateAddress(Dictionary<string, List<string>> e, string key, string section, ProviderAddressModel? a)
    {
        if (IsAddressEmpty(a)) return;
        Max(e, $"{key}.addressLine1", $"{section} line 1", a!.AddressLine1, ProviderFieldLimits.MaxAddressLineLength);
        Max(e, $"{key}.addressLine2", $"{section} line 2", a.AddressLine2, ProviderFieldLimits.MaxAddressLineLength);
        Max(e, $"{key}.city", $"{section} city", a.City, ProviderFieldLimits.MaxAddressLineLength);
        Max(e, $"{key}.zipcode", $"{section} ZIP code", a.Zipcode, ProviderFieldLimits.MaxZipcodeLength);
        Max(e, $"{key}.zipExtension", $"{section} ZIP extension", a.ZipExtension, ProviderFieldLimits.MaxZipExtensionLength);

        var c = a.Contact ?? new ProviderContactModel();
        Max(e, $"{key}.contact.title", $"{section} contact title", c.Title, ProviderFieldLimits.MaxContactNameLength);
        Max(e, $"{key}.contact.firstName", $"{section} contact first name", c.FirstName, ProviderFieldLimits.MaxContactNameLength);
        Max(e, $"{key}.contact.lastName", $"{section} contact last name", c.LastName, ProviderFieldLimits.MaxContactNameLength);
        Max(e, $"{key}.contact.phone", $"{section} contact phone", c.Phone, ProviderFieldLimits.MaxPhoneLength);
        Max(e, $"{key}.contact.wirelessPhone", $"{section} contact wireless phone", c.WirelessPhone, ProviderFieldLimits.MaxPhoneLength);
        if (Has(c.EmailAddress))
        {
            if (c.EmailAddress!.Trim().Length > ProviderFieldLimits.MaxContactEmailLength) Add(e, $"{key}.contact.emailAddress", $"{section} contact email cannot exceed {ProviderFieldLimits.MaxContactEmailLength} characters.");
            else if (!new EmailAddressAttribute().IsValid(c.EmailAddress.Trim())) Add(e, $"{key}.contact.emailAddress", $"{section} contact email is not valid.");
        }
    }

    private async Task CheckUniqueAsync(ProviderEditRequest r, int exceptId, Dictionary<string, List<string>> e)
    {
        var npi = Norm(r.Npi);
        if (npi is not null && !e.ContainsKey("npi") && await uow.Providers.NpiExistsAsync(npi, exceptId))
            Add(e, "npi", "A provider with this NPI already exists.");
        var edison = Norm(r.EdisonNumber);
        if (edison is not null && !e.ContainsKey("edisonNumber") && await uow.Providers.EdisonExistsAsync(edison, exceptId))
            Add(e, "edisonNumber", "A provider with this Edison number already exists.");
    }

    private static void Max(Dictionary<string, List<string>> e, string key, string label, string? v, int max)
    {
        if (v is not null && v.Trim().Length > max) Add(e, key, $"{label} cannot exceed {max} characters.");
    }

    private static void Add(Dictionary<string, List<string>> e, string key, string message)
    {
        if (!e.TryGetValue(key, out var list)) e[key] = list = [];
        list.Add(message);
    }

    private static void Throw(Dictionary<string, List<string>> e)
    {
        if (e.Count > 0) throw new ValidationFailedException(e.ToDictionary(x => x.Key, x => x.Value.ToArray()));
    }

    // ---------------------------------------------------------------- mapping and address sync

    private static ProviderDetail ToDetail(Provider p) => new()
    {
        Id = p.ProviderId, RowVersion = Convert.ToBase64String(p.Version), Name = p.Name, Abbreviation = p.Abbreviation,
        EdisonNumber = p.EdisonNumber, Npi = p.Npi, CreatedOn = p.CreatedOn, UpdatedOn = p.UpdatedOn,
        PhysicalAddress = ToModel(p.ProviderAddresses.FirstOrDefault(pa => pa.AddressTypeId == PhysicalAddressTypeId)),
        RemitAddress = ToModel(p.ProviderAddresses.FirstOrDefault(pa => pa.AddressTypeId == RemitAddressTypeId))
    };

    private static ProviderAddressModel ToModel(ProviderAddress? pa)
    {
        var a = pa?.Address;
        var c = a?.Contact;
        return new()
        {
            AddressLine1 = a?.AddressLine1, AddressLine2 = a?.AddressLine2, City = a?.City, StateId = a?.StateId, CountyId = a?.CountyId,
            Zipcode = a?.Zipcode, ZipExtension = a?.ZipExtension,
            Contact = new() { Title = c?.Title, FirstName = c?.FirstName, LastName = c?.LastName, EmailAddress = c?.EmailAddress, Phone = c?.Phone, WirelessPhone = c?.WirelessPhone }
        };
    }

    // An address section with nothing in it is removed; otherwise the address (and its contact) is created or updated.
    private void SyncAddress(Provider p, ProviderAddressModel model, short typeId)
    {
        var existing = p.ProviderAddresses.FirstOrDefault(pa => pa.AddressTypeId == typeId);
        if (IsAddressEmpty(model))
        {
            if (existing is not null) uow.Providers.RemoveAddressGraph(existing);   // EF drops it from the collection on delete
            return;
        }

        var pa = existing;
        if (pa is null)
        {
            pa = new ProviderAddress { Provider = p, AddressTypeId = typeId, Address = new Address { IsActive = true } };
            p.ProviderAddresses.Add(pa);
        }
        var a = pa.Address;
        a.AddressLine1 = Norm(model.AddressLine1);
        a.AddressLine2 = Norm(model.AddressLine2);
        a.City = Norm(model.City);
        a.StateId = ToShort(model.StateId);
        a.CountyId = ToShort(model.CountyId);
        a.Zipcode = Norm(model.Zipcode);
        a.ZipExtension = Norm(model.ZipExtension);
        a.IsActive = true;

        var cm = model.Contact ?? new ProviderContactModel();
        if (!HasContact(cm))
        {
            if (a.Contact is not null) { uow.Providers.RemoveContact(a.Contact); a.Contact = null; a.ContactId = null; }
            return;
        }
        var c = a.Contact ??= new Contact();
        c.Title = Norm(cm.Title); c.FirstName = Norm(cm.FirstName); c.LastName = Norm(cm.LastName);
        c.EmailAddress = Norm(cm.EmailAddress); c.Phone = Norm(cm.Phone); c.WirelessPhone = Norm(cm.WirelessPhone);
    }

    private static bool IsAddressEmpty(ProviderAddressModel? m) =>
        m is null || (!Has(m.AddressLine1) && !Has(m.AddressLine2) && !Has(m.City) && m.StateId is null && m.CountyId is null
                      && !Has(m.Zipcode) && !Has(m.ZipExtension) && !HasContact(m.Contact));

    private static bool HasContact(ProviderContactModel? c) =>
        c is not null && (Has(c.Title) || Has(c.FirstName) || Has(c.LastName) || Has(c.EmailAddress) || Has(c.Phone) || Has(c.WirelessPhone));

    private static bool Has(string? s) => !string.IsNullOrWhiteSpace(s);
    private static string? Norm(string? s) => Has(s) ? s!.Trim() : null;
    private static short? ToShort(int? v) => v is > 0 and <= short.MaxValue ? (short)v.Value : null;
}
