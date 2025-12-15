using System.ComponentModel.DataAnnotations;

namespace WorkSpace.Application.DTOs.Services
{
    public class CreateWorkSpaceServiceDto
    {
        [Required]
        public string Name { get; set; } = default!;

        public string? Description { get; set; }

        [Range(0, double.MaxValue)]
        public decimal Price { get; set; }

        public string? ImageUrl { get; set; }
    }
}