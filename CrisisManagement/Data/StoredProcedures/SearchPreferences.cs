using System.Xml.Linq;

namespace CrisisManagement.Data.StoredProcedures;

// The <root> XML every legacy usp_CMS_*_rd search procedure takes as @i_PreferenceXML. An element is only added when it
// has a value, which is how the procedures know a filter was not used. The procedures read the values into parameters
// of a dynamic query, so values go in as they are (no quote doubling: that would make "O'Brien" search for two quotes).
public sealed class SearchPreferences
{
    private readonly XElement _root = new("root");

    public SearchPreferences Add(string name, string? value)
    {
        if (!string.IsNullOrWhiteSpace(value)) _root.Add(new XElement(name, value.Trim()));
        return this;
    }

    // For the procedures that build their query by string concatenation (services, service files, suicides): quotes are
    // doubled. Check the value with TextSafety first.
    public SearchPreferences AddEscaped(string name, string? value) => Add(name, value?.Replace("'", "''"));

    public SearchPreferences Add(string name, int? value) => value is > 0 ? Add(name, value.Value.ToString()) : this;

    // Dates travel as MM/dd/yyyy, which the procedures convert with style 101.
    public SearchPreferences Add(string name, DateTime? value) => Add(name, value?.ToString("MM/dd/yyyy"));

    // The caller scope: a comma-separated list of provider ids, from parsed integers only, because the procedures place it
    // inside an IN (...) list. The procedure variable is varchar(50), so a longer list is refused rather than truncated.
    public SearchPreferences AddProviderIds(string name, IReadOnlyCollection<int>? ids)
    {
        if (ids is null) return this;
        var list = string.Join(",", ids);
        if (list.Length > 50) throw new InvalidOperationException("The account has too many providers for this search.");
        return Add(name, list);
    }

    public string ToXml() => _root.ToString(SaveOptions.DisableFormatting);
}

// A text filter that ends up inside quotes in a concatenated query must survive doubling, fit the procedure variable
// (varchar(50)) and hold only characters that stay themselves when SQL Server converts nvarchar to varchar (a look-alike
// such as U+02BC would otherwise turn into a real quote).
public static class TextSafety
{
    public const int MaxEscapedLength = 50;

    // Error text for the value, or null when it is fine.
    public static string? Problem(string? value, string label)
    {
        var v = value?.Trim() ?? "";
        if (v.Any(c => c > 0x17F || char.IsControl(c))) return $"{label} contains characters that cannot be searched.";
        return v.Replace("'", "''").Length > MaxEscapedLength ? $"{label} must be {MaxEscapedLength} characters or fewer (apostrophes count twice)." : null;
    }
}
