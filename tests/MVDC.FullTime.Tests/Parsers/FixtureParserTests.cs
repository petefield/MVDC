using MVDC.FullTime.Parsers;
using Xunit;

namespace MVDC.FullTime.Tests.Parsers;

public class FixtureParserTests
{
    private readonly FixtureParser _parser = new();

    private static string LoadFixturesHtml()
    {
        return File.ReadAllText(Path.Combine("Examples", "example_fixtures.html"));
    }

    [Fact]
    public void Parse_ReturnsSevenFixtures()
    {
        var html = LoadFixturesHtml();
        var fixtures = _parser.Parse(html);

        Assert.Equal(7, fixtures.Count);
    }

    [Fact]
    public void Parse_EachFixtureHasTenCells()
    {
        var html = LoadFixturesHtml();
        var fixtures = _parser.Parse(html);

        foreach (var fixture in fixtures)
        {
            Assert.Equal(10, fixture.Length);
        }
    }

    [Fact]
    public void Parse_FirstFixture_HasCorrectHomeTeam()
    {
        var html = LoadFixturesHtml();
        var fixtures = _parser.Parse(html);

        // Cell index 2 is the home team
        Assert.Equal("Rosegrove FC - Rangers U7S", fixtures[0][2]);
    }

    [Fact]
    public void Parse_FirstFixture_HasCorrectAwayTeam()
    {
        var html = LoadFixturesHtml();
        var fixtures = _parser.Parse(html);

        // Cell index 6 is the away team
        Assert.Equal("Rossendale United - Yellow U7S", fixtures[0][6]);
    }

    [Fact]
    public void Parse_FirstFixture_ScoreIsVS()
    {
        var html = LoadFixturesHtml();
        var fixtures = _parser.Parse(html);

        // Cell index 4 is the score (always "VS" for fixtures)
        Assert.Equal("VS", fixtures[0][4]);
    }

    [Fact]
    public void Parse_EmptyHtml_ReturnsEmptyList()
    {
        var fixtures = _parser.Parse("<html><body></body></html>");
        Assert.Empty(fixtures);
    }

    [Fact]
    public void Parse_NoTableRows_ReturnsEmptyList()
    {
        var fixtures = _parser.Parse("<html><body><table><tr><th>Header</th></tr></table></body></html>");
        Assert.Empty(fixtures);
    }
}
