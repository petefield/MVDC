namespace MVDC.FullTime;

/// <summary>
/// Abstraction for making HTTP requests to the FA Full-Time website.
/// </summary>
public interface IFullTimeClient
{
    /// <summary>
    /// Fetches the HTML content from the specified URL.
    /// </summary>
    Task<string> GetAsync(string url, CancellationToken cancellationToken = default);
}
