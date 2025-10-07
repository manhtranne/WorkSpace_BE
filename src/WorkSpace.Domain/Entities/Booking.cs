using System.ComponentModel.DataAnnotations;
using WorkSpace.Domain.Common;

namespace WorkSpace.Domain.Entities;

public class Booking : AuditableBaseEntity
{
    [Required]
    [MaxLength(50)]
    public required string BookingCode { get; set; } // Unique code for reference

    public int CustomerId { get; set; }
    public int WorkspaceId { get; set; }

    public DateTimeOffset StartTimeUtc { get; set; }
    public DateTimeOffset EndTimeUtc { get; set; }

    public int NumberOfParticipants { get; set; } = 1;

    [MaxLength(1000)]
    public string? SpecialRequests { get; set; }

    public decimal TotalPrice { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal ServiceFee { get; set; }
    public decimal FinalAmount { get; set; }

    [MaxLength(3)]
    public string? Currency { get; set; } = "VND";

    public int BookingStatusId { get; set; }

    public DateTimeOffset? CheckedInAt { get; set; }
    public DateTimeOffset? CheckedOutAt { get; set; }

    [MaxLength(500)]
    public string? CancellationReason { get; set; }
    public bool IsReviewed { get; set; } = false;

    // Navigation properties
    public virtual AppUser? Customer { get; set; }
    public virtual WorkSpace? Workspace { get; set; }
    public virtual BookingStatus? BookingStatus { get; set; }
    public virtual Payment? Payment { get; set; }
    public virtual List<BookingParticipant> BookingParticipants { get; set; } = new();
    public virtual List<Review> Reviews { get; set; } = new();
    public virtual List<PromotionUsage> PromotionUsages { get; set; } = new();
}