using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DataLayer.Entities;

[Table("Note")]
public class Note : BaseEntity
{
    public int SessionId { get; set; }
    public Session Session { get; set; }

    [Required]
    public int Number { get; set; }

    [MaxLength(150, ErrorMessage = "Title cannot exceed 150 characters")]
    public string Title { get; set; }

    [Required]
    [MaxLength(20000, ErrorMessage = "Text cannot exceed 20,000 characters")]
    public string Text { get; set; }

    public List<Tag> Tags { get; set; }
    public List<Noun> Nouns { get; set; }

    [NotMapped]
    public string DisplayName
    {
        get
        {
            var heading = Session == null ? Number.ToString() : $"{Session.Number}.{Number}";
            return $"#{heading}{(string.IsNullOrEmpty(Title) ? string.Empty : $" {Title}")}";
        }
    }
}
