using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DataLayer.Entities;

[Table("User")]
public class User : BaseEntity
{
    [Required]
    [MaxLength(255)]
    public string Email { get; set; }
}
