using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using MVDC.FullTime;
using MVDC.FullTime.Models;
using MVDC.Shared.Models;

namespace MVDC.Api.Services;

/// <summary>
/// Seeds fixtures from the FA Full-Time website into Cosmos DB on API startup.
/// Fetches all fixtures for Mole Valley Girls teams across the fixture groups
/// they appear in, and upserts them to avoid duplicates.
/// </summary>
public static class FullTimeFixtureSeeder
{
    private const int SeasonId = 310037110;

    /// <summary>
    /// Teams to seed fixtures for, each with their Full-Time team ID and
    /// the fixture group keys they appear in.
    /// </summary>
    private static readonly TeamConfig[] Teams =
    [
        new("Mole Valley Girls U9 Blacks", "mv-u9-blacks",
        [
            "1_223342503", // U9 Lime v3
            "1_478076013", // U9 Grey v4
        ]),
        new("Mole Valley Girls U9 Greens", "mv-u9-greens",
        [
            "1_834940432", // U9 Black v4
        ]),
    ];

    /// <summary>
    /// Fetches fixtures from Full-Time and upserts them into the database.
    /// </summary>
    public static async Task SeedAsync(IRepository<Fixture> repository, ILogger logger)
    {
        var division = new Division();
        var totalSeeded = 0;

        foreach (var team in Teams)
        {
            var teamSeeded = 0;

            foreach (var groupKey in team.FixtureGroupKeys)
            {
                logger.LogInformation("Fetching fixtures from Full-Time group '{GroupKey}' for '{TeamName}'...",
                    groupKey, team.Name);

                IReadOnlyList<FormattedFixture> fixtures;
                try
                {
                    fixtures = await division.GetFormattedFixturesAsync(SeasonId, groupKey);
                }
                catch (Exception ex)
                {
                    logger.LogWarning(ex, "Failed to fetch fixtures from Full-Time group '{GroupKey}'. Skipping.", groupKey);
                    continue;
                }

                logger.LogInformation("Found {Count} total fixtures in group '{GroupKey}'.", fixtures.Count, groupKey);

                // Filter to only fixtures involving this team
                var teamFixtures = fixtures
                    .Where(f => string.Equals(f.Home, team.Name, StringComparison.OrdinalIgnoreCase)
                             || string.Equals(f.Away, team.Name, StringComparison.OrdinalIgnoreCase))
                    .ToList();

                logger.LogInformation("Found {Count} fixtures for '{TeamName}' in group '{GroupKey}'.",
                    teamFixtures.Count, team.Name, groupKey);

                foreach (var ftFixture in teamFixtures)
                {
                    var isHome = string.Equals(ftFixture.Home, team.Name, StringComparison.OrdinalIgnoreCase);
                    var opponent = isHome ? ftFixture.Away : ftFixture.Home;

                    // Parse date + time into DateTime
                    DateTime fixtureDate;
                    var dateTimeString = $"{ftFixture.Date} {ftFixture.Time}".Trim();
                    if (!DateTime.TryParseExact(dateTimeString, "dd/MM/yyyy HH:mm",
                            CultureInfo.InvariantCulture, DateTimeStyles.None, out fixtureDate))
                    {
                        // Try date-only if time is missing/empty
                        if (!DateTime.TryParseExact(ftFixture.Date, "dd/MM/yyyy",
                                CultureInfo.InvariantCulture, DateTimeStyles.None, out fixtureDate))
                        {
                            logger.LogWarning("Could not parse date '{Date}' / time '{Time}' for fixture {Home} vs {Away}. Skipping.",
                                ftFixture.Date, ftFixture.Time, ftFixture.Home, ftFixture.Away);
                            continue;
                        }
                    }

                    // Generate a deterministic ID based on date + home + away + group
                    // so that restarting the API doesn't create duplicates.
                    var fixtureId = GenerateDeterministicId(fixtureDate, ftFixture.Home, ftFixture.Away, groupKey);

                    var fixture = new Fixture
                    {
                        Id = fixtureId,
                        TeamId = team.TeamId,
                        Opponent = opponent,
                        Date = fixtureDate,
                        Venue = isHome ? "Home" : "Away",
                        IsHome = isHome,
                        FixtureGroupKey = groupKey,
                    };

                    try
                    {
                        // Upsert: UpdateAsync uses UpsertItemAsync under the hood
                        await repository.UpdateAsync(fixtureId, fixture);
                        teamSeeded++;
                    }
                    catch (Exception ex)
                    {
                        logger.LogWarning(ex, "Failed to upsert fixture '{Id}' ({Home} vs {Away}).",
                            fixtureId, ftFixture.Home, ftFixture.Away);
                    }
                }
            }

            logger.LogInformation("Seeded {Count} fixture(s) for '{TeamName}'.", teamSeeded, team.Name);
            totalSeeded += teamSeeded;
        }

        logger.LogInformation("Full-Time fixture seeding complete. {Count} fixture(s) upserted total.", totalSeeded);
    }

    /// <summary>
    /// Generates a deterministic, URL-safe ID from the fixture details.
    /// Uses SHA256 hash truncated to 16 hex chars for a compact but collision-resistant ID.
    /// </summary>
    private static string GenerateDeterministicId(DateTime date, string home, string away, string groupKey)
    {
        var input = $"{date:O}|{home.ToLowerInvariant()}|{away.ToLowerInvariant()}|{groupKey}";
        var hashBytes = SHA256.HashData(Encoding.UTF8.GetBytes(input));
        // Take first 16 hex chars (8 bytes = 64 bits) — sufficient for this use case
        return Convert.ToHexString(hashBytes)[..16].ToLowerInvariant();
    }

    private sealed record TeamConfig(string Name, string TeamId, string[] FixtureGroupKeys);
}
