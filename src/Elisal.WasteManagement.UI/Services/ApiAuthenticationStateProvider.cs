using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text.Json;
using Microsoft.AspNetCore.Components.Authorization;

namespace Elisal.WasteManagement.UI.Services;

public class ApiAuthenticationStateProvider : AuthenticationStateProvider
{
    private readonly HttpClient _httpClient;
    private readonly LocalStorageService _localStorage;

    public ApiAuthenticationStateProvider(HttpClient httpClient, LocalStorageService localStorage)
    {
        _httpClient = httpClient;
        _localStorage = localStorage;
    }

    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        var savedToken = await _localStorage.GetItemAsync<string>("authToken");

        if (string.IsNullOrWhiteSpace(savedToken))
        {
            return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
        }

        // Check for token expiration
        var claims = ParseClaimsFromJwt(savedToken);
        var expiryClaim = claims.FirstOrDefault(c => c.Type == "exp")?.Value;

        if (expiryClaim != null && long.TryParse(expiryClaim, out var expiryTime))
        {
            var expiryDateTime = DateTimeOffset.FromUnixTimeSeconds(expiryTime).UtcDateTime;
            if (expiryDateTime < DateTime.UtcNow)
            {
                // Token expired
                await MarkUserAsLoggedOut();
                return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
            }
        }

        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", savedToken);

        return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity(claims, "jwt")));
    }

    public void MarkUserAsAuthenticated(string token)
    {
        // FIX: Parse JWT token to extract all claims including roles
        // Previously only created a Name claim, causing AuthorizeView to not work until page reload
        var claims = ParseClaimsFromJwt(token);
        var authenticatedUser = new ClaimsPrincipal(new ClaimsIdentity(claims, "jwt"));
        var authState = Task.FromResult(new AuthenticationState(authenticatedUser));
        NotifyAuthenticationStateChanged(authState);
    }

    public async Task MarkUserAsLoggedOut()
    {
        await _localStorage.RemoveItemAsync("authToken");
        await _localStorage.RemoveItemAsync("refreshToken");
        await _localStorage.RemoveItemAsync("user");

        var anonymousUser = new ClaimsPrincipal(new ClaimsIdentity());
        var authState = Task.FromResult(new AuthenticationState(anonymousUser));
        NotifyAuthenticationStateChanged(authState);
    }

    private IEnumerable<Claim> ParseClaimsFromJwt(string jwt)
    {
        var claims = new List<Claim>();
        var payload = jwt.Split('.')[1];

        var jsonBytes = ParseBase64WithoutPadding(payload);

        var keyValuePairs = JsonSerializer.Deserialize<Dictionary<string, object>>(jsonBytes);

        if (keyValuePairs != null)
        {
            keyValuePairs.TryGetValue(ClaimTypes.Role, out object? roles);
            if (roles == null) keyValuePairs.TryGetValue("role", out roles);

            if (roles != null)
            {
                if (roles.ToString()!.Trim().StartsWith("["))
                {
                    var parsedRoles = JsonSerializer.Deserialize<string[]>(roles.ToString()!);

                    foreach (var parsedRole in parsedRoles!)
                    {
                        claims.Add(new Claim(ClaimTypes.Role, parsedRole));
                    }
                }
                else
                {
                    claims.Add(new Claim(ClaimTypes.Role, roles.ToString()!));
                }

                keyValuePairs.Remove(ClaimTypes.Role);
                keyValuePairs.Remove("role");
            }

            keyValuePairs.TryGetValue(ClaimTypes.Name, out object? name);
            if (name == null) keyValuePairs.TryGetValue("unique_name", out name);
            if (name != null)
            {
                claims.Add(new Claim(ClaimTypes.Name, name.ToString()!));
                keyValuePairs.Remove(ClaimTypes.Name);
                keyValuePairs.Remove("unique_name");
            }

            keyValuePairs.TryGetValue(ClaimTypes.NameIdentifier, out object? userId);
            if (userId == null) keyValuePairs.TryGetValue("nameid", out userId);
            if (userId != null)
            {
                claims.Add(new Claim(ClaimTypes.NameIdentifier, userId.ToString()!));
                keyValuePairs.Remove(ClaimTypes.NameIdentifier);
                keyValuePairs.Remove("nameid");
            }

            claims.AddRange(keyValuePairs.Select(kvp => new Claim(kvp.Key, kvp.Value.ToString()!)));
        }

        return claims;
    }

    private byte[] ParseBase64WithoutPadding(string base64)
    {
        switch (base64.Length % 4)
        {
            case 2: base64 += "=="; break;
            case 3: base64 += "="; break;
        }

        return Convert.FromBase64String(base64);
    }
}
