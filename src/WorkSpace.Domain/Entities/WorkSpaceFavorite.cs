using WorkSpace.Domain.Common;

namespace WorkSpace.Domain.Entities;

public class WorkSpaceFavorite : AuditableBaseEntity
{
    public int UserId { get; set; }
    public int WorkspaceId { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    // Navigation properties
    public virtual AppUser User { get; set; }
    public virtual WorkSpaces Workspace { get; set; } 
}