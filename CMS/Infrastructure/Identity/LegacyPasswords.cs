using System.Security.Cryptography;
using System.Text;
using CMS.Data.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;

namespace CMS.Infrastructure.Identity;

// Ported from the Blazor CMS: existing accounts still hold a Rijndael-encrypted
// RBS_User.Password. On first successful login Identity rehashes and we clear it.
public sealed class LegacyCryptoOptions
{
    public string PassPhrase { get; set; } = "";
    public string SaltValue { get; set; } = "";
    public string HashAlgorithm { get; set; } = "SHA1";
    public int PasswordIterations { get; set; } = 2;
    public string InitVector { get; set; } = "";
    public int KeySize { get; set; } = 256;
}

public sealed class HybridPasswordHasher(IOptions<LegacyCryptoOptions> legacy, IOptions<PasswordHasherOptions> hasher)
    : IPasswordHasher<ApplicationUser>
{
    private readonly PasswordHasher<ApplicationUser> _identity = new(hasher);
    private readonly LegacyCryptoOptions _opt = legacy.Value;

    public string HashPassword(ApplicationUser user, string password) => _identity.HashPassword(user, password);

    public PasswordVerificationResult VerifyHashedPassword(ApplicationUser user, string? hashedPassword, string providedPassword)
    {
        if (!string.IsNullOrWhiteSpace(hashedPassword))
            return _identity.VerifyHashedPassword(user, hashedPassword, providedPassword);

        if (string.IsNullOrWhiteSpace(user.LegacyPassword) || !VerifyLegacy(user.LegacyPassword, providedPassword))
            return PasswordVerificationResult.Failed;

        // Persisted by UserManager's UpdateAsync after the rehash.
        user.LegacyPassword = string.Empty;
        return PasswordVerificationResult.SuccessRehashNeeded;
    }

    private bool VerifyLegacy(string cipherBase64, string provided)
    {
        try
        {
            var plain = Encoding.UTF8.GetBytes(Decrypt(cipherBase64));
            var given = Encoding.UTF8.GetBytes(provided);
            return plain.Length == given.Length && CryptographicOperations.FixedTimeEquals(plain, given);
        }
        catch (Exception ex) when (ex is FormatException or CryptographicException)
        {
            return false;
        }
    }

    private string Decrypt(string cipherBase64)
    {
        var iv = Encoding.UTF8.GetBytes(_opt.InitVector);
        if (iv.Length != 16) throw new CryptographicException("Legacy init vector must be 16 bytes.");

        var cipher = Convert.FromBase64String(cipherBase64);
        using var derive = new PasswordDeriveBytes(_opt.PassPhrase, Encoding.UTF8.GetBytes(_opt.SaltValue), _opt.HashAlgorithm, _opt.PasswordIterations);
        var key = derive.GetBytes(_opt.KeySize / 8);

        using var aes = new RijndaelManaged { Mode = CipherMode.CBC };
        using var stream = new CryptoStream(new MemoryStream(cipher), aes.CreateDecryptor(key, iv), CryptoStreamMode.Read);
        var buffer = new byte[cipher.Length];
        var n = stream.Read(buffer, 0, buffer.Length);
        return Encoding.UTF8.GetString(buffer, 0, n);
    }
}

public sealed class HybridUserManager(
    IUserStore<ApplicationUser> store, IOptions<IdentityOptions> options, IPasswordHasher<ApplicationUser> hasher,
    IEnumerable<IUserValidator<ApplicationUser>> userValidators, IEnumerable<IPasswordValidator<ApplicationUser>> passwordValidators,
    ILookupNormalizer normalizer, IdentityErrorDescriber errors, IServiceProvider services, ILogger<UserManager<ApplicationUser>> logger)
    : UserManager<ApplicationUser>(store, options, hasher, userValidators, passwordValidators, normalizer, errors, services, logger)
{
    public override async Task<bool> IsLockedOutAsync(ApplicationUser user) =>
        user.LegacyIsLockedOut || await base.IsLockedOutAsync(user);

    // Identity short-circuits on a null PasswordHash; let legacy-only users reach the hybrid hasher.
    protected override Task<PasswordVerificationResult> VerifyPasswordAsync(IUserPasswordStore<ApplicationUser> store, ApplicationUser user, string password) =>
        string.IsNullOrWhiteSpace(user.PasswordHash) && !string.IsNullOrWhiteSpace(user.LegacyPassword)
            ? Task.FromResult(PasswordHasher.VerifyHashedPassword(user, user.PasswordHash!, password))
            : base.VerifyPasswordAsync(store, user, password);
}
