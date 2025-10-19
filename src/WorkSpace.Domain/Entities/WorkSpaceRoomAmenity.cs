using WorkSpace.Domain.Common;
using WorkSpace.Domain.Entities;

public class WorkSpaceRoomAmenity : AuditableBaseEntity
{
    public int WorkspaceId { get; set; }
    public int AmenityId { get; set; }
    public int WorkSpaceRoomId { get; set; } 
    public bool IsAvailable { get; set; }
    public virtual WorkSpaceRoom? WorkSpaceRoom { get; set; } 
    public virtual Amenity? Amenity { get; set; }
}