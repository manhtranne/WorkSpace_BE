
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
using System.Threading; 

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
        
            CancellationToken cancellationToken = default;

            var query = _context.Workspaces
                .Include(w => w.Address)
                .Include(w => w.Host)
                    .ThenInclude(h => h.User)
                .Include(w => w.WorkSpaceRooms)
                    .ThenInclude(r => r.WorkSpaceRoomAmenities)
                        .ThenInclude(wra => wra.Amenity)
                .Include(w => w.WorkSpaceRooms)
                    .ThenInclude(r => r.BlockedTimeSlots)
                 .Include(w => w.WorkSpaceRooms) 
                    .ThenInclude(r => r.WorkSpaceRoomImages) 
                .Where(w => w.IsActive && w.IsVerified)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(request.Ward))
            {
                query = query.Where(w => w.Address != null && w.Address.Ward != null && w.Address.Ward.Contains(request.Ward));
            }

            if (!string.IsNullOrWhiteSpace(request.Keyword))
            {
                query = query.Where(w => w.Title.Contains(request.Keyword)
                                         || (w.Description != null && w.Description.Contains(request.Keyword)));
            }
            if (request.Capacity.HasValue && request.Capacity > 0)
            {
                query = query.Where(w => w.WorkSpaceRooms.Any(r => r.Capacity >= request.Capacity.Value && r.IsActive && r.IsVerified));
            }
            if (request.MinPrice.HasValue)
            {
                query = query.Where(w => w.WorkSpaceRooms.Any(r => r.PricePerDay >= request.MinPrice.Value && r.IsActive && r.IsVerified));
            }

            if (request.MaxPrice.HasValue)
            {
                query = query.Where(w => w.WorkSpaceRooms.Any(r => r.PricePerDay <= request.MaxPrice.Value && r.IsActive && r.IsVerified));
            }

            if (request.Amenities != null && request.Amenities.Any())
            {
                foreach (var amenityName in request.Amenities.Where(a => !string.IsNullOrWhiteSpace(a)))
                {
                    query = query.Where(w => w.WorkSpaceRooms.Any(r =>
                        r.IsActive && r.IsVerified &&
                        r.WorkSpaceRoomAmenities.Any(wra => wra.Amenity != null && wra.Amenity.Name == amenityName)
                    ));
                }
            }

            if (request.HasDateTimeFilter())
            {
                DateTimeOffset effectiveStartTime = request.StartTime ?? DateTimeOffset.MinValue;
                DateTimeOffset effectiveEndTime = request.EndTime ?? DateTimeOffset.MaxValue;

                if (request.StartTime.HasValue && request.EndTime.HasValue && effectiveEndTime <= effectiveStartTime)
                {
                    return new Response<IEnumerable<WorkSpaceSearchResultDto>>("End time must be after start time.") { Succeeded = false };
                }

                effectiveStartTime = effectiveStartTime.ToUniversalTime();
                effectiveEndTime = effectiveEndTime.ToUniversalTime();

                query = query.Where(w => w.WorkSpaceRooms.Any(room =>
                   room.IsActive && room.IsVerified &&
                   !room.Bookings.Any(b =>
                       b.StartTimeUtc < effectiveEndTime && b.EndTimeUtc > effectiveStartTime &&
                       b.BookingStatus != null &&
                       (b.BookingStatus.Name == "Confirmed" || b.BookingStatus.Name == "InProgress" || b.BookingStatus.Name == "Pending")) &&
                   !room.BlockedTimeSlots.Any(slot =>
                       slot.StartTime < effectiveEndTime.DateTime && slot.EndTime > effectiveStartTime.DateTime
                   )
               ));
            }
    

            var workspaces = await query
                .Distinct()
                .ToListAsync(cancellationToken);


            var dtoList = workspaces.Select(w => new WorkSpaceSearchResultDto
            {
                Id = w.Id,
                Title = w.Title,
                Description = w.Description,
                Ward = w.Address?.Ward,
                Street = w.Address?.Street,
                HostName = w.Host?.User?.GetFullName()
                           ?? w.Host?.User?.UserName
                           ?? "N/A",
           
                ImageUrls = w.WorkSpaceRooms
                                .Where(r => r.IsActive && r.IsVerified) 
                                .SelectMany(r => r.WorkSpaceRoomImages) 
                                .Select(img => img.ImageUrl) 
                                .Distinct() 
                                .ToList() 
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

            var states = await _context.Addresses
                .Where(a => a.State != null && a.State.Contains(query))
                .Select(a => a.State!)
                .Distinct()
                .Take(3)
                .ToListAsync();

            return wards.Concat(states).Distinct().OrderBy(s => s);
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