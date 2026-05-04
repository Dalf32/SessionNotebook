using DataLayer.Entities;
using SessionNotebook.Services;

namespace SessionNotebook.Data;

public class NotebookState
{
    private Campaign _activeCampaign;
    public Campaign ActiveCampaign
    {
        get => _activeCampaign;
        set
        {
            _activeCampaign = value;
            OnActiveCampaignChanged?.Invoke();
        }
    }

    public bool IsCampaignActive => ActiveCampaign != null;

    public event Action OnActiveCampaignChanged;

    public Session NewSession { get; private set; }

    public Note NewNote { get; private set; }

    public Tag NewTag { get; private set; }

    public Noun NewNoun { get; private set; }

    public async Task EnsureCampaignLoaded(CampaignService campaignService,
        int? campaignId = null, int? sessionId = null, int? noteId = null,
        int? nounId = null, int? tagId = null)
    {
        if (IsCampaignActive)
        {
            return;
        }

        if (sessionId.HasValue)
        {
            campaignId = await campaignService.GetCampaignIdForSession(sessionId.Value);
        }
        
        if (noteId.HasValue)
        {
            campaignId = await campaignService.GetCampaignIdForNote(noteId.Value);
        }
        
        if (nounId.HasValue)
        {
            campaignId = await campaignService.GetCampaignIdForNoun(nounId.Value);
        }
        
        if (tagId.HasValue)
        {
            campaignId = await campaignService.GetCampaignIdForTag(tagId.Value);
        }

        if (!campaignId.HasValue)
        {
            throw new ArgumentNullException(null,
                "One of the ID parameters must be provided.");
        }
        
        ActiveCampaign = await campaignService.GetCampaign(campaignId.Value);
    }
    
    public void ClearNewObjects()
    {
        NewSession = null;
        NewNote = null;
        NewTag = null;
        NewNoun = null;
    }

    public void InitializeNewSession(int campaignId, int sessionNum)
    {
        NewSession = new Session
        {
            CampaignId = campaignId,
            Number = sessionNum,
            StartDate = DateOnly.FromDateTime(DateTime.Now),
            Notes = [],
            Tags = []
        };
    }

    public void InitializeNewNote(int sessionId, int noteNum)
    {
        NewNote = new Note
        {
            SessionId = sessionId,
            Number = noteNum,
            Nouns = [],
            Tags = []
        };
    }

    public void InitializeNewTag(int campaignId)
    {
        NewTag = new Tag
        {
            CampaignId = campaignId
        };
    }

    public void InitializeNewNoun(int campaignId)
    {
        NewNoun = new Noun
        {
            CampaignId = campaignId,
            Tags = []
        };
    }
}
