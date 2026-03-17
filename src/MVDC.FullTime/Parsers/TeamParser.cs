using HtmlAgilityPack;
using MVDC.FullTime.Helpers;

namespace MVDC.FullTime.Parsers;

/// <summary>
/// Extracts team names from the FA Full-Time fixtures page.
/// </summary>
public sealed class TeamParser
{
    /// <summary>
    /// Parses the HTML of a fixtures page and extracts team names
    /// from the team filter dropdown.
    /// </summary>
    public IReadOnlyList<string> Parse(string html)
    {
        var doc = new HtmlDocument();
        doc.LoadHtml(html);

        var optionNodes = doc.DocumentNode.SelectNodes("//*[@id='form1_selectedTeam']/option");
        if (optionNodes is null)
            return [];

        return optionNodes
            .Select(node => StringHelper.RemoveWhitespace(node.InnerText))
            .ToList();
    }
}
