using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.JSInterop;

namespace MVDC.Web.Auth;

public class JwtAuthenticationStateProvider : AuthenticationStateProvider
{
    private readonly IJSRuntime _jsRuntime;
    private string? _cachedToken;

    private static readonly AuthenticationState AnonymousState =
        new(new ClaimsPrincipal(new ClaimsIdentity()));

    public JwtAuthenticationStateProvider(IJSRuntime jsRuntime)
    {
        _jsRuntime = jsRuntime;
    }

    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        var token = await GetTokenAsync();
        if (string.IsNullOrEmpty(token))
            return AnonymousState;

        var claims = ParseClaimsFromJwt(token);
        if (claims is null)
            return AnonymousState;

        var identity = new ClaimsIdentity(claims, "jwt");
        return new AuthenticationState(new ClaimsPrincipal(identity));
    }

    public async Task<string?> GetTokenAsync()
    {
        if (_cachedToken is not null)
            return _cachedToken;

        try
        {
            _cachedToken = await _jsRuntime.InvokeAsync<string?>("localStorage.getItem", "authToken");
        }
        catch (InvalidOperationException)
        {
            // JSRuntime not available during prerendering
            return null;
        }

        return _cachedToken;
    }

    public async Task LoginAsync(string token)
    {
        _cachedToken = token;
        await _jsRuntime.InvokeVoidAsync("localStorage.setItem", "authToken", token);
        NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
    }

    public async Task LogoutAsync()
    {
        _cachedToken = null;
        await _jsRuntime.InvokeVoidAsync("localStorage.removeItem", "authToken");
        NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
    }

    private static IEnumerable<Claim>? ParseClaimsFromJwt(string token)
    {
        try
        {
            var handler = new JwtSecurityTokenHandler();
            var jwtToken = handler.ReadJwtToken(token);

            // Check if the token is expired
            if (jwtToken.ValidTo < DateTime.UtcNow)
                return null;

            return jwtToken.Claims;
        }
        catch
        {
            return null;
        }
    }
}
