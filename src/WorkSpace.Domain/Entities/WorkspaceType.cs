using System.ComponentModel.DataAnnotations;
using WorkSpace.Domain.Common;

namespace WorkSpace.Domain.Entities;

public class WorkspaceType : AuditableBaseEntity
{
    [Required]
    [MaxLength(100)]
    public  required string Name { get; set; } // Private Office, Meeting Room, Hot Desk, etc.

    [MaxLength(500)]
    public string? Description { get; set; }

    // Navigation properties
    public virtual List<WorkSpace> Workspaces { get; set; } = new();
}