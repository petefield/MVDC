using MVDC.FullTime.Parsers;
using Xunit;

namespace MVDC.FullTime.Tests.Parsers;

public class TeamParserTests
{
    private readonly TeamParser _parser = new();

    private static string LoadFixturesHtml()
    {
        return File.ReadAllText(Path.Combine("Examples", "example_fixtures.html"));
    }

    [Fact]
    public void Parse_ReturnsNineTeams()
    {
        var html = LoadFixturesHtml();
        var teams = _parser.Parse(html);

        Assert.Equal(9, teams.Count);
    }

    [Fact]
    public void Parse_FirstTeamIsAll()
    {
        var html = LoadFixturesHtml();
        var teams = _parser.Parse(html);

        Assert.Equal("All", teams[0]);
    }

    [Fact]
    public void Parse_ContainsExpectedTeams()
    {
        var html = LoadFixturesHtml();
        var teams = _parser.Parse(html);

        Assert.Contains("Blackburn Eagles JFC - Blue U7S", teams);
        Assert.Contains("Junior Gardeners - U7S", teams);
        Assert.Contains("Junior Hoops JFC - Bears U7S", teams);
        Assert.Contains("Junior Hoops JFC - Cobras U7S", teams);
        Assert.Contains("Junior Hoops JFC - Dragons U7S", teams);
        Assert.Contains("Rosegrove FC - Clarets U7S", teams);
        Assert.Contains("Rosegrove FC - Rangers U7S", teams);
        Assert.Contains("Rossendale United - Yellow U7S", teams);
    }

    [Fact]
    public void Parse_EmptyHtml_ReturnsEmptyList()
    {
        var teams = _parser.Parse("<html><body></body></html>");
        Assert.Empty(teams);
    }

    [Fact]
    public void Parse_NoTeamDropdown_ReturnsEmptyList()
    {
        var teams = _parser.Parse("<html><body><select id='other'><option>Test</option></select></body></html>");
        Assert.Empty(teams);
    }
}
