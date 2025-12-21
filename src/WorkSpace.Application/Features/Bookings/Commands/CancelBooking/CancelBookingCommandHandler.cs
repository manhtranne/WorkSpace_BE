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
            // 1. Kiểm tra sự tồn tại của booking
            var booking = await _bookingRepository.GetBookingByIdAsync(request.BookingId);

            if (booking == null)
            {
                return new Response<bool>("Không tìm thấy đơn đặt phòng.");
            }

            // 2. Kiểm tra quyền sở hữu (Booking phải thuộc về User yêu cầu hủy)
            if (booking.CustomerId != request.UserId)
            {
                return new Response<bool>("Bạn không có quyền hủy đơn đặt phòng này.");
            }

            // 3. Kiểm tra trạng thái booking (Chỉ cho phép hủy nếu chưa hoàn thành/đã hủy)
            // Giả sử StatusId 7 là Cancelled, 5 là Completed
            if (booking.BookingStatusId == 7 || booking.BookingStatusId == 9)
            {
                return new Response<bool>("Đơn hàng này đã hoàn thành hoặc đã bị hủy trước đó.");
            }

            try
            {
                // 4. Cập nhật lý do hủy và trạng thái Booking
                booking.CancellationReason = request.Reason;
                int cancelledStatusId = 7;
                await _bookingRepository.UpdateBookingStatusAsync(booking.Id, cancelledStatusId);

                // 5. LOGIC GIẢI PHÓNG KHUNG GIỜ:
                // Tìm BlockedTimeSlot được tạo ra bởi đơn đặt phòng này.
                // Chúng ta tìm dựa trên RoomId và khung thời gian khớp tuyệt đối với booking.
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