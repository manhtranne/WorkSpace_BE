using MediatR;
using WorkSpace.Application.Interfaces.Repositories;
using WorkSpace.Application.Wrappers;
using WorkSpace.Domain.Entities;

namespace WorkSpace.Application.Features.Bookings.Commands
{
    public class CancelBookingCommandHandler : IRequestHandler<CancelBookingCommand, Response<bool>>
    {
        private readonly IBookingRepository _bookingRepository;

        public CancelBookingCommandHandler(IBookingRepository bookingRepository)
        {
            _bookingRepository = bookingRepository;
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
                return new Response<bool>("Bạn không có quyền hủy đơn đặt phòng này (Booking không thuộc về bạn).");
            }


            try
            {
                booking.CancellationReason = request.Reason;

                int cancelledStatusId = 7;
                await _bookingRepository.UpdateBookingStatusAsync(booking.Id, cancelledStatusId);



                return new Response<bool>(true, "Hủy đặt phòng thành công.");
            }
            catch (Exception ex)
            {
                return new Response<bool>($"Lỗi hệ thống: {ex.Message}");
            }
        }
    }
}