using System.ComponentModel.DataAnnotations;
using WorkSpace.Domain.Common;

namespace WorkSpace.Domain.Entities;

public class WorkSpaces : AuditableBaseEntity
{
    [Required]
    [MaxLength(255)]
    public string Title { get; set; }

    [MaxLength(1000)]
    public string Description { get; set; }

    public int HostId { get; set; }

    [Required]
    public int AddressId { get; set; }

    public int WorkspaceTypeId { get; set; }

    public decimal PricePerHour { get; set; }
    public decimal PricePerDay { get; set; }
    public decimal PricePerMonth { get; set; }

    public int Capacity { get; set; }
    public double Area { get; set; } // mét vuông

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    public bool IsActive { get; set; } = true;
    public bool IsVerified { get; set; } = false;

    // Navigation properties
    public virtual WorkspaceType WorkspaceType { get; set; }
    public virtual List<WorkspaceImage> WorkspaceImages { get; set; }
    public virtual List<WorkspaceAmenity> WorkspaceAmenities { get; set; }
    public virtual Address Address { get; set; }
    public virtual List<Booking> Bookings { get; set; }
    public virtual List<Review> Reviews { get; set; }

    public virtual HostProfile Host { get; set; }
    public virtual List<AvailabilitySchedule> AvailabilitySchedules { get; set; }
    public virtual List<BlockedTimeSlot> BlockedTimeSlots { get; set; }
    public virtual List<WorkSpaceFavorite> WorkSpaceFavorites { get; set; }
}