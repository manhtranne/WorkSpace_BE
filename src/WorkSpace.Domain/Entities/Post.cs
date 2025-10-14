using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using WorkSpace.Domain.Common;

namespace WorkSpace.Domain.Entities;

public class Post : AuditableBaseEntity
{
    [MaxLength(200)]
    public string? Title { get; set; }
    [MaxLength(int.MaxValue)] 
    public string? ContentMarkdown { get; set; }

    [MaxLength(int.MaxValue)] 
    public string? ContentHtml { get; set; }
    [MaxLength(200)]
    public string? ImageData { get; set; }

    public int UserId { get; set; }
    // Navigation property
    public virtual AppUser? User { get; set; }
}