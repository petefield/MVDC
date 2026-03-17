using MVDC.FullTime.Formatters;
using Xunit;

namespace MVDC.FullTime.Tests.Formatters;

public class ResultFormatterTests
{
    private readonly ResultFormatter _formatter = new();

    /// <summary>
    /// Creates a raw result array matching the structure from ResultParser:
    /// [dateTime, homeTeam, score, awayTeam, division]
    /// </summary>
    private static string[] MakeResult(string dateTime, string home, string score, string away, string division = "UNDER 14S")
    {
        return [dateTime, home, score, away, division];
    }

    [Fact]
    public void FormatResults_ParsesDateAndTime()
    {
        var results = new List<string[]>
        {
            MakeResult("29/05/21 09:30", "Home FC", "0 - 2", "Away FC")
        };

        var formatted = _formatter.FormatResults(results);

        Assert.Single(formatted);
        Assert.Equal("29/05/2021", formatted[0].Date);
        Assert.Equal("09:30", formatted[0].Time);
    }

    [Fact]
    public void FormatResults_ExtractsTeams()
    {
        var results = new List<string[]>
        {
            MakeResult("29/05/21 09:30", "Home FC", "0 - 2", "Away FC")
        };

        var formatted = _formatter.FormatResults(results);

        Assert.Equal("Home FC", formatted[0].Home);
        Assert.Equal("Away FC", formatted[0].Away);
    }

    [Fact]
    public void FormatResults_ExtractsHomeScore()
    {
        var results = new List<string[]>
        {
            MakeResult("29/05/21 09:30", "Home FC", "3 - 1", "Away FC")
        };

        var formatted = _formatter.FormatResults(results);

        Assert.Equal("3", formatted[0].HomeScore);
    }

    [Fact]
    public void FormatResults_ExtractsAwayScore()
    {
        var results = new List<string[]>
        {
            MakeResult("29/05/21 09:30", "Home FC", "3 - 1", "Away FC")
        };

        var formatted = _formatter.FormatResults(results);

        Assert.Equal("1", formatted[0].AwayScore);
    }

    [Fact]
    public void FormatResults_PreservesFullScore()
    {
        var results = new List<string[]>
        {
            MakeResult("29/05/21 09:30", "Home FC", "0 - 2", "Away FC")
        };

        var formatted = _formatter.FormatResults(results);

        Assert.Equal("0 - 2", formatted[0].FullScore);
    }

    [Fact]
    public void FormatResults_DrawScore()
    {
        var results = new List<string[]>
        {
            MakeResult("22/05/21 11:30", "Home FC", "2 - 2", "Away FC")
        };

        var formatted = _formatter.FormatResults(results);

        Assert.Equal("2", formatted[0].HomeScore);
        Assert.Equal("2", formatted[0].AwayScore);
    }

    [Fact]
    public void FormatResults_HighScore()
    {
        var results = new List<string[]>
        {
            MakeResult("15/05/21 11:00", "Home FC", "12 - 3", "Away FC")
        };

        var formatted = _formatter.FormatResults(results);

        Assert.Equal("12", formatted[0].HomeScore);
        Assert.Equal("3", formatted[0].AwayScore);
    }

    [Fact]
    public void FormatResults_CustomDateFormat()
    {
        var results = new List<string[]>
        {
            MakeResult("29/05/21 09:30", "Home FC", "0 - 2", "Away FC")
        };

        var formatted = _formatter.FormatResults(results, dateFormat: "yyyy-MM-dd");

        Assert.Equal("2021-05-29", formatted[0].Date);
    }

    [Fact]
    public void FormatResults_CustomTimeFormat()
    {
        var results = new List<string[]>
        {
            MakeResult("29/05/21 14:30", "Home FC", "0 - 2", "Away FC")
        };

        var formatted = _formatter.FormatResults(results, timeFormat: "h:mm tt");

        Assert.Equal("2:30 PM", formatted[0].Time);
    }

    [Fact]
    public void FormatResults_EmptyList_ReturnsEmpty()
    {
        var formatted = _formatter.FormatResults(new List<string[]>());
        Assert.Empty(formatted);
    }

    [Fact]
    public void FormatResults_InvalidDate_IsSkipped()
    {
        var results = new List<string[]>
        {
            MakeResult("not-a-date", "Home FC", "0 - 2", "Away FC")
        };

        var formatted = _formatter.FormatResults(results);

        Assert.Empty(formatted);
    }

    [Fact]
    public void FormatResults_TooFewElements_IsSkipped()
    {
        var results = new List<string[]>
        {
            new[] { "29/05/21 09:30", "Home FC", "0 - 2" } // only 3 elements, need 4+
        };

        var formatted = _formatter.FormatResults(results);

        Assert.Empty(formatted);
    }

    [Fact]
    public void FormatResults_MultipleResults_AllFormatted()
    {
        var results = new List<string[]>
        {
            MakeResult("29/05/21 09:30", "Team A", "0 - 2", "Team B"),
            MakeResult("22/05/21 11:30", "Team C", "2 - 2", "Team D"),
            MakeResult("15/05/21 11:45", "Team E", "1 - 3", "Team F")
        };

        var formatted = _formatter.FormatResults(results);

        Assert.Equal(3, formatted.Count);
        Assert.Equal("Team A", formatted[0].Home);
        Assert.Equal("Team C", formatted[1].Home);
        Assert.Equal("Team E", formatted[2].Home);
    }
}
