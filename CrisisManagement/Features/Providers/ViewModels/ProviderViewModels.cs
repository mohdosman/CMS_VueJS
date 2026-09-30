namespace CrisisManagement.Features.Providers.ViewModels;

public sealed class ProviderSearchRequest
{
    public string? Name { get; set; }
    public string? Abbreviation { get; set; }
    public string? EdisonNumber { get; set; }
    public string? Npi { get; set; }
    public int PageIndex { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    public string? SortBy { get; set; }
    public bool SortDesc { get; set; }
}

public sealed record ProviderListItem(int ProviderId, string Name, string? Abbreviation, string? EdisonNumber, string? Npi, DateTime UpdatedOn);

public sealed class ProviderContactModel
{
    public string? Title { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? EmailAddress { get; set; }
    public string? Phone { get; set; }
    public string? WirelessPhone { get; set; }
}

// Physical and remit addresses share this shape; the address type is fixed by which one it is.
public sealed class ProviderAddressModel
{
    public string? AddressLine1 { get; set; }
    public string? AddressLine2 { get; set; }
    public string? City { get; set; }
    public int? StateId { get; set; }
    public int? CountyId { get; set; }
    public string? Zipcode { get; set; }
    public string? ZipExtension { get; set; }
    public ProviderContactModel Contact { get; set; } = new();
}

public sealed class ProviderDetail
{
    public int Id { get; set; }
    public string RowVersion { get; set; } = "";
    public string Name { get; set; } = "";
    public string? Abbreviation { get; set; }
    public string? EdisonNumber { get; set; }
    public string? Npi { get; set; }
    public ProviderAddressModel PhysicalAddress { get; set; } = new();
    public ProviderAddressModel RemitAddress { get; set; } = new();
    public DateTime CreatedOn { get; set; }
    public DateTime UpdatedOn { get; set; }
}

// Create and update share one body.
public sealed class ProviderEditRequest
{
    public string? RowVersion { get; set; }
    public string? Name { get; set; }
    public string? Abbreviation { get; set; }
    public string? EdisonNumber { get; set; }
    public string? Npi { get; set; }
    public ProviderAddressModel PhysicalAddress { get; set; } = new();
    public ProviderAddressModel RemitAddress { get; set; } = new();
}

public sealed record ProviderLookups(IReadOnlyList<Shared.Common.LookupItem> States, IReadOnlyList<Shared.Common.LookupItem> Counties);
