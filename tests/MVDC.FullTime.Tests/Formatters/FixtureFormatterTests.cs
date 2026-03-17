using MVDC.FullTime.Formatters;
using Xunit;

namespace MVDC.FullTime.Tests.Formatters;

public class FixtureFormatterTests
{
    private readonly FixtureFormatter _formatter = new();

    /// <summary>
    /// Creates a raw fixture array matching the structure from FixtureParser:
    /// [type, dateTime, home, logo, "VS", logo, away, venue, competition, status]
    /// </summary>
    private static string[] MakeFixture(string type, string dateTime, string home, string away)
    {
        return [type, dateTime, home, "", "VS", "", away, "", "UNDER 07S", ""];
    }

    [Fact]
    public void FormatFixtures_ParsesDateAndTime()
    {
        var fixtures = new List<string[]>
        {
            MakeFixture("L", "05/02/22 09:10", "Home FC", "Away FC")
        };

        var result = _formatter.FormatFixtures(fixtures);

        Assert.Single(result);
        Assert.Equal("05/02/2022", result[0].Date);
        Assert.Equal("09:10", result[0].Time);
    }

    [Fact]
    public void FormatFixtures_ExtractsTeams()
    {
        var fixtures = new List<string[]>
        {
            MakeFixture("L", "05/02/22 09:10", "Home FC", "Away FC")
        };

        var result = _formatter.FormatFixtures(fixtures);

        Assert.Equal("Home FC", result[0].Home);
        Assert.Equal("Away FC", result[0].Away);
    }

    [Fact]
    public void FormatFixtures_SetsFixtureType()
    {
        var fixtures = new List<string[]>
        {
            MakeFixture("L", "05/02/22 09:10", "Home FC", "Away FC")
        };

        var result = _formatter.FormatFixtures(fixtures);

        Assert.Equal("L", result[0].FixtureType);
    }

    [Fact]
    public void FormatFixtures_CustomDateFormat()
    {
        var fixtures = new List<string[]>
        {
            MakeFixture("L", "05/02/22 09:10", "Home FC", "Away FC")
        };

        var result = _formatter.FormatFixtures(fixtures, dateFormat: "yyyy-MM-dd");

        Assert.Equal("2022-02-05", result[0].Date);
    }

    [Fact]
    public void FormatFixtures_CustomTimeFormat()
    {
        var fixtures = new List<string[]>
        {
            MakeFixture("L", "05/02/22 14:30", "Home FC", "Away FC")
        };

        var result = _formatter.FormatFixtures(fixtures, timeFormat: "h:mm tt");

        Assert.Equal("2:30 PM", result[0].Time);
    }

    [Fact]
    public void FormatFixtures_TbcFixture_IncludedByDefault()
    {
        var fixtures = new List<string[]>
        {
            new[] { "L", "TBC", "Home FC", "", "VS", "", "Away FC", "", "UNDER 07S", "" }
        };

        var result = _formatter.FormatFixtures(fixtures);

        Assert.Single(result);
        Assert.Equal("TBC", result[0].Date);
        Assert.Equal("TBC", result[0].Time);
        Assert.Equal("Home FC", result[0].Home);
        Assert.Equal("Away FC", result[0].Away);
    }

    [Fact]
    public void FormatFixtures_TbcFixture_ExcludedWhenFlagged()
    {
        var fixtures = new List<string[]>
        {
            new[] { "L", "TBC", "Home FC", "", "VS", "", "Away FC", "", "UNDER 07S", "" }
        };

        var result = _formatter.FormatFixtures(fixtures, includeTbcFixtures: false);

        Assert.Empty(result);
    }

    [Fact]
    public void FormatFixtures_CupFixture_IncludedByDefault()
    {
        var fixtures = new List<string[]>
        {
            MakeFixture("Cup", "05/02/22 09:10", "Home FC", "Away FC")
        };

        var result = _formatter.FormatFixtures(fixtures);

        Assert.Single(result);
        Assert.Equal("Cup", result[0].FixtureType);
    }

    [Fact]
    public void FormatFixtures_CupFixture_ExcludedWhenFlagged()
    {
        var fixtures = new List<string[]>
        {
            MakeFixture("Cup", "05/02/22 09:10", "Home FC", "Away FC")
        };

        var result = _formatter.FormatFixtures(fixtures, includeCupFixtures: false);

        Assert.Empty(result);
    }

    [Fact]
    public void FormatFixtures_EmptyArray_ReturnsEmpty()
    {
        var result = _formatter.FormatFixtures(new List<string[]>());
        Assert.Empty(result);
    }

    [Fact]
    public void FormatFixtures_InvalidDate_IsSkipped()
    {
        var fixtures = new List<string[]>
        {
            MakeFixture("L", "not-a-date", "Home FC", "Away FC")
        };

        var result = _formatter.FormatFixtures(fixtures);

        Assert.Empty(result);
    }

    [Fact]
    public void FormatFixtures_MultipleFixtures_AllFormatted()
    {
        var fixtures = new List<string[]>
        {
            MakeFixture("L", "05/02/22 09:10", "Team A", "Team B"),
            MakeFixture("L", "19/02/22 11:15", "Team C", "Team D"),
            MakeFixture("L", "12/03/22 09:55", "Team E", "Team F")
        };

        var result = _formatter.FormatFixtures(fixtures);

        Assert.Equal(3, result.Count);
        Assert.Equal("Team A", result[0].Home);
        Assert.Equal("Team C", result[1].Home);
        Assert.Equal("Team E", result[2].Home);
    }
}
