using MVDC.FullTime.Parsers;
using Xunit;

namespace MVDC.FullTime.Tests.Parsers;

public class ResultParserTests
{
    private readonly ResultParser _parser = new();

    private static string LoadResultsHtml()
    {
        return File.ReadAllText(Path.Combine("Examples", "example_results.html"));
    }

    [Fact]
    public void Parse_ReturnsFiveResults()
    {
        var html = LoadResultsHtml();
        var results = _parser.Parse(html);

        Assert.Equal(5, results.Count);
    }

    [Fact]
    public void Parse_EachResultHasFiveElements()
    {
        var html = LoadResultsHtml();
        var results = _parser.Parse(html);

        foreach (var result in results)
        {
            Assert.Equal(5, result.Length);
        }
    }

    [Fact]
    public void Parse_FirstResult_HasCorrectHomeTeam()
    {
        var html = LoadResultsHtml();
        var results = _parser.Parse(html);

        Assert.Contains("Mill Hill Juniors - Red U14S", results[0][1]);
    }

    [Fact]
    public void Parse_FirstResult_HasCorrectScore()
    {
        var html = LoadResultsHtml();
        var results = _parser.Parse(html);

        Assert.Contains("0 - 2", results[0][2]);
    }

    [Fact]
    public void Parse_FirstResult_HasCorrectAwayTeam()
    {
        var html = LoadResultsHtml();
        var results = _parser.Parse(html);

        Assert.Contains("Wilpshire Wanderers - Blue U14S", results[0][3]);
    }

    [Fact]
    public void Parse_EmptyHtml_ReturnsEmptyList()
    {
        var results = _parser.Parse("<html><body></body></html>");
        Assert.Empty(results);
    }

    [Fact]
    public void Parse_NoResultsList_ReturnsEmptyList()
    {
        var results = _parser.Parse("<html><body><div id='other'>content</div></body></html>");
        Assert.Empty(results);
    }
}
