using MVDC.FullTime.Formatters;
using MVDC.FullTime.Models;
using MVDC.FullTime.Parsers;

namespace MVDC.FullTime;

/// <summary>
/// Main entry point for querying the FA Full-Time system for a division's
/// teams, fixtures, and results.
/// </summary>
public sealed class Division
{
    private const string FixturesUrlTemplate =
        "https://fulltime.thefa.com/fixtures.html?selectedSeason={0}&selectedFixtureGroupKey={1}&selectedDateCode=all&selectedRelatedFixtureOption=1&previousSelectedFixtureGroupKey={1}&itemsPerPage=10000";

    private const string TeamsUrlTemplate =
        "https://fulltime.thefa.com/fixtures.html?selectedSeason={0}&selectedFixtureGroupKey={1}&selectedDateCode=all&selectedRelatedFixtureOption=1&itemsPerPage=100";

    private const string ResultsUrlTemplate =
        "https://fulltime.thefa.com/results.html?selectedSeason={0}&selectedFixtureGroupKey={1}&selectedDateCode=all&selectedRelatedFixtureOption=1&previousSelectedFixtureGroupKey={1}&itemsPerPage=10000";

    private readonly IFullTimeClient _client;
    private readonly TeamParser _teamParser;
    private readonly FixtureParser _fixtureParser;
    private readonly ResultParser _resultParser;
    private readonly FixtureFormatter _fixtureFormatter;
    private readonly ResultFormatter _resultFormatter;

    public Division(IFullTimeClient client)
    {
        _client = client ?? throw new ArgumentNullException(nameof(client));
        _teamParser = new TeamParser();
        _fixtureParser = new FixtureParser();
        _resultParser = new ResultParser();
        _fixtureFormatter = new FixtureFormatter();
        _resultFormatter = new ResultFormatter();
    }

    /// <summary>
    /// Gets the list of team names for the given season and group.
    /// </summary>
    public async Task<IReadOnlyList<string>> GetTeamsAsync(
        int seasonId,
        string groupId,
        CancellationToken cancellationToken = default)
    {
        var url = string.Format(TeamsUrlTemplate, seasonId, groupId);
        var html = await _client.GetAsync(url, cancellationToken);
        return _teamParser.Parse(html);
    }

    /// <summary>
    /// Gets raw (unformatted) fixture data for the given season and group.
    /// Each fixture is a string array of table cell values.
    /// </summary>
    public async Task<IReadOnlyList<string[]>> GetFixturesAsync(
        int seasonId,
        string groupId,
        CancellationToken cancellationToken = default)
    {
        var url = string.Format(FixturesUrlTemplate, seasonId, groupId);
        var html = await _client.GetAsync(url, cancellationToken);
        return _fixtureParser.Parse(html);
    }

    /// <summary>
    /// Gets formatted fixture data for the given season and group.
    /// </summary>
    public async Task<IReadOnlyList<FormattedFixture>> GetFormattedFixturesAsync(
        int seasonId,
        string groupId,
        bool includeTbcFixtures = true,
        bool includeCupFixtures = true,
        string? dateFormat = null,
        string? timeFormat = null,
        CancellationToken cancellationToken = default)
    {
        var raw = await GetFixturesAsync(seasonId, groupId, cancellationToken);
        return _fixtureFormatter.FormatFixtures(raw, includeTbcFixtures, includeCupFixtures, dateFormat, timeFormat);
    }

    /// <summary>
    /// Gets raw (unformatted) result data for the given season and group.
    /// Each result is a string array: [dateTime, homeTeam, score, awayTeam, division].
    /// </summary>
    public async Task<IReadOnlyList<string[]>> GetResultsAsync(
        int seasonId,
        string groupId,
        CancellationToken cancellationToken = default)
    {
        var url = string.Format(ResultsUrlTemplate, seasonId, groupId);
        var html = await _client.GetAsync(url, cancellationToken);
        return _resultParser.Parse(html);
    }

    /// <summary>
    /// Gets formatted result data for the given season and group.
    /// </summary>
    public async Task<IReadOnlyList<FormattedResult>> GetFormattedResultsAsync(
        int seasonId,
        string groupId,
        string? dateFormat = null,
        string? timeFormat = null,
        CancellationToken cancellationToken = default)
    {
        var raw = await GetResultsAsync(seasonId, groupId, cancellationToken);
        return _resultFormatter.FormatResults(raw, dateFormat, timeFormat);
    }
}
