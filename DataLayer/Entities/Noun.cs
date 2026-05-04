using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DataLayer.Entities;

[Table("Noun")]
public class Noun : BaseEntity
{
    public int CampaignId { get; set; }
    public Campaign Campaign { get; set; }

    [Required]
    [MaxLength(50, ErrorMessage = "Name cannot exceed 50 characters")]
    [RegularExpression(@"([\w'""-,.]+\s?)+", ErrorMessage = "Name contains invalid characters")]
    public string Name { get; set; }

    [MaxLength(150, ErrorMessage = "Synopsis cannot exceed 150 characters")]
    public string Synopsis { get; set; }

    [MaxLength(1000, ErrorMessage = "Description cannot exceed 1,000 characters")]
    public string Description { get; set; }

    public List<Tag> Tags { get; set; }
    public List<Note> Notes { get; set; }

    [NotMapped]
    public int NoteCount { get; set; }
}
