
using System.Collections.Generic;

namespace WorkSpace.Application.DTOs.WorkSpaces
{
    public class WorkSpaceRoomDetailDto : WorkSpaceRoomListItemDto
    {
        public string? Description { get; set; }
        public string? AddressLine { get; set; }
        public string? Country { get; set; }
        public decimal PricePerHour { get; set; }
        public decimal PricePerMonth { get; set; }
        public bool IsActive { get; set; }
        public IEnumerable<string> Images { get; set; } = Enumerable.Empty<string>();
        public IEnumerable<string> Amenities { get; set; } = Enumerable.Empty<string>();
    }
}