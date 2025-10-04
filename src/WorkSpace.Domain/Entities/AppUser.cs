using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;

namespace WorkSpace.Domain.Entities;
[Table("AppUsers")]
public class AppUser : IdentityUser<int>
{
    [Required]
    [MaxLength(100)]
    public required string FirstName { get; set; }
        
    [Required]
    [MaxLength(100)]
    public required string LastName { get; set; }
    public bool IsActive { get; set; }
    public string? RefreshToken { get; set; }
    public DateTime? RefreshTokenExpiryTime { get; set; }
    public DateTime DateCreated { get; set; }
    public DateTime? Dob { get; set; }
    
    [MaxLength(500)]
    public string? Avatar { get; set; }
    public DateTime? LastLoginDate { get; set; }
    
    
    // Navigation properties
    public virtual List<Booking> Bookings { get; set; }
    public virtual HostProfile HostProfile { get; set; }
    public virtual List<Review> Reviews { get; set; }
    public virtual List<WorkSpaceFavorite> WorkSpaceFavorites { get; set; }
    public virtual List<PromotionUsage> PromotionUsages { get; set; }
    public virtual List<Post> Posts { get; set; }

    public string GetFullName()
    {
        return this.FirstName + " " + this.LastName;
    }
}