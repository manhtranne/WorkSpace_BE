using WorkSpace.Domain.Entities;

namespace WorkSpace.Application.Interfaces.Repositories;

public interface IBookingRepository 
{
    Task<IEnumerable<Booking>> GetAllBookingsAsync();
    Task<Booking> GetBookingByIdAsync(int id);
    Task<int> CreateBookingAsync(string userId);
    Task<int> CreateBookingGuestAsync(string guestId);
    Task UpdateBookingAsync(int id, Booking booking);
    Task UpdateBookingStatusAsync(int bookingId, int bookingStatusId);
    Task DeleteBookingAsync(int id);
    Task<IEnumerable<Booking>> GetBookingsByUserIdAsync(string userId);

}