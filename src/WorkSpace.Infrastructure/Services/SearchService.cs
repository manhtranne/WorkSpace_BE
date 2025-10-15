// src/WorkSpace.Infrastructure/Services/SearchService.cs

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

            return wards;
        }

        public async Task<IEnumerable<string>> GetAllWardsAsync()
        {
            var wards = await _context.Addresses
                .Select(a => a.Ward)
                .Distinct()
                .OrderBy(w => w)
                .ToListAsync();

            return wards;
        }

        public async Task<PagedResponse<IEnumerable<WorkSpaceRoomListItemDto>>> SearchWorkSpaceRoomsAsync(SearchRequestDto request)
        {
            var query = _context.WorkSpaceRooms
                .Include(r => r.WorkSpace)
                    .ThenInclude(w => w.Address)
                .Include(r => r.WorkSpaceRoomImages)
                .Include(r => r.Reviews)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(request.LocationQuery))
            {
                query = query.Where(r => r.WorkSpace.Address.Ward.Contains(request.LocationQuery));
            }

      
            if (request.Capacity.HasValue && request.Capacity > 0)
            {
                query = query.Where(r => r.Capacity >= request.Capacity.Value);
            }

    
            if (request.MinPrice.HasValue)
            {
                query = query.Where(r => r.PricePerDay >= request.MinPrice.Value);
            }
            if (request.MaxPrice.HasValue)
            {
                query = query.Where(r => r.PricePerDay <= request.MaxPrice.Value);
            }

 
            if (!string.IsNullOrWhiteSpace(request.Keyword))
            {
                query = query.Where(r => r.Title.Contains(request.Keyword) ||
                                         r.WorkSpace.Title.Contains(request.Keyword) ||
                                         r.WorkSpaceRoomType.Name.Contains(request.Keyword));
            }

            if (request.Amenities != null && request.Amenities.Any())
            {
                foreach (var amenity in request.Amenities)
                {
                    query = query.Where(r => r.WorkSpaceRoomAmenities.Any(a => a.Amenity.Name == amenity));
                }
            }

      
            if (request.StartTime.HasValue && request.EndTime.HasValue)
            {
                var startTime = request.StartTime.Value;
                var endTime = request.EndTime.Value;

                query = query.Where(room =>
              
                    !room.BlockedTimeSlots.Any(slot =>
                        (slot.StartTime < endTime && slot.EndTime > startTime)
                    ) &&

                    !Enumerable.Range(0, (endTime.Date - startTime.Date).Days + 1)
                                .Select(offset => startTime.Date.AddDays(offset))
                                .Any(date => !room.AvailabilitySchedules.Any(schedule =>
                                     schedule.DayOfWeek == date.DayOfWeek &&
                                     schedule.StartTime <= (date == startTime.Date ? startTime.TimeOfDay : TimeSpan.Zero) &&
                                     schedule.EndTime >= (date == endTime.Date ? endTime.TimeOfDay : new TimeSpan(23, 59, 59))
                                ))
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