using HtmlAgilityPack;
using MVDC.FullTime.Helpers;

namespace MVDC.FullTime.Parsers;

/// <summary>
/// Extracts result data from the FA Full-Time results page HTML.
/// Each result is returned as a string array: [dateTime, homeTeam, score, awayTeam, division].
/// </summary>
public sealed class ResultParser
{
    /// <summary>
    /// Parses the HTML and returns raw result data as arrays.
    /// </summary>
    public IReadOnlyList<string[]> Parse(string html)
    {
        var doc = new HtmlDocument();
        doc.LoadHtml(html);

        var resultNodes = doc.DocumentNode.SelectNodes(
            "//*[@id='results-list']/div/div[3]/div/div[2]/div");

        if (resultNodes is null)
            return [];

        var results = new List<string[]>();

        foreach (var node in resultNodes)
        {
            var dateTime = StringHelper.RemoveWhitespace(
                ExtractNodeContent(node, ".//div[contains(@class, 'datetime-col')]"));
            var homeTeam = ExtractNodeContent(node, ".//div[contains(@class, 'home-team-col')]");
            var score = ExtractNodeContent(node, ".//div[contains(@class, 'score-col')]");
            var awayTeam = ExtractNodeContent(node, ".//div[contains(@class, 'road-team-col')]");
            var division = ExtractNodeContent(node, ".//div[contains(@class, 'fg-col')]");

            results.Add([dateTime, homeTeam, score, awayTeam, division]);
        }

        return results;
    }

    private static string ExtractNodeContent(HtmlNode contextNode, string xpath)
    {
        var node = contextNode.SelectSingleNode(xpath);
        return node is not null ? node.InnerText.Trim() : string.Empty;
    }
}
