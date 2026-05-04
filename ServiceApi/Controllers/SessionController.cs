using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
using Common;
using DataLayer;
using DataLayer.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ServiceApi.Data;

namespace ServiceApi.Controllers;

public class SessionController: BaseController
{
    [HttpGet]
    [Route("Campaign/{campaignId:int}")]
    public ActionResult<SearchResults<Session>> ListByCampaign(int campaignId, [FromQuery]SearchCriteria searchCriteria)
    {
        using var context = new NotebookContext();
        var sessions = context.Sessions.Where(s => s.CampaignId == campaignId &&
                                                   s.Campaign.UserId == CurrentUser.Id);
        sessions = ApplySearchCriteria(sessions, searchCriteria);

        var sessionsList = ApplyPagination(sessions, searchCriteria.PageSize, searchCriteria.PageNumber).AsNoTracking().ToList();
        MapNoteCounts(context, sessionsList);

        return new SearchResults<Session>
        {
            PageSize = searchCriteria.PageSize,
            TotalCount = sessions.Count(),
            Results = sessionsList
        };
    }

    [HttpGet]
    [Route("Campaign/{campaignId:int}/Tag/{tagId:int}")]
    public ActionResult<SearchResults<Session>> ListByCampaignAndTag(int campaignId, int tagId,
        [FromQuery]SearchCriteria searchCriteria)
    {
        using var context = new NotebookContext();
        var sessions = context.Sessions.Where(s => s.CampaignId == campaignId &&
                                                   s.Campaign.UserId == CurrentUser.Id &&
                                                   s.Tags.Any(t => t.Id == tagId));
        sessions = ApplySearchCriteria(sessions, searchCriteria);
        
        var sessionsList = ApplyPagination(sessions, searchCriteria.PageSize, searchCriteria.PageNumber).AsNoTracking().ToList();
        MapNoteCounts(context, sessionsList);

        return new SearchResults<Session>
        {
            PageSize = searchCriteria.PageSize,
            TotalCount = sessions.Count(),
            Results = sessionsList
        };
    }

    [HttpGet]
    [Route("{id:int}")]
    public ActionResult<Session> Get(int id)
    {
        using var context = new NotebookContext();
        var session = context.Sessions.Where(s => s.Id == id)
            .Include(s => s.Tags)
            .Include(s => s.Notes)
            .Include(s => s.Campaign)
            .AsNoTracking().SingleOrDefault();

        if (session == null) return NotFound();
        if (session.Campaign.UserId != CurrentUser.Id) return Unauthorized();

        session.NoteCount = session.Notes.Count;
        
        return session;
    }

    [HttpPut]
    public ActionResult<Session> Create([FromBody] Session session)
    {
        using var context = new NotebookContext();
        session.Tags.Clear();
        return Create(context, session, "Session/{0}");
    }

    [HttpPatch]
    public ActionResult Update([FromBody] Session session)
    {
        using var context = new NotebookContext();
        return Update(context, context.Sessions, session);
    }

    [HttpDelete]
    [Route("{id:int}")]
    public ActionResult Delete(int id)
    {
        using var context = new NotebookContext();
        return Delete(context, context.Sessions, id);
    }

    [HttpPatch]
    [Route("{id:int}/Tags")]
    public ActionResult UpdateTags(int id, [FromBody] List<Tag> tags)
    {
        using var context = new NotebookContext();
        var session = context.Sessions.Where(s => s.Id == id && s.Campaign.UserId == CurrentUser.Id)
            .Include(s => s.Tags).SingleOrDefault();

        if (session == null) return NotFound();

        session.Tags.RemoveAll(t => tags.All(ot => ot.Id != t.Id));
        session.Tags.AddRange(tags.Where(t => session.Tags.All(ot => ot.Id != t.Id)));
        context.SaveChanges();

        return Ok();
    }

    private static Expression<Func<Session, object>> GetOrderBy(string sortBy)
    {
        return sortBy switch
        {
            "Number" => s => s.Number,
            "Title" => s => s.Title,
            "Date" => s => s.StartDate,
            "Notes" => s => s.Notes.Count,
            "UpdatedAt" => s => s.UpdatedAt,
            _ => s => s.Number
        };
    }

    [SuppressMessage("Performance", "CA1862:Use the \'StringComparison\' method overloads to perform case-insensitive string comparisons")]
    private static IQueryable<Session> ApplySearchCriteria(IQueryable<Session> sessions, SearchCriteria searchCriteria)
    {
        if (searchCriteria.HasQuery)
        {
            sessions = sessions.Where(s => s.Title.ToLower().Contains(searchCriteria.Query));
        }

        return GetOrderDirection(sessions, searchCriteria.IsSortAscending).Invoke(GetOrderBy(searchCriteria.SortBy))
            .ThenByDescending(s => s.CreatedAt);
    }

    private static void MapNoteCounts(NotebookContext context, List<Session> sessionsList)
    {
        var noteCounts = context.Notes.GroupBy(n => n.SessionId).AsNoTracking()
            .ToDictionary(s => s.Key, s => s.Count());

        foreach (var session in sessionsList)
        {
            session.NoteCount = noteCounts.TryGetValue(session.Id, out var count) ? count : 0;
        }
    }
}
