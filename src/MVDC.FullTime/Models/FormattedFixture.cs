namespace MVDC.FullTime.Models;

/// <summary>
/// A formatted fixture with parsed date, time, and team information.
/// </summary>
public sealed record FormattedFixture
{
    public required string Date { get; init; }
    public required string Time { get; init; }
    public required string Home { get; init; }
    public required string Away { get; init; }
    public required string FixtureType { get; init; }
}
