using System.Globalization;
using MVDC.FullTime.Models;

namespace MVDC.FullTime.Formatters;

/// <summary>
/// Formats raw fixture data (string arrays from FixtureParser) into structured FormattedFixture objects.
/// </summary>
public sealed class FixtureFormatter
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
    /// Formats raw fixture arrays into structured fixture objects.
    /// </summary>
    /// <param name="fixtures">Raw fixture data from FixtureParser. Each array has cells: [type, dateTime, home, "", "VS", "", away, "", division, status].</param>
    /// <param name="includeTbcFixtures">Whether to include fixtures with a "TBC" date.</param>
    /// <param name="includeCupFixtures">Whether to include cup fixtures.</param>
    /// <param name="dateFormat">Custom date format string (defaults to dd/MM/yyyy).</param>
    /// <param name="timeFormat">Custom time format string (defaults to HH:mm).</param>
    public IReadOnlyList<FormattedFixture> FormatFixtures(
        IReadOnlyList<string[]> fixtures,
        bool includeTbcFixtures = true,
        bool includeCupFixtures = true,
        string? dateFormat = null,
        string? timeFormat = null)
    {
        var outputDateFormat = dateFormat ?? DefaultDateFormat;
        var outputTimeFormat = timeFormat ?? DefaultTimeFormat;
        var result = new List<FormattedFixture>();

        foreach (var fixture in fixtures)
        {
            if (fixture.Length == 0)
                continue;

            var fixtureType = fixture[0];

            // Skip cup fixtures if not wanted
            if (!includeCupFixtures && string.Equals(fixtureType, "Cup", StringComparison.OrdinalIgnoreCase))
                continue;

            // Handle TBC fixtures
            if (fixture.Length > 1 && string.Equals(fixture[1], "TBC", StringComparison.OrdinalIgnoreCase))
            {
                if (includeTbcFixtures)
                {
                    result.Add(new FormattedFixture
                    {
                        Date = "TBC",
                        Time = "TBC",
                        Home = fixture.Length > 2 ? fixture[2] : string.Empty,
                        Away = fixture.Length > 6 ? fixture[6] : string.Empty,
                        FixtureType = fixtureType
                    });
                }
                continue;
            }

            // Parse the Full-Time date format
            if (fixture.Length > 6 &&
                DateTime.TryParseExact(fixture[1], FullTimeDateFormat, CultureInfo.InvariantCulture,
                    DateTimeStyles.None, out var dateTime))
            {
                result.Add(new FormattedFixture
                {
                    Date = dateTime.ToString(outputDateFormat, CultureInfo.InvariantCulture),
                    Time = dateTime.ToString(outputTimeFormat, CultureInfo.InvariantCulture),
                    Home = fixture[2],
                    Away = fixture[6],
                    FixtureType = fixtureType
                });
            }
        }

        return result;
    }
}
