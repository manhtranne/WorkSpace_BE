using MediatR;
using WorkSpace.Application.Exceptions;
using WorkSpace.Application.Interfaces.Repositories;
using WorkSpace.Application.Wrappers;
using WorkSpace.Domain.Entities;

namespace WorkSpace.Application.Features.Bookings.Commands
{
    public class CancelBookingCommandHandler : IRequestHandler<CancelBookingCommand, Response<bool>>
    {
        private readonly IBookingRepository _bookingRepository;
        private readonly IBlockedTimeSlotRepository _blockedTimeRepository;

        public CancelBookingCommandHandler(
            IBookingRepository bookingRepository,
            IBlockedTimeSlotRepository blockedTimeRepository)
        {
            _bookingRepository = bookingRepository;
            _blockedTimeRepository = blockedTimeRepository;
        }

        public async Task<Response<bool>> Handle(CancelBookingCommand request, CancellationToken cancellationToken)
        {
       
            var booking = await _bookingRepository.GetBookingByIdAsync(request.BookingId);

            if (booking == null)
            {
                return new Response<bool>("Không tìm thấy đơn đặt phòng.");
            }

            if (booking.CustomerId != request.UserId)
            {
                return new Response<bool>("Bạn không có quyền hủy đơn đặt phòng này.");
            }

    
            if (booking.BookingStatusId == 7 || booking.BookingStatusId == 9)
            {
                return new Response<bool>("Đơn hàng này đã hoàn thành hoặc đã bị hủy trước đó.");
            }

            try
            {
                booking.CancellationReason = request.Reason;
                int cancelledStatusId = 7;
                await _bookingRepository.UpdateBookingStatusAsync(booking.Id, cancelledStatusId);

      
                var blockedSlot = await _blockedTimeRepository.GetByRoomAndTimeAsync(
                    booking.WorkSpaceRoomId,
                    booking.StartTimeUtc,
                    booking.EndTimeUtc,
                    cancellationToken);

                if (blockedSlot != null)
                {
                    // Xóa block để người khác có thể đặt khung giờ này
                    await _blockedTimeRepository.DeleteAsync(blockedSlot);
                }

                return new Response<bool>(true, "Hủy đặt phòng và giải phóng khung giờ thành công.");
            }
            catch (Exception ex)
            {
                return new Response<bool>($"Lỗi hệ thống khi xử lý hủy: {ex.Message}");
            }
        }
    }
}