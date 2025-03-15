using System.ComponentModel.DataAnnotations;

namespace WebApplication1.Models;

public class Team
{
    [Key]
    [Required]
    public int TeamId { get; set; }
    
    [Required]
    public string TeamName { get; set; } = string.Empty;
    
    public int? CaptainId { get; set; }
    
    // Navigation property for related bowlers
    public ICollection<Bowler>? Bowlers { get; set; }
}
