using System.ComponentModel.DataAnnotations;

namespace MVDC.Shared.Models;

public class Fixture
{
    public string Id { get; set; } = Guid.NewGuid().ToString();

    [Required]
    public string TeamId { get; set; } = string.Empty;

    [Required]
    public string Opponent { get; set; } = string.Empty;

    [Required]
    public DateTime Date { get; set; }

    [Required]
    public string Venue { get; set; } = string.Empty;

    public bool IsHome { get; set; }

    /// <summary>
    /// The Full-Time fixture group key this fixture was sourced from (e.g. "1_478076013").
    /// </summary>
    public string FixtureGroupKey { get; set; } = string.Empty;

    public string DocumentType { get; init; } = nameof(Fixture);
}
