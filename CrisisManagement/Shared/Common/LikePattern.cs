namespace CrisisManagement.Shared.Common;

// A "contains" pattern for EF.Functions.Like: % and _ typed by the user match themselves instead of acting as wildcards.
// Pass Escape as the third argument of Like, e.g. EF.Functions.Like(col, LikePattern.Contains(text), LikePattern.Escape).
public static class LikePattern
{
    public const string Escape = "\\";

    public static string Contains(string text) =>
        "%" + text.Trim().Replace("\\", "\\\\").Replace("%", "\\%").Replace("_", "\\_").Replace("[", "\\[") + "%";
}
