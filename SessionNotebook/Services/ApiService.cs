using System.Net.Http.Headers;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Components.Authorization;

namespace SessionNotebook.Services;

public abstract class ApiService(HttpClient httpClient, AuthenticationStateProvider authStateProvider)
{
    protected readonly HttpClient HttpClient = httpClient;
    protected readonly JsonSerializerOptions JsonOpts = new()
    {
        ReferenceHandler = ReferenceHandler.IgnoreCycles
    };

    private bool _isTokenSet;

    protected async Task SetAuthHeader()
    {
        if (_isTokenSet)
        {
            return;
        }

        var authState = await authStateProvider.GetAuthenticationStateAsync();
        var user = authState.User;
        var idToken = user.Claims.FirstOrDefault(c => c.Type == "ApiJwt")?.Value;

        HttpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", idToken);
        _isTokenSet = true;
    }

    protected static string BuildQueryParams(params (string paramName, string paramValue)[] paramPairs)
    {
        var queryParams = "?" + string.Join('&', paramPairs
            .Where(pair => !string.IsNullOrEmpty(pair.paramValue))
            .Select(pair => $"{pair.paramName}={pair.paramValue}"));
        
        return queryParams.Length == 1 ? string.Empty : queryParams;
    }
}
