using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
using Common;
using DataLayer;
using DataLayer.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ServiceApi.Data;

namespace ServiceApi.Controllers;

public class NoteController: BaseController
{
    [HttpGet]
    [Route("Campaign/{campaignId:int}")]
    public ActionResult<SearchResults<Note>> ListByCampaign(int campaignId, [FromQuery]SearchCriteria searchCriteria)
    {
        using var context = new NotebookContext();
        var notes = context.Notes.Where(n => n.Session.CampaignId == campaignId &&
                                             n.Session.Campaign.UserId == CurrentUser.Id);
        notes = ApplySearchCriteria(notes, searchCriteria, true).Include(n => n.Tags)
            .Include(n => n.Session);

        var notesList = ApplyPagination(notes, searchCriteria.PageSize, searchCriteria.PageNumber).AsNoTracking().ToList();
        
        return new SearchResults<Note>
        {
            PageSize = searchCriteria.PageSize,
            TotalCount = notes.Count(),
            Results = notesList
        };
    }

    [HttpGet]
    [Route("Campaign/{campaignId:int}/Tag/{tagId:int}")]
    public ActionResult<SearchResults<Note>> ListByCampaignAndTag(int campaignId, int tagId,
        [FromQuery]SearchCriteria searchCriteria)
    {
        using var context = new NotebookContext();
        var notes = context.Notes.Where(n => n.Session.CampaignId == campaignId &&
                                             n.Tags.Any(t => t.Id == tagId) &&
                                             n.Session.Campaign.UserId == CurrentUser.Id);
        notes = ApplySearchCriteria(notes, searchCriteria, true).Include(n => n.Tags)
            .Include(n => n.Session);

        var notesList = ApplyPagination(notes, searchCriteria.PageSize, searchCriteria.PageNumber).AsNoTracking().ToList();
        
        return new SearchResults<Note>
        {
            PageSize = searchCriteria.PageSize,
            TotalCount = notes.Count(),
            Results = notesList
        };
    }

    [HttpGet]
    [Route("Session/{sessionId:int}")]
    public ActionResult<SearchResults<Note>> ListBySession(int sessionId, [FromQuery]SearchCriteria searchCriteria)
    {
        using var context = new NotebookContext();
        var notes = context.Notes.Where(n => n.SessionId == sessionId &&
                                             n.Session.Campaign.UserId == CurrentUser.Id);
        notes = ApplySearchCriteria(notes, searchCriteria, false)
            .Include(n => n.Tags);

        var notesList = ApplyPagination(notes, searchCriteria.PageSize, searchCriteria.PageNumber).AsNoTracking().ToList();
        
        return new SearchResults<Note>
        {
            PageSize = searchCriteria.PageSize,
            TotalCount = notes.Count(),
            Results = notesList
        };
    }

    [HttpGet]
    [Route("Session/{sessionId:int}/Tag/{tagId:int}")]
    public ActionResult<SearchResults<Note>> ListBySessionAndTag(int sessionId, int tagId,
        [FromQuery]SearchCriteria searchCriteria)
    {
        using var context = new NotebookContext();
        var notes = context.Notes.Where(n => n.SessionId == sessionId &&
                                             n.Tags.Any(t => t.Id == tagId) &&
                                             n.Session.Campaign.UserId == CurrentUser.Id);
        notes = ApplySearchCriteria(notes, searchCriteria, false)
            .Include(n => n.Tags);

        var notesList = ApplyPagination(notes, searchCriteria.PageSize, searchCriteria.PageNumber).AsNoTracking().ToList();
        
        return new SearchResults<Note>
        {
            PageSize = searchCriteria.PageSize,
            TotalCount = notes.Count(),
            Results = notesList
        };
    }

    [HttpGet]
    [Route("Noun/{nounId:int}")]
    public ActionResult<SearchResults<Note>> ListByNoun(int nounId, [FromQuery]SearchCriteria searchCriteria)
    {
        using var context = new NotebookContext();
        var notes = context.Notes.Where(n => n.Nouns.Any(no => no.Id == nounId) &&
                                             n.Session.Campaign.UserId == CurrentUser.Id);
        notes = ApplySearchCriteria(notes, searchCriteria, true).Include(n => n.Tags)
            .Include(n => n.Session);

        var notesList = ApplyPagination(notes, searchCriteria.PageSize, searchCriteria.PageNumber).AsNoTracking().ToList();
        
        return new SearchResults<Note>
        {
            PageSize = searchCriteria.PageSize,
            TotalCount = notes.Count(),
            Results = notesList
        };
    }

