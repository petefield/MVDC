namespace MVDC.FullTime;

/// <summary>
/// Default HTTP client for fetching pages from the FA Full-Time website.
/// Accepts an <see cref="HttpClient"/> via constructor injection to avoid socket exhaustion.
/// </summary>
public sealed class FullTimeClient : IFullTimeClient
{
    private readonly HttpClient _httpClient;

    public FullTimeClient(HttpClient httpClient)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
    }

    /// <inheritdoc />
    public async Task<string> GetAsync(string url, CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.GetAsync(url, cancellationToken);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadAsStringAsync(cancellationToken);
    }
}
