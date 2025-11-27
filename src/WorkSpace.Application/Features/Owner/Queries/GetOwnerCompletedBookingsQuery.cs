using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using WorkSpace.Application.DTOs.Bookings;
using WorkSpace.Application.Exceptions;
using WorkSpace.Application.Interfaces;
using WorkSpace.Application.Interfaces.Repositories;

namespace WorkSpace.Application.Features.Owner.Queries
{
    // 1. Sửa kiểu trả về của IRequest thành IEnumerable<BookingAdminDto> (bỏ Response<...>)
    public class GetOwnerCompletedBookingsQuery : IRequest<IEnumerable<BookingAdminDto>>
    {
        [JsonIgnore]
        public int OwnerUserId { get; set; }
    }

    // 2. Cập nhật IRequestHandler tương ứng
    public class GetOwnerCompletedBookingsQueryHandler : IRequestHandler<GetOwnerCompletedBookingsQuery, IEnumerable<BookingAdminDto>>
    {
        private readonly IApplicationDbContext _context;
        private readonly IHostProfileAsyncRepository _hostRepo;

        public GetOwnerCompletedBookingsQueryHandler(IApplicationDbContext context, IHostProfileAsyncRepository hostRepo)
        {
            _context = context;
            _hostRepo = hostRepo;
        }

        public async Task<IEnumerable<BookingAdminDto>> Handle(GetOwnerCompletedBookingsQuery request, CancellationToken cancellationToken)
        {
            var hostProfile = await _hostRepo.GetHostProfileByUserId(request.OwnerUserId, cancellationToken);
            if (hostProfile == null) throw new ApiException("Owner profile not found.");

            var bookings = await _context.Bookings
                .Include(b => b.Customer)
                .Include(b => b.WorkSpaceRoom.WorkSpace)
                .Include(b => b.BookingStatus)
                .Where(b => b.WorkSpaceRoom.WorkSpace.HostId == hostProfile.Id)
                .Where(b => b.BookingStatus.Name == "Completed")
                .OrderByDescending(b => b.CreateUtc)
                .AsNoTracking()
                .ToListAsync(cancellationToken);

            var dtos = bookings.Select(b => new BookingAdminDto
            {
                Id = b.Id,
                BookingCode = b.BookingCode,
                CustomerId = b.CustomerId,
                CustomerName = b.Customer?.GetFullName(),
                CustomerEmail = b.Customer?.Email,
                WorkSpaceRoomId = b.WorkSpaceRoomId,
                WorkSpaceRoomTitle = b.WorkSpaceRoom?.Title,
                StartTimeUtc = b.StartTimeUtc,
                EndTimeUtc = b.EndTimeUtc,
                FinalAmount = b.FinalAmount,
                Currency = b.Currency,
                BookingStatusId = b.BookingStatusId,
                BookingStatusName = b.BookingStatus?.Name,
                CreateUtc = b.CreateUtc,
                CheckedInAt = b.CheckedInAt,
                CheckedOutAt = b.CheckedOutAt,
                IsReviewed = b.IsReviewed
            }).ToList();

     
            return dtos;
        }
    }
}