    [HttpGet]
    [Route("Noun/{nounId:int}/Tag/{tagId:int}")]
    public ActionResult<SearchResults<Note>> ListByNounAndTag(int nounId, int tagId,
        [FromQuery]SearchCriteria searchCriteria)
    {
        using var context = new NotebookContext();
        var notes = context.Notes.Where(n => n.Nouns.Any(no => no.Id == nounId) &&
                                             n.Tags.Any(t => t.Id == tagId) &&
                                             n.Session.Campaign.UserId == CurrentUser.Id);
        notes = ApplySearchCriteria(notes, searchCriteria, true).Include(n => n.Tags)
            .Include(n => n.Session);

        var notesList = ApplyPagination(notes, searchCriteria.PageSize, searchCriteria.PageNumber).AsNoTracking().ToList();
        
        return new SearchResults<Note>
        {
            PageSize = searchCriteria.PageSize,
            TotalCount = notes.Count(),
            Results = notesList
        };
    }

    [HttpGet]
    [Route("{id:int}")]
    public ActionResult<Note> Get(int id)
    {
        using var context = new NotebookContext();
        var note = context.Notes.Where(n => n.Id == id)
            .Include(n => n.Tags)
            .Include(n => n.Nouns)
            .Include(n => n.Session).ThenInclude(s => s.Campaign)
            .AsNoTracking().SingleOrDefault();

        if (note == null) return NotFound();
        if (note.Session.Campaign.UserId != CurrentUser.Id) return Unauthorized();

        return note;
    }

    [HttpPut]
    public ActionResult<Note> Create([FromBody] Note note)
    {
        using var context = new NotebookContext();
        note.Nouns.Clear();
        note.Tags.Clear();
        return Create(context, note, "Note/{0}");
    }

    [HttpPatch]
    public ActionResult Update([FromBody] Note note)
    {
        using var context = new NotebookContext();
        return Update(context, context.Notes, note);
    }

    [HttpDelete]
    [Route("{id:int}")]
    public ActionResult Delete(int id)
    {
        using var context = new NotebookContext();
        return Delete(context, context.Notes, id);
    }

    [HttpPatch]
    [Route("{id:int}/Tags")]
    public ActionResult UpdateTags(int id, [FromBody] List<Tag> tags)
    {
        using var context = new NotebookContext();
        var note = context.Notes.Where(n => n.Id == id && n.Session.Campaign.UserId == CurrentUser.Id)
            .Include(n => n.Tags).SingleOrDefault();

        if (note == null) return NotFound();

        note.Tags.RemoveAll(t => tags.All(ot => ot.Id != t.Id));
        note.Tags.AddRange(tags.Where(t => note.Tags.All(ot => ot.Id != t.Id)));
        context.SaveChanges();

        return Ok();
    }

    [HttpPatch]
    [Route("{id:int}/Nouns")]
    public ActionResult UpdateNouns(int id, [FromBody] List<Noun> nouns)
    {
        using var context = new NotebookContext();
        var note = context.Notes.Where(n => n.Id == id && n.Session.Campaign.UserId == CurrentUser.Id)
            .Include(n => n.Nouns).SingleOrDefault();

        if (note == null) return NotFound();

        note.Nouns.RemoveAll(n => nouns.All(on => on.Id != n.Id));
        note.Nouns.AddRange(nouns.Where(n => note.Nouns.All(on => on.Id != n.Id)));
        context.SaveChanges();

        return Ok();
    }

    private static Expression<Func<Note, object>> GetOrderBy(string sortBy)
    {
        return sortBy switch
        {
            "Number" => n => n.Number,
            "Title" => n => n.Title,
            "UpdatedAt" => n => n.UpdatedAt,
            _ => n => n.Number
        };
    }

    [SuppressMessage("Performance", "CA1862:Use the \'StringComparison\' method overloads to perform case-insensitive string comparisons")]
    private static IQueryable<Note> ApplySearchCriteria(IQueryable<Note> notes, SearchCriteria searchCriteria,
        bool includeSessionNum)
    {
        if (searchCriteria.HasQuery)
        {
            notes = notes.Where(n => n.Title.ToLower().Contains(searchCriteria.Query) ||
                                     n.Text.ToLower().Contains(searchCriteria.Query));
        }

        if (includeSessionNum && (string.IsNullOrEmpty(searchCriteria.SortBy) || searchCriteria.SortBy == "Number"))
        {
            return GetSecondaryOrderDirection(
                    GetOrderDirection(notes, searchCriteria.IsSortAscending).Invoke(n => n.Session.Number),
                    searchCriteria.IsSortAscending)
                .Invoke(GetOrderBy(searchCriteria.SortBy))
                .ThenByDescending(n => n.CreatedAt);
        }
        
        return GetOrderDirection(notes, searchCriteria.IsSortAscending)
            .Invoke(GetOrderBy(searchCriteria.SortBy)).ThenByDescending(n => n.CreatedAt);
    }
}
