using AutoMapper;
using Microsoft.EntityFrameworkCore;
using WorkSpace.Application.DTOs.WorkSpaces;
using WorkSpace.Application.Interfaces.Services;
using WorkSpace.Application.Wrappers;
using WorkSpace.Domain.Entities;
using WorkSpace.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

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

        public async Task<Response<IEnumerable<WorkSpaceRoomListItemDto>>> SearchWorkSpaceRoomsAsync(SearchRequestDto request)
        {
            var query = _context.WorkSpaceRooms
                .Include(r => r.WorkSpace)
                    .ThenInclude(w => w.Address)
                .Include(r => r.WorkSpaceRoomImages)
                .Include(r => r.Reviews)
                .Include(r => r.BlockedTimeSlots)
                .Include(r => r.AvailabilitySchedules)
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

            var potentialRooms = await query.ToListAsync();
            IEnumerable<WorkSpaceRoom> finalRooms = potentialRooms;

            if (request.StartTime.HasValue && request.EndTime.HasValue)
            {
                var startTime = request.StartTime.Value;
                var endTime = request.EndTime.Value;

                finalRooms = potentialRooms.Where(room =>
                {
                    
                    bool isBlocked = room.BlockedTimeSlots.Any(slot =>
                        slot.StartTime < endTime && slot.EndTime > startTime);

                    if (isBlocked) return false;

                  
                    for (var date = startTime.Date; date <= endTime.Date; date = date.AddDays(1))
                    {
                        var dayOfWeek = date.DayOfWeek;
                        var schedule = room.AvailabilitySchedules.FirstOrDefault(s => s.DayOfWeek == dayOfWeek);

                        if (schedule == null || !schedule.IsAvailable) return false;

                      
                        var requestedStartOfDay = (date == startTime.Date) ? startTime.TimeOfDay : TimeSpan.Zero;
                        var requestedEndOfDay = (date == endTime.Date) ? endTime.TimeOfDay : new TimeSpan(23, 59, 59);

                        if (requestedEndOfDay <= schedule.StartTime || requestedStartOfDay >= schedule.EndTime)
                        {
                            return false;
                        }
                    }

                    return true;
                }).ToList();
            }


            var dtoList = _mapper.Map<IEnumerable<WorkSpaceRoomListItemDto>>(finalRooms);
            return new Response<IEnumerable<WorkSpaceRoomListItemDto>>(dtoList, $"Found {dtoList.Count()} records.");
        }
    }
}