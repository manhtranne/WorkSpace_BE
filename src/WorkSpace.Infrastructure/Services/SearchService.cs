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

        public async Task<Response<IEnumerable<WorkSpaceSearchResultDto>>> SearchWorkSpacesAsync(SearchRequestDto request)
        {
            var query = _context.Workspaces
                .Include(w => w.Address)
                .Include(w => w.WorkSpaceRooms)
                    .ThenInclude(r => r.WorkSpaceRoomAmenities)
                        .ThenInclude(wra => wra.Amenity)
                .Include(w => w.WorkSpaceRooms)
                    .ThenInclude(r => r.BlockedTimeSlots)
                .Where(w => w.IsActive && w.IsVerified)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(request.Ward))
            {
                query = query.Where(w => w.Address.Ward.Contains(request.Ward));
            }

            if (!string.IsNullOrWhiteSpace(request.Keyword))
            {
                query = query.Where(w => w.Title.Contains(request.Keyword)
                                         || (w.Description != null && w.Description.Contains(request.Keyword)));
            }

  
            if (request.Capacity.HasValue && request.Capacity > 0)
            {
                query = query.Where(w => w.WorkSpaceRooms.Any(r => r.Capacity >= request.Capacity.Value && r.IsActive));
            }

            if (request.MinPrice.HasValue)
            {
                query = query.Where(w => w.WorkSpaceRooms.Any(r => r.PricePerDay >= request.MinPrice.Value && r.IsActive));
            }

            if (request.MaxPrice.HasValue)
            {
                query = query.Where(w => w.WorkSpaceRooms.Any(r => r.PricePerDay <= request.MaxPrice.Value && r.IsActive));
            }

            if (request.Amenities != null && request.Amenities.Any())
            {
                foreach (var amenityName in request.Amenities)
                {
                    query = query.Where(w => w.WorkSpaceRooms.Any(r =>
                        r.WorkSpaceRoomAmenities.Any(wra => wra.Amenity.Name == amenityName && wra.IsAvailable) && r.IsActive));
                }
            }

            if (request.HasDateTimeFilter())
            {
                DateTime effectiveStartTime = request.StartTime ?? DateTime.MinValue;
                DateTime effectiveEndTime = request.EndTime ?? DateTime.MaxValue;

                if (request.StartTime.HasValue && request.EndTime.HasValue && effectiveEndTime <= effectiveStartTime)
                {
                    return new Response<IEnumerable<WorkSpaceSearchResultDto>>("End time must be after start time.") { Succeeded = false };
                }

               
                query = query.Where(w => w.WorkSpaceRooms.Any(room =>
                    room.IsActive &&
                    !room.BlockedTimeSlots.Any(slot =>
                        slot.StartTime < effectiveEndTime && slot.EndTime > effectiveStartTime
                    )
                ));
            }

            var workspaces = await query
                .Distinct() 
                .ToListAsync();

       
            var dtoList = workspaces.Select(w => new WorkSpaceSearchResultDto
            {
                Id = w.Id,
                Title = w.Title,
                Description = w.Description,
                Ward = w.Address?.Ward, 
                Street = w.Address?.Street 
            }).ToList();

            int count = dtoList.Count();
            return new Response<IEnumerable<WorkSpaceSearchResultDto>>(dtoList, $"Found {count} records matching criteria.");
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