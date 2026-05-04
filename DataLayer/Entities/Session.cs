using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DataLayer.Entities;

[Table("Session")]
public class Session : BaseEntity
{
    public int CampaignId { get; set; }
    public Campaign Campaign { get; set; }

    [Required]
    public int Number { get; set; }

    [MaxLength(150, ErrorMessage = "Title cannot exceed 150 characters")]
    public string Title { get; set; }
    public DateOnly? StartDate { get; set; }

    public List<Tag> Tags { get; set; }
    public List<Note> Notes { get; set; }

    [NotMapped]
    public int NoteCount { get; set; }

    [NotMapped]
    public string DisplayName
    {
        get
        {
            var displayName = Number.ToString();
            return $"#{displayName}{(string.IsNullOrEmpty(Title) ? string.Empty : $" - {Title}")}";
        }
    }
}
