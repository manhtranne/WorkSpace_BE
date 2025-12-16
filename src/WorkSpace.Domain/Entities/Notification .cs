using System.ComponentModel.DataAnnotations;
using WorkSpace.Domain.Common;

namespace WorkSpace.Domain.Entities
{
    public class Notification : AuditableBaseEntity
    {
        [Required]
        [MaxLength(200)]
        public string Title { get; set; }

        [Required]
        public string Content { get; set; }

        public int SenderId { get; set; }

        [Required]
        [MaxLength(50)]
        public string SenderRole { get; set; }
    }
}