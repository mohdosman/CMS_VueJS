namespace CMS.Shared.Constants;

/// <summary>
/// Column lengths for a provider and its addresses and contacts. The database is the authority, so
/// the EF configuration is built from these too: a validator that allows more than the column holds
/// fails at SQL with a truncation error instead of a message on the field.
/// </summary>
public static class ProviderFieldLimits
{
    /// <summary>CMS_Provider.Name, varchar(150).</summary>
    public const int MaxNameLength = 150;

    /// <summary>CMS_Provider.Abbreviation, varchar(50).</summary>
    public const int MaxAbbreviationLength = 50;

    /// <summary>
    /// CMS_Provider.EdisonNumber, varchar(50). Far wider than the value it holds: an Edison number
    /// is exactly ten digits, which <see cref="EdisonNumberPolicy"/> enforces.
    /// </summary>
    public const int MaxEdisonNumberLength = 50;

    /// <summary>CMS_Provider.NPI, varchar(50); the value itself is exactly ten digits.</summary>
    public const int MaxNpiColumnLength = 50;

    /// <summary>An NPI is exactly ten digits, well inside its column.</summary>
    public const int NpiLength = 10;

    /// <summary>CMS_Address.AddressLine1 / AddressLine2 / City, all varchar(50).</summary>
    public const int MaxAddressLineLength = 50;

    /// <summary>CMS_Address.Zipcode, varchar(5).</summary>
    public const int MaxZipcodeLength = 5;

    /// <summary>CMS_Address.ZipExtension, varchar(4).</summary>
    public const int MaxZipExtensionLength = 4;

    /// <summary>CMS_Contact.Title / FirstName / LastName, all varchar(150).</summary>
    public const int MaxContactNameLength = 150;

    /// <summary>CMS_Contact.EmailAddress, varchar(250).</summary>
    public const int MaxContactEmailLength = 250;

    /// <summary>CMS_Contact.Phone / WirelessPhone, both varchar(15).</summary>
    public const int MaxPhoneLength = 15;
}
