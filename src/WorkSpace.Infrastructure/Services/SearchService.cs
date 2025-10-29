// src/WorkSpace.Infrastructure/Services/SearchService.cs
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
        private readonly IMapper _mapper; // Mapper có thể không cần nếu bạn map thủ công

        public SearchService(WorkSpaceContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<Response<IEnumerable<WorkSpaceSearchResultDto>>> SearchWorkSpacesAsync(SearchRequestDto request)
        {
            var query = _context.Workspaces
                .Include(w => w.Address)
                .Include(w => w.Host) // Include HostProfile
                    .ThenInclude(h => h.User) // Include User để lấy tên
                .Include(w => w.WorkSpaceRooms)
                    .ThenInclude(r => r.WorkSpaceRoomAmenities) // Include bảng trung gian
                        .ThenInclude(wra => wra.Amenity) // Include Amenity từ bảng trung gian
                .Include(w => w.WorkSpaceRooms)
                    .ThenInclude(r => r.BlockedTimeSlots)
                .Where(w => w.IsActive && w.IsVerified)
                .AsQueryable();

            // ... (Các bộ lọc khác giữ nguyên: Ward, Keyword, Capacity, MinPrice, MaxPrice)

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
                // Giả sử lọc theo PricePerDay
                query = query.Where(w => w.WorkSpaceRooms.Any(r => r.PricePerDay >= request.MinPrice.Value && r.IsActive && r.IsVerified));
            }

            if (request.MaxPrice.HasValue)
            {
                // Giả sử lọc theo PricePerDay
                query = query.Where(w => w.WorkSpaceRooms.Any(r => r.PricePerDay <= request.MaxPrice.Value && r.IsActive && r.IsVerified));
            }


            // Sửa lại cách lọc Amenity
            if (request.Amenities != null && request.Amenities.Any())
            {
                foreach (var amenityName in request.Amenities.Where(a => !string.IsNullOrWhiteSpace(a)))
                {
                    // Kiểm tra xem có bất kỳ phòng nào (đang active và verified)
                    // chứa amenity có tên trùng khớp không
                    query = query.Where(w => w.WorkSpaceRooms.Any(r =>
                        r.IsActive && r.IsVerified && // Chỉ xét phòng active/verified
                        r.WorkSpaceRoomAmenities.Any(wra => wra.Amenity != null && wra.Amenity.Name == amenityName)
                    ));
                }
            }


            // ... (Bộ lọc thời gian giữ nguyên)
            if (request.HasDateTimeFilter())
            {
                // Sử dụng Offset để đảm bảo tính đúng múi giờ nếu cần
                DateTimeOffset effectiveStartTime = request.StartTime ?? DateTimeOffset.MinValue;
                DateTimeOffset effectiveEndTime = request.EndTime ?? DateTimeOffset.MaxValue;

                if (request.StartTime.HasValue && request.EndTime.HasValue && effectiveEndTime <= effectiveStartTime)
                {
                    return new Response<IEnumerable<WorkSpaceSearchResultDto>>("End time must be after start time.") { Succeeded = false };
                }

                // Chuyển sang UTC nếu cần so sánh với DB lưu UTC
                effectiveStartTime = effectiveStartTime.ToUniversalTime();
                effectiveEndTime = effectiveEndTime.ToUniversalTime();


                // Tìm các Workspace có *ít nhất một phòng* rảnh trong khoảng thời gian đó
                query = query.Where(w => w.WorkSpaceRooms.Any(room =>
                   room.IsActive && room.IsVerified && // Chỉ phòng hoạt động, đã duyệt
                                                       // Không có lịch đặt nào bị trùng (chỉ xét các trạng thái chiếm chỗ)
                   !room.Bookings.Any(b =>
                       b.StartTimeUtc < effectiveEndTime && b.EndTimeUtc > effectiveStartTime &&
                       (b.BookingStatusId == 2 || b.BookingStatusId == 3)) && // Giả sử 2=Confirmed, 3=InProgress
                                                                              // Không có thời gian chặn nào bị trùng
                   !room.BlockedTimeSlots.Any(slot =>
                       slot.StartTime < effectiveEndTime.DateTime && slot.EndTime > effectiveStartTime.DateTime // Chuyển sang DateTime nếu DB lưu DateTime
                   )
               ));
            }


            var workspaces = await query
                .Distinct()
                .ToListAsync();

            // Map kết quả sang DTO, bao gồm HostName
            var dtoList = workspaces.Select(w => new WorkSpaceSearchResultDto
            {
                Id = w.Id,
                Title = w.Title,
                Description = w.Description,
                Ward = w.Address?.Ward,
                Street = w.Address?.Street,
                // Lấy tên Host từ User liên kết với HostProfile
                HostName = w.Host?.User?.GetFullName() // Sử dụng hàm GetFullName() nếu có
                           ?? w.Host?.User?.UserName // Hoặc fallback về UserName
                           ?? "N/A" // Hoặc giá trị mặc định nếu không có thông tin
            }).ToList();

            int count = dtoList.Count();
            return new Response<IEnumerable<WorkSpaceSearchResultDto>>(dtoList, $"Found {count} records matching criteria.");
        }

        // ... (GetLocationSuggestionsAsync và GetAllWardsAsync giữ nguyên)
        public async Task<IEnumerable<string>> GetLocationSuggestionsAsync(string query)
        {
            if (string.IsNullOrWhiteSpace(query))
            {
                return Enumerable.Empty<string>();
            }
            // Tìm các Quận/Huyện (Ward) khớp
            var wards = await _context.Addresses
                .Where(a => a.Ward != null && a.Ward.Contains(query))
                .Select(a => a.Ward!) // Thêm ! để báo compiler là không null
                .Distinct()
                .Take(5) // Giới hạn số lượng gợi ý
                .ToListAsync();

            // Có thể thêm tìm kiếm theo Tỉnh/Thành phố (State) nếu cần
            var states = await _context.Addresses
                .Where(a => a.State != null && a.State.Contains(query))
                .Select(a => a.State!)
                .Distinct()
                .Take(3)
                .ToListAsync();

            // Kết hợp và loại bỏ trùng lặp
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