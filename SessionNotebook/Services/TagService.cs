using System.Net.Http.Json;
using Common;
using DataLayer.Entities;
using Microsoft.AspNetCore.Components.Authorization;
using SessionNotebook.Data;

namespace SessionNotebook.Services;

public class TagService(HttpClient httpClient, AuthenticationStateProvider authStateProvider)
    : ApiService(httpClient, authStateProvider)
{
    public async Task<SearchResults<Tag>> ListTags(int campaignId, Type associatedType = null, SearchCriteria criteria = null)
    {
        await SetAuthHeader();
        string queryParams;

        if (associatedType != null)
        {
            queryParams = BuildQueryParams(("associatedType", associatedType.Name),
                SearchCriteria.SortDirParam(true));

            return await HttpClient.GetFromJsonAsync<SearchResults<Tag>>($"Tag/Campaign/{campaignId}{queryParams}");
        }

        queryParams = criteria == null ? BuildQueryParams(SearchCriteria.SortDirParam(true)) :
            BuildQueryParams(criteria.GetParamPairs());
        return await HttpClient.GetFromJsonAsync<SearchResults<Tag>>($"Tag/Campaign/{campaignId}{queryParams}");
    }

    public async Task<List<Tag>> ListAllTags(int campaignId, Type associatedType = null)
    {
        return (await ListTags(campaignId, associatedType, SearchCriteria.NoPaging)).Results;
    }

    public async Task<Tag> GetTag(int id)
    {
        await SetAuthHeader();
        return await HttpClient.GetFromJsonAsync<Tag>($"Tag/{id}");
    }

    public async Task<Tag> SaveTag(Tag tag)
    {
        await SetAuthHeader();
        if (tag.Id == 0)
        {
            var response = await HttpClient.PutAsJsonAsync("Tag", tag, JsonOpts);
            response.EnsureSuccessStatusCode();
            tag = await response.Content.ReadFromJsonAsync<Tag>();
        }
        else
        {
            var response = await HttpClient.PatchAsJsonAsync("Tag", tag, JsonOpts);
            response.EnsureSuccessStatusCode();
        }

        return tag;
    }

    public async Task<List<Tag>> SaveTags(List<Tag> tags)
    {
        if (tags.Count == 0) return tags;

        var allTags = await ListAllTags(tags.First().CampaignId);
        var result = tags.Select(tag =>
        {
            var existingTag = allTags.FirstOrDefault(t => t.Name == tag.Name);
            if (tag.Id == 0 && existingTag != null)
            {
                existingTag.MergeFlags(tag);
                return SaveTag(existingTag);
            }

            return SaveTag(tag);
        });

        return (await Task.WhenAll(result)).ToList();
    }

    public async Task DeleteTag(int id)
    {
        await SetAuthHeader();
        await HttpClient.DeleteAsync($"Tag/{id}");
    }

    public static void UpdateTags(List<Tag> currentTags, List<string> tagNames, Func<string, Tag> createTagFunc)
    {
        var tagsToAdd = tagNames.Except(currentTags.Select(t => t.Name)).Select(createTagFunc).ToList();
        currentTags.RemoveAll(t => !tagNames.Contains(t.Name));
        currentTags.AddRange(tagsToAdd);
    }
}
