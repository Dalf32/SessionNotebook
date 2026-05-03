using System.Diagnostics.CodeAnalysis;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text.Json;
using Microsoft.AspNetCore.Components.WebAssembly.Authentication;
using Microsoft.AspNetCore.Components.WebAssembly.Authentication.Internal;
using Microsoft.JSInterop;

namespace SessionNotebook.Providers;

public class ApiAuthAccountFactory: AccountClaimsPrincipalFactory<RemoteUserAccount>
{
    private readonly HttpClient _httpClient;
    private readonly IJSRuntime _jsRuntime;
    private readonly IConfiguration _configuration;

    public ApiAuthAccountFactory(IAccessTokenProviderAccessor accessor,
        HttpClient httpClient, IJSRuntime jsRuntime, IConfiguration configuration) : base(accessor)
    {
        _httpClient = httpClient;
        _jsRuntime = jsRuntime;
        _configuration = configuration;
    }

    public override async ValueTask<ClaimsPrincipal> CreateUserAsync(RemoteUserAccount account,
        RemoteAuthenticationUserOptions options)
    {
        var user = await base.CreateUserAsync(account, options);

        if (user is not { Identity.IsAuthenticated: true }) return user;

        var userData = await GetOidcUserData();
        if (userData.Email == null)
        {
            //TODO: Verify if this works, intent is to de-auth the user
            ((ClaimsIdentity)user.Identity).AddClaim(new Claim(ClaimTypes.Authentication, "false"));
            return user;
        }

        ((ClaimsIdentity)user.Identity).AddClaim(new Claim("ApiJwt", userData.id_token));
        await LogInViaApi(userData.id_token);

        return user;
    }

    private async Task<OidcUserData> GetOidcUserData()
    {
        var userDataKey = $"oidc.user:{_configuration["Oidc:Authority"]}:{_configuration["Oidc:ClientId"]}";
        var userDataJson = await _jsRuntime.InvokeAsync<string>("sessionStorage.getItem", userDataKey);
        return JsonSerializer.Deserialize<OidcUserData>(userDataJson);
    }

    private async Task LogInViaApi(string idToken)
    {
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", idToken);

        var response = await _httpClient.PostAsync("/Account/EnsureUser", null);
        response.EnsureSuccessStatusCode();
    }

    [SuppressMessage("ReSharper", "InconsistentNaming")]
    private class OidcUserData
    {
        public string id_token { get; set; }
        public string scope { get; set; }
        // ReSharper disable once CollectionNeverUpdated.Local
        public Dictionary<string, object> profile { get; set; } = new();

        public string Email => profile["email"].ToString();
    }
}
