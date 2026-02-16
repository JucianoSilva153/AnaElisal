using System.Net.Http;
using System.Net.Http.Headers;

namespace Elisal.WasteManagement.UI.Services;

public class JwtInterceptor : DelegatingHandler
{
    private readonly LocalStorageService _localStorage;
    private readonly IServiceProvider _serviceProvider;

    public JwtInterceptor(LocalStorageService localStorage, IServiceProvider serviceProvider)
    {
        _localStorage = localStorage;
        _serviceProvider = serviceProvider;
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        var token = await _localStorage.GetItemAsync<string>("authToken");

        if (!string.IsNullOrEmpty(token))
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }

        var response = await base.SendAsync(request, cancellationToken);

        if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
        {
            // Resolve provider from service provider to avoid circular dependency
            var authProvider =
                _serviceProvider.GetService(
                        typeof(Microsoft.AspNetCore.Components.Authorization.AuthenticationStateProvider)) as
                    ApiAuthenticationStateProvider;
            if (authProvider != null)
            {
                await authProvider.MarkUserAsLoggedOut();
            }
        }

        return response;
    }
}
