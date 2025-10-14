using System.ComponentModel.DataAnnotations;
using WorkSpace.Domain.Common;

namespace WorkSpace.Domain.Entities;

public class AvailabilitySchedule  : AuditableBaseEntity
{
    public int WorkSpaceRoomId { get; set; }

    public DayOfWeek DayOfWeek { get; set; }

    [Required]
    public TimeSpan StartTime { get; set; }

    [Required]
    public TimeSpan EndTime { get; set; }

    public bool IsAvailable { get; set; } = true;

    public virtual WorkSpaceRoom? WorkSpaceRoom { get; set; }
}