using System.ComponentModel.DataAnnotations;
using WorkSpace.Domain.Common;

namespace WorkSpace.Domain.Entities;

public class Promotion : AuditableBaseEntity
{

    [Required]
    [MaxLength(100)]
    public string Code { get; set; }

    [MaxLength(255)]
    public string Description { get; set; }

    public decimal DiscountValue { get; set; }
    public string DiscountType { get; set; } // Percentage, FixedAmount

    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }

    public int UsageLimit { get; set; } = 0; // 0 = unlimited
    public int UsedCount { get; set; } = 0;

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public virtual List<PromotionUsage> PromotionUsages { get; set; }
}