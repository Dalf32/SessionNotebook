using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
using Common;
using DataLayer;
using DataLayer.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ServiceApi.Data;

namespace ServiceApi.Controllers;

public class CampaignController: BaseController
{
    [HttpGet]
    public ActionResult<SearchResults<Campaign>> List([FromQuery]SearchCriteria searchCriteria)
    {
        using var context = new NotebookContext();
        var campaigns = context.Campaigns.Where(c => c.UserId == CurrentUser.Id);
        campaigns = ApplySearchCriteria(campaigns, searchCriteria);
        var campaignsList = ApplyPagination(campaigns, searchCriteria.PageSize, searchCriteria.PageNumber).AsNoTracking().ToList();
        
        var sessionData = context.Sessions.GroupBy(s => s.CampaignId).AsNoTracking()
            .ToDictionary(c => c.Key, c => (
                    Count: c.Count(), LastSession: c.OrderBy(s => s.Number).Last()
                )
            );

        foreach (var campaign in campaignsList)
        {
            var hasSessionInfo = sessionData.TryGetValue(campaign.Id, out var sessionInfo);
            campaign.SessionCount = hasSessionInfo ? sessionInfo.Count : 0;
            campaign.LastSession = hasSessionInfo ? sessionInfo.LastSession : null;
        }

        return new SearchResults<Campaign>
        {
            PageSize = searchCriteria.PageSize,
            TotalCount = campaigns.Count(),
            Results = campaignsList
        };
    }

    [HttpGet]
    [Route("{id:int}")]
    public ActionResult<Campaign> Get(int id)
    {
        using var context = new NotebookContext();
        var campaign = context.Campaigns.Where(c => c.Id == id)
            .Include(c => c.Sessions)
            .AsNoTracking().SingleOrDefault();

        if (campaign == null) return NotFound();
        if (campaign.UserId != CurrentUser.Id) return Unauthorized();

        campaign.SessionCount = campaign.Sessions.Count;
        
        return campaign;
    }

    [HttpPut]
    public ActionResult<Campaign> Create([FromBody] Campaign campaign)
    {
        using var context = new NotebookContext();

        campaign.UserId = CurrentUser.Id;
        return Create(context, campaign, "Campaign/{0}");
    }

    [HttpPatch]
    public ActionResult Update([FromBody] Campaign campaign)
    {
        using var context = new NotebookContext();
        return Update(context, context.Campaigns, campaign);
    }

    [HttpDelete]
    [Route("{id:int}")]
    public ActionResult Delete(int id)
    {
        using var context = new NotebookContext();
        return Delete(context, context.Campaigns, id);
    }

    #region GetId
    
    [HttpGet]
    [Route("ForSession/{sessionId:int}")]
    public ActionResult<int> GetIdForSession(int sessionId)
    {
        using var context = new NotebookContext();
        var campaignId = context.Sessions.SingleOrDefault(s => s.Id == sessionId && s.Campaign.UserId == CurrentUser.Id)
            ?.CampaignId;

        if (!campaignId.HasValue) return 0;
        return campaignId;
    }
    
    [HttpGet]
    [Route("ForNote/{noteId:int}")]
    public ActionResult<int> GetIdForNote(int noteId)
    {
        using var context = new NotebookContext();
        var campaignId = context.Notes.Include(n => n.Session)
            .SingleOrDefault(n => n.Id == noteId && n.Session.Campaign.UserId == CurrentUser.Id)
            ?.Session.CampaignId;

        if (!campaignId.HasValue) return 0;
        return campaignId;
    }
    
    [HttpGet]
    [Route("ForNoun/{nounId:int}")]
    public ActionResult<int> GetIdForNoun(int nounId)
    {
        using var context = new NotebookContext();
        var campaignId = context.Nouns.SingleOrDefault(n => n.Id == nounId && n.Campaign.UserId == CurrentUser.Id)
            ?.CampaignId;

        if (!campaignId.HasValue) return 0;
        return campaignId;
    }
    
    [HttpGet]
    [Route("ForTag/{tagId:int}")]
    public ActionResult<int> GetIdForTag(int tagId)
    {
        using var context = new NotebookContext();
        var campaignId = context.Tags.SingleOrDefault(t => t.Id == tagId && t.Campaign.UserId == CurrentUser.Id)
            ?.CampaignId;

        if (!campaignId.HasValue) return 0;
        return campaignId;
    }
    
    #endregion
    
    private static Expression<Func<Campaign, object>> GetOrderBy(string sortBy)
    {
        return sortBy switch
        {
            "Name" => c => c.Name,
            "StartDate" => c => c.StartDate,
            "EndDate" => c => c.EndDate,
            "SessionDate" => c => c.Sessions.OrderBy(s => s.Number).Last().StartDate,
            "Sessions" => c => c.Sessions.Count,
            "UpdatedAt" => c => c.UpdatedAt,
            _ => c => c.Sessions.OrderBy(s => s.Number).Last().StartDate
        };
    }

    [SuppressMessage("Performance", "CA1862:Use the \'StringComparison\' method overloads to perform case-insensitive string comparisons")]
    private static IQueryable<Campaign> ApplySearchCriteria(IQueryable<Campaign> campaigns, SearchCriteria searchCriteria)
    {
        if (searchCriteria.HasQuery)
        {
            campaigns = campaigns.Where(c => c.Name.ToLower().Contains(searchCriteria.Query));
        }

        return GetOrderDirection(campaigns, searchCriteria.IsSortAscending).Invoke(GetOrderBy(searchCriteria.SortBy))
            .ThenByDescending(s => s.CreatedAt);
    }
}
