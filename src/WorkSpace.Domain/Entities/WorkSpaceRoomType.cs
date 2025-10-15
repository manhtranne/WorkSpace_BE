// src/WorkSpace.Domain/Entities/WorkSpaceRoomType.cs
using System.ComponentModel.DataAnnotations;
using WorkSpace.Domain.Common;

namespace WorkSpace.Domain.Entities
{
    public class WorkSpaceRoomType : AuditableBaseEntity
    {
        [Required]
        [MaxLength(100)]
        public required string Name { get; set; } // Private Office, Meeting Room, Hot Desk, etc.

        [MaxLength(500)]
        public string? Description { get; set; }

        // Navigation properties
        public virtual List<WorkSpaceRoom> WorkSpaceRooms { get; set; } = new();
    }
}