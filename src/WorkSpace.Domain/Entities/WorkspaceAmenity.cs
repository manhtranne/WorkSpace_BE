using WorkSpace.Domain.Common;

namespace WorkSpace.Domain.Entities;

public class WorkspaceAmenity : AuditableBaseEntity
{
    public int WorkspaceId { get; set; }
    public int AmenityId { get; set; }

    public bool IsAvailable { get; set; } = true;

    // Navigation properties
    public virtual WorkSpace? Workspace { get; set; }
    public virtual Amenity? Amenity { get; set; }
}