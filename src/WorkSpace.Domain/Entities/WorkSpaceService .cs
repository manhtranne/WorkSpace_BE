using System.ComponentModel.DataAnnotations;
using WorkSpace.Domain.Common;

namespace WorkSpace.Domain.Entities
{
    public class WorkSpaceService : BaseEntity
    {
        [Required]
        [MaxLength(255)]
        public required string Name { get; set; }

        [MaxLength(1000)]
        public string? Description { get; set; }

        public decimal Price { get; set; }

        [MaxLength(500)]
        public string? ImageUrl { get; set; }

        public bool IsActive { get; set; } = true;


        public int WorkSpaceId { get; set; }
        public virtual WorkSpace? WorkSpace { get; set; }
    }
}