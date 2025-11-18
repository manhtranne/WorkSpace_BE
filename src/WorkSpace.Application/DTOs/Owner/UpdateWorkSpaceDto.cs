using System.ComponentModel.DataAnnotations;

namespace WorkSpace.Application.DTOs.Owner
{
    public class UpdateWorkSpaceDto
    {
        [MaxLength(255)]
        public string? Title { get; set; }

        [MaxLength(1000)]
        public string? Description { get; set; }

        public int? AddressId { get; set; }

        public int? WorkSpaceTypeId { get; set; }

        public bool? IsActive { get; set; }
    }
}