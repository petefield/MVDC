using HtmlAgilityPack;
using MVDC.FullTime.Helpers;

namespace MVDC.FullTime.Parsers;

/// <summary>
/// Extracts fixture data from the FA Full-Time fixtures page HTML.
/// Each fixture is returned as a string array matching the table row cells.
/// </summary>
public sealed class FixtureParser
{
    /// <summary>
    /// Parses the HTML and returns raw fixture data as arrays of cell values.
    /// </summary>
    public IReadOnlyList<string[]> Parse(string html)
    {
        var doc = new HtmlDocument();
        doc.LoadHtml(html);

        var rows = doc.DocumentNode.SelectNodes("//table//tr");
        if (rows is null)
            return [];

        var fixtures = new List<string[]>();

        foreach (var row in rows)
        {
            var cells = row.SelectNodes("td");
            if (cells is null || cells.Count == 0)
                continue;

            var fixture = cells
                .Select(cell => StringHelper.RemoveWhitespace(cell.InnerText))
                .ToArray();

            fixtures.Add(fixture);
        }

        return fixtures;
    }
}
