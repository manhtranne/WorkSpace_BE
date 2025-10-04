using System.ComponentModel.DataAnnotations;
using WorkSpace.Domain.Common;

namespace WorkSpace.Domain.Entities;

public class Booking : AuditableBaseEntity
{
    [Required]
    [MaxLength(50)]
    public string BookingCode { get; set; } // Unique code for reference

    public int CustomerId { get; set; }
    public int WorkspaceId { get; set; }

    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }

    public int NumberOfParticipants { get; set; } = 1;

    [MaxLength(1000)]
    public string SpecialRequests { get; set; }

    public decimal TotalPrice { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal ServiceFee { get; set; }
    public decimal FinalAmount { get; set; }

    public string Currency { get; set; } = "VND";

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    public int BookingStatusId { get; set; }

    public DateTime? CheckedInAt { get; set; }
    public DateTime? CheckedOutAt { get; set; }

    [MaxLength(500)]
    public string CancellationReason { get; set; }
    public bool IsReviewed { get; set; } = false;

    // Navigation properties
    public virtual AppUser Customer { get; set; }
    public virtual WorkSpaces Workspace { get; set; }
    public virtual BookingStatus BookingStatus { get; set; }
    public virtual Payment Payment { get; set; }
    public virtual List<BookingParticipant> BookingParticipants { get; set; }
    public virtual List<Review> Reviews { get; set; }
    public virtual List<PromotionUsage> PromotionUsages { get; set; }
}