using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Identity;

namespace CMS.Infrastructure.Identity;

// Microsoft Entra ID sign-in for Active Directory (state employee) accounts, done on the server like SafetyNet:
// the OpenID Connect handler runs the authorization-code + PKCE flow and drops the result in Identity's external
// cookie; AccountController.AzureCallback maps it to a local AD account and signs that user in with the normal
// application cookie, so roles, permissions and the API work exactly as for a password sign-in.
//
// Needs, in the "AzureAd" section: TenantId, ClientId (appsettings.json) and ClientSecret (user-secrets),
// plus the redirect URI  https://<host>/signin-oidc  registered on the Entra app as a Web platform URI.
// Without ClientId and ClientSecret the scheme is not registered and the sign-in button is hidden.
public static class EntraSignIn
{
    public const string Scheme = "Entra";

    public static bool IsConfigured(IConfiguration config) =>
        !string.IsNullOrWhiteSpace(config["AzureAd:ClientId"]) && !string.IsNullOrWhiteSpace(config["AzureAd:ClientSecret"])
        && !string.IsNullOrWhiteSpace(config["AzureAd:TenantId"]);

    public static AuthenticationBuilder AddEntraSignIn(this AuthenticationBuilder auth, IConfiguration config)
    {
        if (!IsConfigured(config)) return auth;

        var section = config.GetSection("AzureAd");
        return auth.AddOpenIdConnect(Scheme, "Microsoft Entra ID", o =>
        {
            var instance = (section["Instance"] ?? "https://login.microsoftonline.com/").TrimEnd('/');
            o.Authority = $"{instance}/{section["TenantId"]}/v2.0";
            o.ClientId = section["ClientId"];
            o.ClientSecret = section["ClientSecret"];
            o.CallbackPath = section["CallbackPath"] ?? "/signin-oidc";
            o.ResponseType = "code";
            o.UsePkce = true;
            o.SaveTokens = false;
            o.MapInboundClaims = false;   // keep "preferred_username" as it is in the token
            o.SignInScheme = IdentityConstants.ExternalScheme;
            o.Scope.Clear();
            o.Scope.Add("openid");
            o.Scope.Add("profile");

            // The user cancelled at Microsoft, or Microsoft refused: back to the sign-in page with a fixed message.
            o.Events.OnRemoteFailure = ctx =>
            {
                ctx.HttpContext.RequestServices.GetRequiredService<ILoggerFactory>().CreateLogger("EntraSignIn")
                    .LogWarning(ctx.Failure, "Microsoft sign-in failed");
                ctx.Response.Redirect("/Account/Login?error=entra");
                ctx.HandleResponse();
                return Task.CompletedTask;
            };
        });
    }
}
