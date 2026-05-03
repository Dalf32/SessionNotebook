using System.Net.Http.Json;
using Common;
using DataLayer.Entities;
using Microsoft.AspNetCore.Components.Authorization;
using SessionNotebook.Data;

namespace SessionNotebook.Services;

public class SessionService(HttpClient httpClient, AuthenticationStateProvider authStateProvider)
    : ApiService(httpClient, authStateProvider)
{
    public async Task<SearchResults<Session>> ListSessions(int campaignId, SearchCriteria criteria = null)
    {
        await SetAuthHeader();
        var queryParams = criteria == null ? string.Empty : BuildQueryParams(criteria.GetParamPairs());

        if (criteria is { TagId: not null })
        {
            return await HttpClient.GetFromJsonAsync<SearchResults<Session>>(
                $"Session/Campaign/{campaignId}/Tag/{criteria.TagId}{queryParams}");
        }

        return await HttpClient.GetFromJsonAsync<SearchResults<Session>>($"Session/Campaign/{campaignId}{queryParams}");
    }

    public async Task<Session> GetSession(int id)
    {
        await SetAuthHeader();
        return await HttpClient.GetFromJsonAsync<Session>($"Session/{id}");
    }

    public async Task<Session> SaveSession(Session session)
    {
        await SetAuthHeader();
        if (session.Id == 0)
        {
            //Create
            var response = await HttpClient.PutAsJsonAsync("Session", session, JsonOpts);
            response.EnsureSuccessStatusCode();
            session = await response.Content.ReadFromJsonAsync<Session>();
        }
        else
        {
            //Update
            var response = await HttpClient.PatchAsJsonAsync("Session", session, JsonOpts);
            response.EnsureSuccessStatusCode();
        }

        return session;
    }

    public async Task DeleteSession(int id)
    {
        await SetAuthHeader();
        await HttpClient.DeleteAsync($"Session/{id}");
    }

    public async Task UpdateTags(int id, List<Tag> tags)
    {
        await SetAuthHeader();
        var response = await HttpClient.PatchAsJsonAsync($"Session/{id}/Tags", tags);
        response.EnsureSuccessStatusCode();
    }
}
