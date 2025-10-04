﻿using System.ComponentModel.DataAnnotations;
using WorkSpace.Domain.Common;

namespace WorkSpace.Domain.Entities;

public class Amenity : AuditableBaseEntity
{
    [Required]
    [MaxLength(100)]
    public string Name { get; set; } // WiFi, Projector, Coffee, etc.

    [MaxLength(255)]
    public string Description { get; set; }

    [MaxLength(50)]
    public string IconClass { get; set; } // For UI

    // Navigation properties
    public virtual List<WorkspaceAmenity> WorkspaceAmenities { get; set; }
}