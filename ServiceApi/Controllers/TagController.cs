using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
using Common;
using DataLayer;
using DataLayer.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ServiceApi.Data;

namespace ServiceApi.Controllers;

public class TagController: BaseController
{
    [HttpGet]
    [Route("Campaign/{campaignId:int}")]
    public ActionResult<SearchResults<Tag>> ListByCampaign(int campaignId, [FromQuery]string associatedType,
        [FromQuery]SearchCriteria searchCriteria)
    {
        if (!string.IsNullOrEmpty(associatedType)) return ListByCampaignAndType(campaignId, associatedType);

        using var context = new NotebookContext();
        var tags = context.Tags.Where(t => t.CampaignId == campaignId && t.Campaign.UserId == CurrentUser.Id);
        tags = ApplySearchCriteria(tags, searchCriteria);

        var tagsList = ApplyPagination(tags, searchCriteria.PageSize, searchCriteria.PageNumber).AsNoTracking().ToList();
        MapCounts(context, tagsList);

        return new SearchResults<Tag>
        {
            PageSize = searchCriteria.PageSize,
            TotalCount = tags.Count(),
            Results = tagsList
        };
    }

    [HttpGet]
    [Route("{id:int}")]
    public ActionResult<Tag> Get(int id)
    {
        using var context = new NotebookContext();
        var tag = context.Tags.Where(t => t.Id == id)
            .Include(t => t.Sessions)
            .Include(t => t.Notes)
            .Include(t => t.Nouns)
            .Include(t => t.Campaign)
            .AsNoTracking().SingleOrDefault();

        if (tag == null) return NotFound();
        if (tag.Campaign.UserId != CurrentUser.Id) return Unauthorized();

        tag.SessionCount = tag.Sessions.Count;
        tag.NoteCount = tag.Notes.Count;
        tag.NounCount = tag.Nouns.Count;
        
        return tag;
    }

    [HttpPut]
    public ActionResult<Tag> Create([FromBody] Tag tag)
    {
        using var context = new NotebookContext();
        return Create(context, tag, "Tag/{0}");
    }

    [HttpPatch]
    public ActionResult Update([FromBody] Tag tag)
    {
        using var context = new NotebookContext();

        tag.ClearAssociations();
        return Update(context, context.Tags, tag);
    }

    [HttpDelete]
    [Route("{id:int}")]
    public ActionResult Delete(int id)
    {
        using var context = new NotebookContext();
        return Delete(context, context.Tags, id);
    }

    private ActionResult<SearchResults<Tag>> ListByCampaignAndType(int campaignId, string associatedType)
    {
        using var context = new NotebookContext();
        List<Tag> tags = null;

        if (associatedType.EqualsIgnoreCase("Session"))
        {
            tags = context.Tags.Where(t => t.CampaignId == campaignId && t.Campaign.UserId == CurrentUser.Id &&
                                           t.IsSessionTag)
                .OrderBy(t => t.Name).ToList();
        }

        if (associatedType.EqualsIgnoreCase("Note"))
        {
            tags = context.Tags.Where(t => t.CampaignId == campaignId && t.Campaign.UserId == CurrentUser.Id &&
                                           t.IsNoteTag)
                .OrderBy(t => t.Name).ToList();
        }

        if (associatedType.EqualsIgnoreCase("Noun"))
        {
            tags = context.Tags.Where(t => t.CampaignId == campaignId && t.Campaign.UserId == CurrentUser.Id &&
                                           t.IsNounTag)
                .OrderBy(t => t.Name).ToList();
        }

        if (tags != null)
        {
            return new SearchResults<Tag>
            {
                TotalCount = tags.Count,
                Results = tags
            };
        }

        return ValidationProblem(detail: "AssociatedType must be one of Session, Note, or Noun.");
    }

    private static Expression<Func<Tag, object>> GetOrderBy(string sortBy)
    {
        return sortBy switch
        {
            "Name" => t => t.Name,
            "Usages" => n => n.Sessions.Count + n.Notes.Count + n.Nouns.Count,
            "UpdatedAt" => n => n.UpdatedAt,
            _ => n => n.Name
        };
    }

    [SuppressMessage("Performance", "CA1862:Use the \'StringComparison\' method overloads to perform case-insensitive string comparisons")]
    private static IQueryable<Tag> ApplySearchCriteria(IQueryable<Tag> tags, SearchCriteria searchCriteria)
    {
        if (searchCriteria.HasQuery)
        {
            tags = tags.Where(t => t.Name.ToLower().Contains(searchCriteria.Query) ||
                                     t.Description.ToLower().Contains(searchCriteria.Query));
        }

        return GetOrderDirection(tags, searchCriteria.IsSortAscending)
            .Invoke(GetOrderBy(searchCriteria.SortBy)).ThenByDescending(n => n.CreatedAt);
    }
    
    private static void MapCounts(NotebookContext context, List<Tag> tagsList)
    {
        var allCounts = context.Tags.Select(t => new
        {
            t.Id,
            SessionCount = t.Sessions.Count,
            NoteCount = t.Notes.Count,
            NounCount = t.Nouns.Count
        }).AsNoTracking().ToDictionary(set => set.Id, set => set);

        foreach (var tag in tagsList)
        {
            if (!allCounts.TryGetValue(tag.Id, out var counts)) continue;

            tag.SessionCount = counts.SessionCount;
            tag.NoteCount = counts.NoteCount;
            tag.NounCount = counts.NounCount;
        }
    }
}
