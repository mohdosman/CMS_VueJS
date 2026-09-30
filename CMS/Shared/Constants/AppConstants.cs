namespace CMS.Shared.Constants;

public static class AppClaimTypes
{
    // Role claims in RBS_RoleClaim use this type; Identity copies them onto the principal at sign-in.
    public const string Permission = "permission";
    // Comma-separated CMS_ProviderUser ids, the shape the Blazor CMS put in its JWT.
    public const string ProviderIds = "provider_ids";
}

public static class AppRoles
{
    public const string Admin = "Administrator";
}
