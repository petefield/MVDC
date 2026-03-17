using System.Text.RegularExpressions;

namespace MVDC.FullTime.Helpers;

/// <summary>
/// String manipulation utilities.
/// </summary>
public static partial class StringHelper
{
    /// <summary>
    /// Trims a string, removes newlines and carriage returns,
    /// and collapses multiple spaces into a single space.
    /// </summary>
    public static string RemoveWhitespace(string text)
    {
        var trimmed = text.Trim();
        var noNewlines = trimmed.Replace("\n", "").Replace("\r", "");
        return MultipleSpacesRegex().Replace(noNewlines, " ");
    }

    [GeneratedRegex(@"\s+")]
    private static partial Regex MultipleSpacesRegex();
}
