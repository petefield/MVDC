using System.Net.Http.Headers;

namespace MVDC.Web.Auth;

public class JwtAuthorizationHandler : DelegatingHandler
{
    private readonly JwtAuthenticationStateProvider _authStateProvider;

    public JwtAuthorizationHandler(JwtAuthenticationStateProvider authStateProvider)
    {
        _authStateProvider = authStateProvider;
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var token = await _authStateProvider.GetTokenAsync();
        if (!string.IsNullOrEmpty(token))
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }

        return await base.SendAsync(request, cancellationToken);
    }
}
