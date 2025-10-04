using System.ComponentModel.DataAnnotations;
using WorkSpace.Domain.Common;

namespace WorkSpace.Domain.Entities;

public class Payment : AuditableBaseEntity
{
    public int BookingId { get; set; }

    [Required]
    [Range(0.01, double.MaxValue)]
    public decimal Amount { get; set; }

    [Required]
    public DateTime PaymentDate { get; set; } = DateTime.Now;

    [Required]
    [StringLength(50)]
    public string PaymentMethod { get; set; } = "VNPay"; // Mặc định là VNPay

    [Required]
    [StringLength(20)]
    public string Status { get; set; } = "Pending"; // Pending, Completed, Failed

    [StringLength(255)]
    public string? TransactionId { get; set; } // Mã giao dịch từ VNPay

    [StringLength(2000)]
    public string? PaymentResponse { get; set; } // JSON response từ VNPay

    // Navigation properties
    public virtual Booking Booking { get; set; }
}