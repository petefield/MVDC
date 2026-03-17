namespace MVDC.FullTime.Models;

/// <summary>
/// A formatted result with parsed date, time, teams, and scores.
/// </summary>
public sealed record FormattedResult
{
    public required string Date { get; init; }
    public required string Time { get; init; }
    public required string Home { get; init; }
    public required string HomeScore { get; init; }
    public required string Away { get; init; }
    public required string AwayScore { get; init; }
    public required string FullScore { get; init; }
}
