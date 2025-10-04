﻿using System.ComponentModel.DataAnnotations;
using WorkSpace.Domain.Common;

namespace WorkSpace.Domain.Entities;

public class Review : AuditableBaseEntity
{
    public int BookingId { get; set; }
    public int UserId { get; set; }
    public int WorkspaceId { get; set; }

    [Range(1, 5)]
    public int Rating { get; set; }

    [MaxLength(1000)]
    public string Comment { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    public bool IsVerified { get; set; } = false; // Verified as actual customer
    public bool IsPublic { get; set; } = true;

    // Navigation properties
    public virtual Booking Booking { get; set; }
    public virtual AppUser User { get; set; }
    public virtual WorkSpaces Workspace { get; set; }
}