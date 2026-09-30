namespace CMS.Shared.Constants;

public static class AppClaimTypes
{
    // Role claims in RBS_RoleClaim use this type; Identity copies them onto the principal at sign-in.
    public const string Permission = "permission";
    // Comma-separated CMS_ProviderUser ids, the shape the Blazor CMS put in its JWT.
    public const string ProviderIds = "provider_ids";
    // Set while the signed-in user owes the account an action; see RequiredAccountActionMiddleware.
    public const string IsTemporaryPassword = "is_temporary_password";
    public const string RequiresMfaSetup = "requires_mfa_setup";
}

public static class AppRoles
{
    public const string Admin = "Administrator";
}
