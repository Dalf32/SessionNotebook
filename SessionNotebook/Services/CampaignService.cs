using System.Net.Http.Json;
using Common;
using DataLayer.Entities;
using Microsoft.AspNetCore.Components.Authorization;
using SessionNotebook.Data;

namespace SessionNotebook.Services;

public class CampaignService(HttpClient httpClient, AuthenticationStateProvider authStateProvider)
    : ApiService(httpClient, authStateProvider)
{
    public async Task<SearchResults<Campaign>> ListCampaigns(SearchCriteria criteria = null)
    {
        await SetAuthHeader();
        var queryParams = criteria == null ? string.Empty : BuildQueryParams(criteria.GetParamPairs());
        
        return await HttpClient.GetFromJsonAsync<SearchResults<Campaign>>($"Campaign{queryParams}");
    }

    public async Task<Campaign> GetCampaign(int id)
    {
        await SetAuthHeader();
        return await HttpClient.GetFromJsonAsync<Campaign>($"Campaign/{id}");
    }

    public async Task<Campaign> SaveCampaign(Campaign campaign)
    {
        await SetAuthHeader();
        if (campaign.Id == 0)
        {
            //Create
            var response = await HttpClient.PutAsJsonAsync("Campaign", campaign);
            response.EnsureSuccessStatusCode();
            campaign = await response.Content.ReadFromJsonAsync<Campaign>();
        }
        else
        {
            //Update
            var response = await HttpClient.PatchAsJsonAsync("Campaign", campaign);
            response.EnsureSuccessStatusCode();
        }

        return campaign;
    }

    public async Task DeleteCampaign(int id)
    {
        await SetAuthHeader();
        await HttpClient.DeleteAsync($"Campaign/{id}");
    }

    public async Task<int> GetCampaignIdForSession(int sessionId)
    {
        await SetAuthHeader();
        return await HttpClient.GetFromJsonAsync<int>($"Campaign/ForSession/{sessionId}");
    }
    
    public async Task<int> GetCampaignIdForNote(int noteId)
    {
        await SetAuthHeader();
        return await HttpClient.GetFromJsonAsync<int>($"Campaign/ForNote/{noteId}");
    }
    
    public async Task<int> GetCampaignIdForNoun(int nounId)
    {
        await SetAuthHeader();
        return await HttpClient.GetFromJsonAsync<int>($"Campaign/ForNoun/{nounId}");
    }
    
    public async Task<int> GetCampaignIdForTag(int tagId)
    {
        await SetAuthHeader();
        return await HttpClient.GetFromJsonAsync<int>($"Campaign/ForTag/{tagId}");
    }
}
