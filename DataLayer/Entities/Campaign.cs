using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DataLayer.Entities;

[Table("Campaign")]
public class Campaign : BaseEntity
{
    public int UserId { get; set; }
    public User User { get; set; }

    [Required]
    [MaxLength(50, ErrorMessage = "Name cannot exceed 50 characters")]
    public string Name { get; set; }

    [MaxLength(1000, ErrorMessage = "Description cannot exceed 1,000 characters")]
    public string Description { get; set; }
    public DateOnly? StartDate { get; set; }
    public DateOnly? EndDate { get; set; }

    public List<Session> Sessions { get; set; }

    [NotMapped]
    public int SessionCount { get; set; }

    [NotMapped]
    public Session LastSession { get; set; }
    
    [NotMapped]
    public bool HasLastSession => LastSession != null;

    public string FormatDateRange()
    {
        if (!StartDate.HasValue && !EndDate.HasValue) return string.Empty;

        var dateRangeStr = StartDate.HasValue ? StartDate.Value.ToString() : "...";

        if (EndDate.HasValue)
        {
            dateRangeStr += $" - {EndDate.ToString()}";
        }

        return dateRangeStr;
    }
}
