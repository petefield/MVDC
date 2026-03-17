using NSubstitute;
using Xunit;

namespace MVDC.FullTime.Tests;

public class DivisionTests
{
    private readonly IFullTimeClient _mockClient;
    private readonly Division _division;

    public DivisionTests()
    {
        _mockClient = Substitute.For<IFullTimeClient>();
        _division = new Division(_mockClient);
    }

    private static string LoadFixturesHtml()
    {
        return File.ReadAllText(Path.Combine("Examples", "example_fixtures.html"));
    }

    private static string LoadResultsHtml()
    {
        return File.ReadAllText(Path.Combine("Examples", "example_results.html"));
    }

    [Fact]
    public async Task GetTeamsAsync_CallsCorrectUrl()
    {
        _mockClient.GetAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(LoadFixturesHtml());

        await _division.GetTeamsAsync(12345, "group-key-1");

        await _mockClient.Received(1).GetAsync(
            "https://fulltime.thefa.com/fixtures.html?selectedSeason=12345&selectedFixtureGroupKey=group-key-1&selectedDateCode=all&selectedRelatedFixtureOption=1&itemsPerPage=100",
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetTeamsAsync_ReturnsTeams()
    {
        _mockClient.GetAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(LoadFixturesHtml());

        var teams = await _division.GetTeamsAsync(12345, "group-key-1");

        Assert.Equal(9, teams.Count);
        Assert.Equal("All", teams[0]);
    }

    [Fact]
    public async Task GetFixturesAsync_CallsCorrectUrl()
    {
        _mockClient.GetAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(LoadFixturesHtml());

        await _division.GetFixturesAsync(12345, "group-key-1");

        await _mockClient.Received(1).GetAsync(
            "https://fulltime.thefa.com/fixtures.html?selectedSeason=12345&selectedFixtureGroupKey=group-key-1&selectedDateCode=all&selectedRelatedFixtureOption=1&previousSelectedFixtureGroupKey=group-key-1&itemsPerPage=10000",
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetFixturesAsync_ReturnsRawFixtures()
    {
        _mockClient.GetAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(LoadFixturesHtml());

        var fixtures = await _division.GetFixturesAsync(12345, "group-key-1");

        Assert.Equal(7, fixtures.Count);
    }

    [Fact]
    public async Task GetFormattedFixturesAsync_ReturnsFormattedFixtures()
    {
        _mockClient.GetAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(LoadFixturesHtml());

        var fixtures = await _division.GetFormattedFixturesAsync(12345, "group-key-1");

        Assert.NotEmpty(fixtures);
        Assert.All(fixtures, f =>
        {
            Assert.NotEmpty(f.Date);
            Assert.NotEmpty(f.Home);
            Assert.NotEmpty(f.Away);
        });
    }

    [Fact]
    public async Task GetResultsAsync_CallsCorrectUrl()
    {
        _mockClient.GetAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(LoadResultsHtml());

        await _division.GetResultsAsync(12345, "group-key-1");

        await _mockClient.Received(1).GetAsync(
            "https://fulltime.thefa.com/results.html?selectedSeason=12345&selectedFixtureGroupKey=group-key-1&selectedDateCode=all&selectedRelatedFixtureOption=1&previousSelectedFixtureGroupKey=group-key-1&itemsPerPage=10000",
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetResultsAsync_ReturnsRawResults()
    {
        _mockClient.GetAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(LoadResultsHtml());

        var results = await _division.GetResultsAsync(12345, "group-key-1");

        Assert.Equal(5, results.Count);
    }

    [Fact]
    public async Task GetFormattedResultsAsync_ReturnsFormattedResults()
    {
        _mockClient.GetAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(LoadResultsHtml());

        var results = await _division.GetFormattedResultsAsync(12345, "group-key-1");

        Assert.NotEmpty(results);
        Assert.All(results, r =>
        {
            Assert.NotEmpty(r.Date);
            Assert.NotEmpty(r.Home);
            Assert.NotEmpty(r.Away);
            Assert.NotEmpty(r.FullScore);
        });
    }

    [Fact]
    public async Task GetFormattedResultsAsync_FirstResult_HasCorrectData()
    {
        _mockClient.GetAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(LoadResultsHtml());

        var results = await _division.GetFormattedResultsAsync(12345, "group-key-1");

        Assert.Equal("29/05/2021", results[0].Date);
        Assert.Equal("09:30", results[0].Time);
        Assert.Equal("0", results[0].HomeScore);
        Assert.Equal("2", results[0].AwayScore);
    }
}
