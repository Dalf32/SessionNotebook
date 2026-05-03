using System.Net.Http.Json;
using Common;
using DataLayer.Entities;
using Microsoft.AspNetCore.Components.Authorization;
using SessionNotebook.Data;

namespace SessionNotebook.Services;

public class NounService(HttpClient httpClient, AuthenticationStateProvider authStateProvider)
    : ApiService(httpClient, authStateProvider)
{
    public async Task<SearchResults<Noun>> ListNouns(int campaignId, SearchCriteria criteria = null)
    {
        await SetAuthHeader();
        var queryParams = criteria == null ? BuildQueryParams(SearchCriteria.SortDirParam(true)) :
            BuildQueryParams(criteria.GetParamPairs());

        if (criteria is { TagId: not null })
        {
            return await HttpClient.GetFromJsonAsync<SearchResults<Noun>>(
                $"Noun/Campaign/{campaignId}/Tag/{criteria.TagId}{queryParams}");
        }

        return await HttpClient.GetFromJsonAsync<SearchResults<Noun>>($"Noun/Campaign/{campaignId}{queryParams}");
    }

    public async Task<List<Noun>> ListAllNouns(int campaignId)
    {
        return (await ListNouns(campaignId, SearchCriteria.NoPaging)).Results;
    }

    public async Task<Noun> GetNoun(int id)
    {
        await SetAuthHeader();
        return await HttpClient.GetFromJsonAsync<Noun>($"Noun/{id}");
    }

    public async Task<Noun> SaveNoun(Noun noun)
    {
        await SetAuthHeader();
        if (noun.Id == 0)
        {
            var response = await HttpClient.PutAsJsonAsync("Noun", noun);
            response.EnsureSuccessStatusCode();
            noun = await response.Content.ReadFromJsonAsync<Noun>();
        }
        else
        {
            var response = await HttpClient.PatchAsJsonAsync("Noun", noun);
            response.EnsureSuccessStatusCode();
        }

        return noun;
    }

    public async Task<List<Noun>> SaveNouns(List<Noun> nouns)
    {
        if (nouns.Count == 0) return nouns;

        var result = nouns.Select(noun => noun.Id == 0 ? SaveNoun(noun) : Task.FromResult(noun));
        return (await Task.WhenAll(result)).ToList();
    }

    public async Task DeleteNoun(int id)
    {
        await SetAuthHeader();
        await HttpClient.DeleteAsync($"Noun/{id}");
    }

    public async Task UpdateTags(int id, List<Tag> tags)
    {
        await SetAuthHeader();
        var response = await HttpClient.PatchAsJsonAsync($"Noun/{id}/Tags", tags);
        response.EnsureSuccessStatusCode();
    }
}
