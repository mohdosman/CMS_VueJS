namespace CMS.Shared.Constants;

/// <summary>
/// Column lengths for a menu item, verified against INFORMATION_SCHEMA rather than the EF mapping,
/// which has been wrong elsewhere in this codebase.
/// </summary>
public static class MenuFieldLimits
{
    /// <summary>RBS_MenuItem.MenuItemName, varchar(50).</summary>
    public const int MaxNameLength = 50;

    /// <summary>RBS_MenuItem.Icon, varchar(50).</summary>
    public const int MaxIconLength = 50;

    /// <summary>
    /// RBS_MenuItem.Url / DetailUrl / TemplateUrl / DetailTemplateUrl / ApiUrl, all varchar(250).
    /// </summary>
    public const int MaxUrlLength = 250;

    /// <summary>RBS_MenuItem.Description / Comment, both varchar(250).</summary>
    public const int MaxTextLength = 250;
}
