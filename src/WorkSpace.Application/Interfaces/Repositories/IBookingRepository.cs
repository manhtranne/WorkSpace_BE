using WorkSpace.Application.DTOs.Bookings;
using WorkSpace.Domain.Entities;

namespace WorkSpace.Application.Interfaces.Repositories;

public interface IBookingRepository 
{
    Task<IEnumerable<Booking>> GetAllBookingsAsync();
    Task<Booking> GetBookingByIdAsync(int id);
    Task<int> CreateBookingCustomerAsync(int userId, CreateBookingDto bookingDto);
    Task<int> CreateBookingGuestAsync(int guestId, CreateBookingDto bookingDto);
    Task UpdateBookingAsync(int id, Booking booking);
    Task UpdateBookingStatusAsync(int bookingId, int bookingStatusId);
    Task DeleteBookingAsync(int id);
    Task<IEnumerable<Booking>> GetBookingsByUserIdAsync(int userId);

}