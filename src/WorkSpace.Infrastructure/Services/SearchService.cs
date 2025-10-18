
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


        public async Task<Response<IEnumerable<WorkSpaceRoomListItemDto>>> SearchWorkSpaceRoomsAsync(SearchRequestDto request)
        {

            var query = _context.WorkSpaceRooms
                .Include(r => r.WorkSpace)
                    .ThenInclude(w => w.Address)
                .Include(r => r.WorkSpaceRoomImages)
                .Include(r => r.Reviews)
                .Include(r => r.BlockedTimeSlots)       
                .Include(r => r.AvailabilitySchedules) 
                .Include(r => r.WorkSpaceRoomType)
                .Include(r => r.WorkSpaceRoomAmenities)
                    .ThenInclude(wra => wra.Amenity)
                .Where(r => r.IsActive && r.WorkSpace.IsActive)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(request.LocationQuery)) { query = query.Where(r => r.WorkSpace.Address.Ward.Contains(request.LocationQuery)); }
            if (request.Capacity.HasValue && request.Capacity > 0) { query = query.Where(r => r.Capacity >= request.Capacity.Value); }
            if (request.MinPrice.HasValue) { query = query.Where(r => r.PricePerDay >= request.MinPrice.Value); }
            if (request.MaxPrice.HasValue) { query = query.Where(r => r.PricePerDay <= request.MaxPrice.Value); }
            if (!string.IsNullOrWhiteSpace(request.Keyword)) { query = query.Where(r => r.Title.Contains(request.Keyword) || (r.Description != null && r.Description.Contains(request.Keyword)) || r.WorkSpace.Title.Contains(request.Keyword) || (r.WorkSpace.Description != null && r.WorkSpace.Description.Contains(request.Keyword)) || r.WorkSpaceRoomType.Name.Contains(request.Keyword)); }
            if (request.Amenities != null && request.Amenities.Any()) { foreach (var amenityName in request.Amenities) { query = query.Where(r => r.WorkSpaceRoomAmenities.Any(wra => wra.Amenity.Name == amenityName && wra.IsAvailable)); } }


            var potentialRooms = await query.ToListAsync();

            IEnumerable<WorkSpaceRoom> finalRooms = potentialRooms;

            bool hasDateFilter = request.HasDateFilter();
            bool hasTimeFilter = request.HasTimeOfDayFilter();

            if (hasDateFilter || hasTimeFilter) 
            {
                finalRooms = potentialRooms.Where(room =>
                {

                    DateOnly startDateOnly = request.GetSearchStartDateOnly(); 
                    DateOnly endDateOnly = request.GetSearchEndDateOnly();   


                    for (var date = startDateOnly; date <= endDateOnly; date = date.AddDays(1))
                    {
   
                        if (!IsRoomAvailableOnDate(room, date, request.StartTimeOfDay, request.EndTimeOfDay))
                        {
                          
                            return false;
                        }
                    }
      
                    return true;
                }).ToList();
            }
       


            var dtoList = _mapper.Map<IEnumerable<WorkSpaceRoomListItemDto>>(finalRooms);
            return new Response<IEnumerable<WorkSpaceRoomListItemDto>>(dtoList, $"Found {dtoList.Count()} records matching criteria.");
        }

     
        private bool IsRoomAvailableOnDate(WorkSpaceRoom room, DateOnly date, TimeSpan? requestedStartTime, TimeSpan? requestedEndTime)
        {
            var dayOfWeek = date.DayOfWeek;

            var schedule = room.AvailabilitySchedules.FirstOrDefault(s => s.DayOfWeek == dayOfWeek && s.IsAvailable);
            if (schedule == null) return false; 

            TimeSpan checkStartTime, checkEndTime;
            bool checkSpecificTime = requestedStartTime.HasValue && requestedEndTime.HasValue;

            if (checkSpecificTime) 
            {
                checkStartTime = requestedStartTime.Value;
                checkEndTime = requestedEndTime.Value;
                if (checkEndTime <= checkStartTime) return false; 

    
                bool scheduleOverlap = checkStartTime < schedule.EndTime && schedule.StartTime < checkEndTime;
                if (!scheduleOverlap) return false; 

               
                checkStartTime = TimeSpan.FromTicks(Math.Max(checkStartTime.Ticks, schedule.StartTime.Ticks));
                checkEndTime = TimeSpan.FromTicks(Math.Min(checkEndTime.Ticks, schedule.EndTime.Ticks));
                if (checkEndTime <= checkStartTime) return false; 
            }
            else 
            {
                checkStartTime = schedule.StartTime;
                checkEndTime = schedule.EndTime;
                if (checkEndTime <= checkStartTime) return false; 
            }

        
            DateTime effectiveStartDateTime = date.ToDateTime(TimeOnly.FromTimeSpan(checkStartTime), DateTimeKind.Unspecified); 
            DateTime effectiveEndDateTime = date.ToDateTime(TimeOnly.FromTimeSpan(checkEndTime), DateTimeKind.Unspecified);

            bool isBlocked = room.BlockedTimeSlots.Any(slot =>

                slot.StartTime < effectiveEndDateTime && slot.EndTime > effectiveStartDateTime
            );

            return !isBlocked; 
        }

        public async Task<IEnumerable<string>> GetLocationSuggestionsAsync(string query)
        {
            if (string.IsNullOrWhiteSpace(query))
            {
                return Enumerable.Empty<string>();
            }
            var wards = await _context.Addresses
                .Where(a => a.Ward != null && a.Ward.Contains(query))
                .Select(a => a.Ward!)
                .Distinct()
                .Take(5)
                .ToListAsync();
            return wards;
        }

        public async Task<IEnumerable<string>> GetAllWardsAsync()
        {
            var wards = await _context.Addresses
                .Where(a => a.Ward != null)
                .Select(a => a.Ward!)
                .Distinct()
                .OrderBy(w => w)
                .ToListAsync();
            return wards;
        }
    }
}