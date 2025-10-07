using System.ComponentModel.DataAnnotations;
using WorkSpace.Domain.Common;

namespace WorkSpace.Domain.Entities;

public class WorkSpace : AuditableBaseEntity
{
    [Required]
    [MaxLength(255)]
    public required string Title { get; set; }

    [MaxLength(1000)]
    public string? Description { get; set; }

    public int HostId { get; set; }

    [Required]
    public int AddressId { get; set; }

    public int WorkspaceTypeId { get; set; }

    public decimal PricePerHour { get; set; }
    public decimal PricePerDay { get; set; }
    public decimal PricePerMonth { get; set; }

    public int Capacity { get; set; }
    public double Area { get; set; } // mét vuông
    
    public bool IsActive { get; set; } = true;
    public bool IsVerified { get; set; } = false;

    // Navigation properties
    public virtual WorkspaceType? WorkspaceType { get; set; }
    public virtual List<WorkspaceImage> WorkspaceImages { get; set; } = new();
    public virtual List<WorkspaceAmenity> WorkspaceAmenities { get; set; } = new();
    public virtual Address? Address { get; set; }
    public virtual List<Booking> Bookings { get; set; } = new();
    public virtual List<Review> Reviews { get; set; } = new();

    public virtual HostProfile? Host { get; set; }
    public virtual List<AvailabilitySchedule> AvailabilitySchedules { get; set; } = new();
    public virtual List<BlockedTimeSlot> BlockedTimeSlots { get; set; } = new();
    public virtual List<WorkSpaceFavorite> WorkSpaceFavorites { get; set; } = new();
}