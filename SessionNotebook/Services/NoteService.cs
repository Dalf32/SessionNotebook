using System.Net.Http.Json;
using Common;
using DataLayer.Entities;
using Microsoft.AspNetCore.Components.Authorization;
using SessionNotebook.Data;

namespace SessionNotebook.Services;

public class NoteService(HttpClient httpClient, AuthenticationStateProvider authStateProvider)
    : ApiService(httpClient, authStateProvider)
{
    public async Task<SearchResults<Note>> ListNotes(int campaignId, SearchCriteria criteria = null)
    {
        await SetAuthHeader();
        var queryParams = criteria == null ? string.Empty : BuildQueryParams(criteria.GetParamPairs());

        if (criteria is { TagId: not null })
        {
            return await HttpClient.GetFromJsonAsync<SearchResults<Note>>(
                $"Note/Campaign/{campaignId}/Tag/{criteria.TagId}{queryParams}");
        }

        return await HttpClient.GetFromJsonAsync<SearchResults<Note>>($"Note/Campaign/{campaignId}{queryParams}");
    }

    public async Task<SearchResults<Note>> ListNotesForSession(int sessionId, SearchCriteria criteria = null)
    {
        await SetAuthHeader();
        var queryParams = criteria == null ? string.Empty : BuildQueryParams(criteria.GetParamPairs());

        if (criteria is { TagId: not null })
        {
            return await HttpClient.GetFromJsonAsync<SearchResults<Note>>(
                $"Note/Session/{sessionId}/Tag/{criteria.TagId}{queryParams}");
        }

        return await HttpClient.GetFromJsonAsync<SearchResults<Note>>($"Note/Session/{sessionId}{queryParams}");
    }

    public async Task<SearchResults<Note>> ListNotesForNoun(int nounId, SearchCriteria criteria = null)
    {
        await SetAuthHeader();
        var queryParams = criteria == null ? string.Empty : BuildQueryParams(criteria.GetParamPairs());

        if (criteria is { TagId: not null })
        {
            return await HttpClient.GetFromJsonAsync<SearchResults<Note>>(
                $"Note/Noun/{nounId}/Tag/{criteria.TagId}{queryParams}");
        }

        return await HttpClient.GetFromJsonAsync<SearchResults<Note>>($"Note/Noun/{nounId}{queryParams}");
    }

    public async Task<Note> GetNote(int id)
    {
        await SetAuthHeader();
        return await HttpClient.GetFromJsonAsync<Note>($"Note/{id}");
    }

    public async Task<Note> SaveNote(Note note)
    {
        await SetAuthHeader();
        if (note.Id == 0)
        {
            var response = await HttpClient.PutAsJsonAsync("Note", note, JsonOpts);
            response.EnsureSuccessStatusCode();
            note = await response.Content.ReadFromJsonAsync<Note>();
        }
        else
        {
            var response = await HttpClient.PatchAsJsonAsync("Note", note, JsonOpts);
            response.EnsureSuccessStatusCode();
        }

        return note;
    }

    public async Task DeleteNote(int id)
    {
        await SetAuthHeader();
        await HttpClient.DeleteAsync($"Note/{id}");
    }

    public async Task UpdateTags(int id, List<Tag> tags)
    {
        await SetAuthHeader();
        var response = await HttpClient.PatchAsJsonAsync($"Note/{id}/Tags", tags);
        response.EnsureSuccessStatusCode();
    }

    public async Task UpdateNouns(int id, List<Noun> nouns)
    {
        await SetAuthHeader();
        var response = await HttpClient.PatchAsJsonAsync($"Note/{id}/Nouns", nouns);
        response.EnsureSuccessStatusCode();
    }
}
