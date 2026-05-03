using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
using Common;
using DataLayer;
using DataLayer.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ServiceApi.Data;

namespace ServiceApi.Controllers;

public class NounController: BaseController
{
    [HttpGet]
    [Route("Campaign/{campaignId:int}")]
    public ActionResult<SearchResults<Noun>> List(int campaignId, [FromQuery]SearchCriteria searchCriteria)
    {
        using var context = new NotebookContext();
        var nouns = context.Nouns.Where(n => n.CampaignId == campaignId &&
                                             n.Campaign.UserId == CurrentUser.Id);
        nouns = ApplySearchCriteria(nouns, searchCriteria);

        var nounsList = ApplyPagination(nouns, searchCriteria.PageSize, searchCriteria.PageNumber).AsNoTracking().ToList();
        MapCounts(context, nounsList);

        return new SearchResults<Noun>
        {
            PageSize = searchCriteria.PageSize,
            TotalCount = nouns.Count(),
            Results = nounsList
        };
    }

    [HttpGet]
    [Route("Campaign/{campaignId:int}/Tag/{tagId:int}")]
    public ActionResult<SearchResults<Noun>> ListByTag(int campaignId, int tagId, [FromQuery]SearchCriteria searchCriteria)
    {
        using var context = new NotebookContext();
        var nouns = context.Nouns.Where(n => n.CampaignId == campaignId &&
                                             n.Tags.Any(t => t.Id == tagId) && n.Campaign.UserId == CurrentUser.Id);
        nouns = ApplySearchCriteria(nouns, searchCriteria);

        var nounsList = ApplyPagination(nouns, searchCriteria.PageSize, searchCriteria.PageNumber).AsNoTracking().ToList();
        MapCounts(context, nounsList);

        return new SearchResults<Noun>
        {
            PageSize = searchCriteria.PageSize,
            TotalCount = nouns.Count(),
            Results = nounsList
        };
    }

    [HttpGet]
    [Route("{id:int}")]
    public ActionResult<Noun> Get(int id)
    {
        using var context = new NotebookContext();
        var noun = context.Nouns.Where(n => n.Id == id)
            .Include(n => n.Tags)
            .Include(n => n.Campaign)
            .Include(n => n.Notes)
            .AsNoTracking().SingleOrDefault();

        if (noun == null) return NotFound();
        if (noun.Campaign.UserId != CurrentUser.Id) return Unauthorized();

        noun.NoteCount = noun.Notes.Count;
        
        return noun;
    }

    [HttpPut]
    public ActionResult<Noun> Create([FromBody] Noun noun)
    {
        using var context = new NotebookContext();
        noun.Tags?.Clear();
        return Create(context, noun, "Noun/{0}");
    }

    [HttpPatch]
    public ActionResult Update([FromBody] Noun noun)
    {
        using var context = new NotebookContext();
        return Update(context, context.Nouns, noun);
    }

    [HttpDelete]
    [Route("{id:int}")]
    public ActionResult Delete(int id)
    {
        using var context = new NotebookContext();
        return Delete(context, context.Nouns, id);
    }

    [HttpPatch]
    [Route("{id:int}/Tags")]
    public ActionResult UpdateTags(int id, [FromBody] List<Tag> tags)
    {
        using var context = new NotebookContext();
        var noun = context.Nouns.Where(n => n.Id == id && n.Campaign.UserId == CurrentUser.Id)
            .Include(n => n.Tags).SingleOrDefault();

        if (noun == null) return NotFound();

        noun.Tags.RemoveAll(t => tags.All(ot => ot.Id != t.Id));
        noun.Tags.AddRange(tags.Where(t => noun.Tags.All(ot => ot.Id != t.Id)));
        context.SaveChanges();

        return Ok();
    }

    private static Expression<Func<Noun, object>> GetOrderBy(string sortBy)
    {
        return sortBy switch
        {
            "Name" => n => n.Name,
            "Notes" => n => n.Notes.Count,
            "UpdatedAt" => n => n.UpdatedAt,
            _ => n => n.Name
        };
    }

    [SuppressMessage("Performance", "CA1862:Use the \'StringComparison\' method overloads to perform case-insensitive string comparisons")]
    private static IQueryable<Noun> ApplySearchCriteria(IQueryable<Noun> nouns, SearchCriteria searchCriteria)
    {
        if (searchCriteria.HasQuery)
        {
            nouns = nouns.Where(n => n.Name.ToLower().Contains(searchCriteria.Query) ||
                                     n.Synopsis.ToLower().Contains(searchCriteria.Query) ||
                                     n.Description.ToLower().Contains(searchCriteria.Query));
        }

        return GetOrderDirection(nouns, searchCriteria.IsSortAscending).Invoke(GetOrderBy(searchCriteria.SortBy))
            .ThenByDescending(s => s.CreatedAt);
    }

    private static void MapCounts(NotebookContext context, List<Noun> nounsList)
    {
        var noteCounts = context.Nouns.Select(n => new
            {
                n.Id,
                NoteCount = n.Notes.Count
            })
            .AsNoTracking().ToDictionary(set => set.Id, set => set);

        foreach (var noun in nounsList)
        {
            if (!noteCounts.TryGetValue(noun.Id, out var count)) continue;
            
            noun.NoteCount = count.NoteCount;
        }
    }
}
