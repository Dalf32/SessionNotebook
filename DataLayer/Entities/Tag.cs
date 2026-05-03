using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DataLayer.Entities;

[Table("Tag")]
public class Tag : BaseEntity
{
    public int CampaignId { get; set; }
    public Campaign Campaign { get; set; }

    [Required]
    [MaxLength(50, ErrorMessage = "Name cannot exceed 50 characters")]
    [RegularExpression(@"\w+", ErrorMessage = "Name contains invalid characters")]
    public string Name { get; set; }

    [MaxLength(1000, ErrorMessage = "Description cannot exceed 1000 characters")]
    public string Description { get; set; }
    public bool IsSessionTag { get; set; }
    public bool IsNoteTag { get; set; }
    public bool IsNounTag { get; set; }

    public List<Session> Sessions { get; set; }
    public List<Note> Notes { get; set; }
    public List<Noun> Nouns { get; set; }

    [NotMapped]
    public int SessionCount { get; set; }

    [NotMapped]
    public int NoteCount { get; set; }

    [NotMapped]
    public int NounCount { get; set; }

    [NotMapped]
    public int UsageCount => SessionCount + NoteCount + NounCount;

    public void MergeFlags(Tag otherTag)
    {
        IsSessionTag |= otherTag.IsSessionTag;
        IsNoteTag |= otherTag.IsNoteTag;
        IsNounTag |= otherTag.IsNounTag;
    }

    public void ClearAssociations()
    {
        Sessions = null;
        Notes = null;
        Nouns = null;
    }
}
