using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WorkSpace.Application.DTOs.Bookings;
using WorkSpace.Application.DTOs.Guest;
using WorkSpace.Application.Interfaces.Repositories;
using WorkSpace.Application.Interfaces.Services;

namespace WorkSpace.Infrastructure.Services
{

    public class BookingService : IBookingService
    {
        private readonly IBookingRepository _bookingRepository;
        private readonly IGuestRepository _guestRepository;

        public BookingService(IBookingRepository bookingRepository, IGuestRepository guestRepository)
        {
            _bookingRepository = bookingRepository;
            _guestRepository = guestRepository;
        }

        public async Task<int> HandleGuestBookingAsync(CreateBookingDto bookingDto, GuestInfo guestInfo)
        {
            int guestId = await _guestRepository.GetOrCreateGuestAsync(guestInfo);
            int bookingId = await _bookingRepository.CreateBookingGuestAsync(guestId, bookingDto);
            // gửi mail..
            return bookingId;
        }
    }
}
