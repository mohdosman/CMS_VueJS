using System.Text.RegularExpressions;

namespace CrisisManagement.Shared.Common;

// Social security number rules shared by the assessment and service screens (Blazor CMS SsnPolicy). Blank is allowed.
// An SSN is entered as ddd-dd-dddd or nine digits and stored as nine digits.
public static partial class SsnPolicy
{
    public const string InvalidMessage = "SSN is invalid";

    private static readonly HashSet<string> Dummies = ["123456789", "219099999", "078051120", "987654321"];

    // The nine digits when the text has the SSN shape, otherwise false. Used to normalize a search filter.
    public static bool TryNormalize(string? value, out string digits)
    {
        digits = "";
        var v = value?.Trim() ?? "";
        if (!Shape().IsMatch(v)) return false;
        digits = v.Replace("-", "");
        return true;
    }

    // Blank is valid; otherwise the shape and the invalid-number rules must hold.
    public static bool IsValid(string? value)
    {
        if (string.IsNullOrWhiteSpace(value)) return true;
        if (!TryNormalize(value, out var d)) return false;
        return !(d.Distinct().Count() == 1 || Dummies.Contains(d) || d[..3] is "000" or "666" || d.Substring(3, 2) == "00" || d[5..] == "0000");
    }

    [GeneratedRegex(@"^(\d{9}|\d{3}-\d{2}-\d{4})$")] private static partial Regex Shape();
}
