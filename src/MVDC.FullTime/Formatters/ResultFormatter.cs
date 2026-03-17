using System.Globalization;
using System.Text.RegularExpressions;
using MVDC.FullTime.Models;

namespace MVDC.FullTime.Formatters;

/// <summary>
/// Formats raw result data (string arrays from ResultParser) into structured FormattedResult objects.
/// </summary>
public sealed partial class ResultFormatter
{
    /// <summary>
    /// The date format used by the FA Full-Time system: "dd/MM/yy HH:mm".
    /// </summary>
    private const string FullTimeDateFormat = "dd/MM/yy HH:mm";

    /// <summary>
    /// Default output date format.
    /// </summary>
    private const string DefaultDateFormat = "dd/MM/yyyy";

    /// <summary>
    /// Default output time format.
    /// </summary>
    private const string DefaultTimeFormat = "HH:mm";

    /// <summary>
    /// Formats raw result arrays into structured result objects.
    /// </summary>
    /// <param name="results">Raw result data from ResultParser. Each array: [dateTime, homeTeam, score, awayTeam, division].</param>
    /// <param name="dateFormat">Custom date format string (defaults to dd/MM/yyyy).</param>
    /// <param name="timeFormat">Custom time format string (defaults to HH:mm).</param>
    public IReadOnlyList<FormattedResult> FormatResults(
        IReadOnlyList<string[]> results,
        string? dateFormat = null,
        string? timeFormat = null)
    {
        var outputDateFormat = dateFormat ?? DefaultDateFormat;
        var outputTimeFormat = timeFormat ?? DefaultTimeFormat;
        var formatted = new List<FormattedResult>();

        foreach (var result in results)
        {
            if (result.Length < 4)
                continue;

            if (!DateTime.TryParseExact(result[0], FullTimeDateFormat, CultureInfo.InvariantCulture,
                    DateTimeStyles.None, out var dateTime))
                continue;

            var fullScore = result[2];

            // Extract home score (first number in the score string)
            var homeScoreMatch = FirstDigitRegex().Match(fullScore);
            var homeScore = homeScoreMatch.Success ? homeScoreMatch.Groups[1].Value : "0";

            // Extract away score (last number in the score string)
            var awayScoreMatch = LastDigitRegex().Match(fullScore);
            var awayScore = awayScoreMatch.Success ? awayScoreMatch.Groups[1].Value : "0";

            formatted.Add(new FormattedResult
            {
                Date = dateTime.ToString(outputDateFormat, CultureInfo.InvariantCulture),
                Time = dateTime.ToString(outputTimeFormat, CultureInfo.InvariantCulture),
                Home = result[1],
                HomeScore = homeScore,
                Away = result[3],
                AwayScore = awayScore,
                FullScore = fullScore
            });
        }

        return formatted;
    }

    [GeneratedRegex(@"(\d+)")]
    private static partial Regex FirstDigitRegex();

    [GeneratedRegex(@"(\d+)(?!.*\d)")]
    private static partial Regex LastDigitRegex();
}
