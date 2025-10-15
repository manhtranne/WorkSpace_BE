using AutoMapper;
using Microsoft.EntityFrameworkCore;
using WorkSpace.Application.DTOs.WorkSpaces;
using WorkSpace.Application.Interfaces.Services;
using WorkSpace.Application.Wrappers;
using WorkSpace.Domain.Entities;
using WorkSpace.Infrastructure;
namespace WorkSpace.Infrastructure.Services
{
    public class SearchService : ISearchService
    {
        private readonly WorkSpaceContext _context;
        private readonly IMapper _mapper;

        public SearchService(WorkSpaceContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<IEnumerable<string>> GetLocationSuggestionsAsync(string query)
        {
            if (string.IsNullOrWhiteSpace(query))
            {
                return Enumerable.Empty<string>();
            }

            var wards = await _context.Addresses
                .Where(a => a.Ward.Contains(query))
                .Select(a => a.Ward)
                .Distinct()
                .Take(5)
                .ToListAsync();

            var districts = await _context.Addresses
                .Where(a => a.District.Contains(query))
                .Select(a => a.District)
                .Distinct()
                .Take(5)
                .ToListAsync();

            return districts.Concat(wards).Distinct();
        }

        public async Task<PagedResponse<IEnumerable<WorkSpaceRoomListItemDto>>> SearchWorkSpaceRoomsAsync(SearchRequestDto request)
        {
            var query = _context.WorkSpaceRooms
                .Include(r => r.WorkSpace)
                    .ThenInclude(w => w.Address)
                .Include(r => r.WorkSpaceRoomImages)
                .Include(r => r.Reviews)
                .AsQueryable();

            // 1. Filter theo địa điểm (Phường hoặc Quận)
            if (!string.IsNullOrWhiteSpace(request.LocationQuery))
            {
                query = query.Where(r => r.WorkSpace.Address.Ward.Contains(request.LocationQuery) ||
                                         r.WorkSpace.Address.District.Contains(request.LocationQuery));
            }

            // 2. Filter theo sức chứa
            if (request.Capacity.HasValue && request.Capacity > 0)
            {
                query = query.Where(r => r.Capacity >= request.Capacity.Value);
            }

            // 3. Filter theo giá
            if (request.MinPrice.HasValue)
            {
                query = query.Where(r => r.PricePerDay >= request.MinPrice.Value);
            }
            if (request.MaxPrice.HasValue)
            {
                query = query.Where(r => r.PricePerDay <= request.MaxPrice.Value);
            }

            // 4. Filter theo từ khóa
            if (!string.IsNullOrWhiteSpace(request.Keyword))
            {
                query = query.Where(r => r.Title.Contains(request.Keyword) ||
                                         r.WorkSpace.Title.Contains(request.Keyword) ||
                                         r.WorkSpaceRoomType.Name.Contains(request.Keyword));
            }

            // 5. Filter theo tiện ích (Amenities)
            if (request.Amenities != null && request.Amenities.Any())
            {
                foreach (var amenity in request.Amenities)
                {
                    query = query.Where(r => r.WorkSpaceRoomAmenities.Any(a => a.Amenity.Name == amenity));
                }
            }

            // 6. Filter theo thời gian trống (Availability)
            if (request.StartTime.HasValue && request.EndTime.HasValue)
            {
                var startTime = request.StartTime.Value;
                var endTime = request.EndTime.Value;

                query = query.Where(room =>
                    // Điều kiện a: Không có lịch chặn nào trùng với thời gian yêu cầu
                    !room.BlockedTimeSlots.Any(slot =>
                        (slot.StartTime < endTime && slot.EndTime > startTime)
                    ) &&
                    // Điều kiện b: Phải có lịch khả dụng trong khung giờ đó
                    room.AvailabilitySchedules.Any(schedule =>
                        schedule.DayOfWeek == startTime.DayOfWeek &&
                        schedule.StartTime <= startTime.TimeOfDay &&
                        schedule.EndTime >= endTime.TimeOfDay
                    )
                );
            }

            var totalRecords = await query.CountAsync();

            var rooms = await query
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToListAsync();

            var dtoList = _mapper.Map<IEnumerable<WorkSpaceRoomListItemDto>>(rooms);

            return new PagedResponse<IEnumerable<WorkSpaceRoomListItemDto>>(dtoList, request.PageNumber, request.PageSize)
            {
                Message = $"Found {totalRecords} records."
            };
        }
    }
}