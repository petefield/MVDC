namespace MVDC.FullTime;

/// <summary>
/// Default HTTP client for fetching pages from the FA Full-Time website.
/// </summary>
public sealed class FullTimeClient : IFullTimeClient
{
    private readonly HttpClient _httpClient;

    public FullTimeClient(HttpClient? httpClient = null)
    {
        _httpClient = httpClient ?? new HttpClient();
    }

    /// <inheritdoc />
    public async Task<string> GetAsync(string url, CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.GetAsync(url, cancellationToken);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadAsStringAsync(cancellationToken);
    }
}
