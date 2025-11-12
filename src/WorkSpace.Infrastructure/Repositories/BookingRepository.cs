using Microsoft.EntityFrameworkCore;
using WorkSpace.Application.Interfaces.Repositories;
using WorkSpace.Domain.Entities;

namespace WorkSpace.Infrastructure.Repositories;

public class BookingRepository : IBookingRepository
{
    public Task<int> CreateBookingAsync(string userId)
    {
        throw new NotImplementedException();
    }

    public Task<int> CreateBookingGuestAsync(string guestId)
    {
        throw new NotImplementedException();
    }

    public Task DeleteBookingAsync(int id)
    {
        throw new NotImplementedException();
    }

    public Task<IEnumerable<Booking>> GetAllBookingsAsync()
    {
        throw new NotImplementedException();
    }

    public Task<Booking> GetBookingByIdAsync(int id)
    {
        throw new NotImplementedException();
    }

    public Task<IEnumerable<Booking>> GetBookingsByUserIdAsync(string userId)
    {
        throw new NotImplementedException();
    }

    public Task UpdateBookingAsync(int id, Booking booking)
    {
        throw new NotImplementedException();
    }

    public Task UpdateBookingStatusAsync(int bookingId, int bookingStatusId)
    {
        throw new NotImplementedException();
    }
}