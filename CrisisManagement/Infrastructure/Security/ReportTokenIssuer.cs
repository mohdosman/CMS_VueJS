using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography.X509Certificates;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace CrisisManagement.Infrastructure.Security;

// The report server (Crystal Reports site) trusts a JWT signed with the shared certificate. Configuration, as in the
// Blazor CMS: Jwt:Issuer, Jwt:Audience, Jwt:Signing:Algorithm and Jwt:Signing:Cert:PfxPath/Password (user secrets or
// environment, never appsettings), and the ReportServer section.
public sealed class JwtOptions
{
    public string Issuer { get; set; } = "";
    public string Audience { get; set; } = "";
    public SigningOptions Signing { get; set; } = new();

    public sealed class SigningOptions
    {
        public string Algorithm { get; set; } = SecurityAlgorithms.RsaSha256;
        public CertOptions Cert { get; set; } = new();
    }

    public sealed class CertOptions
    {
        public string? PfxPath { get; set; }
        public string? Password { get; set; }
    }
}

public sealed class ReportServerOptions
{
    public string ReportServerUrl { get; set; } = "";
    public string ReportFilesPath { get; set; } = "";
    public string ReportEnvironment { get; set; } = "";
    public int TokenExpirationMinutes { get; set; } = 30;

    // Roles whose members may only see their own provider's data in a report.
    public string[] ProviderScopedRoles { get; set; } = ["Provider Staff", "Provider Read Only", "Managed Care Organization"];
}

public sealed class ReportTokenIssuer(IOptions<JwtOptions> jwt, IOptions<ReportServerOptions> report)
{
    private readonly Lazy<SigningCredentials> _credentials = new(() => Load(jwt.Value));

    private static SigningCredentials Load(JwtOptions o)
    {
        var path = o.Signing.Cert.PfxPath;
        if (string.IsNullOrWhiteSpace(path) || !File.Exists(path))
            throw new InvalidOperationException("The report signing certificate is not configured (Jwt:Signing:Cert:PfxPath).");
        var cert = X509CertificateLoader.LoadPkcs12FromFile(path, o.Signing.Cert.Password);
        return new SigningCredentials(new X509SecurityKey(cert) { KeyId = cert.Thumbprint }, o.Signing.Algorithm);
    }

    // The token the report server reads: the user name, "{exportOption}" or "{exportOption}|{providerName}", and a version stamp.
    // Audience: "{Jwt:Audience}/{ReportEnvironment}/{report file}". Claim types are the long ClaimTypes URIs, as the report server expects.
    public string Issue(string userName, string userData, string reportFile)
    {
        var now = DateTime.UtcNow;
        var claims = new[]
        {
            new Claim(ClaimTypes.Name, userName),
            new Claim(ClaimTypes.UserData, userData),
            new Claim(ClaimTypes.Version, now.Ticks.ToString())
        };
        var audience = string.Join("/", jwt.Value.Audience, report.Value.ReportEnvironment, reportFile);
        var token = new JwtSecurityToken(jwt.Value.Issuer, audience, claims, now, now.AddMinutes(report.Value.TokenExpirationMinutes), _credentials.Value);
        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
