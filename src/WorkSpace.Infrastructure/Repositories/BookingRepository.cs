using Microsoft.EntityFrameworkCore;
using Org.BouncyCastle.Utilities;
using WorkSpace.Application.DTOs.Bookings;
using WorkSpace.Application.Interfaces.Repositories;
using WorkSpace.Domain.Entities;

namespace WorkSpace.Infrastructure.Repositories;

public class BookingRepository : IBookingRepository
{
    private readonly WorkSpaceContext _context;

    public BookingRepository(WorkSpaceContext dbContext)
    {
        _context = dbContext;
    }

    private string GenerateBookingCode()
    {
        return "BK-" + Guid.NewGuid().ToString("N")[..8].ToUpper();
    }


    public async Task<int> CreateBookingAsync(int userId, CreateBookingDto bookingDto)
    {
        var booking = new Booking
        {
            BookingCode = GenerateBookingCode(),
            CustomerId = userId,
            GuestId = null,
            WorkSpaceRoomId = bookingDto.WorkSpaceRoomId,
            StartTimeUtc = bookingDto.StartTimeUtc,
            EndTimeUtc = bookingDto.EndTimeUtc,
            NumberOfParticipants = bookingDto.NumberOfParticipants,
            SpecialRequests = bookingDto.SpecialRequests,
            TotalPrice = bookingDto.TotalPrice,
            TaxAmount = bookingDto.TaxAmount,
            ServiceFee = bookingDto.ServiceFee,
            FinalAmount = bookingDto.FinalAmount,
            Currency = "VND",
            BookingStatusId = 1 // Default to 'Pending'
        };
        _context.Bookings.Add(booking);
        await _context.SaveChangesAsync();
        return booking.Id;
    }

    public async Task<int> CreateBookingGuestAsync(int guestId, CreateBookingDto bookingDto)
    {
        var booking = new Booking
        {
            BookingCode = GenerateBookingCode(),
            CustomerId = null,
            GuestId = guestId,
            WorkSpaceRoomId = bookingDto.WorkSpaceRoomId,
            StartTimeUtc = bookingDto.StartTimeUtc,
            EndTimeUtc = bookingDto.EndTimeUtc,
            NumberOfParticipants = bookingDto.NumberOfParticipants,
            SpecialRequests = bookingDto.SpecialRequests,
            TotalPrice = bookingDto.TotalPrice,
            TaxAmount = bookingDto.TaxAmount,
            ServiceFee = bookingDto.ServiceFee,
            FinalAmount = bookingDto.FinalAmount,
            Currency = "VND",
            BookingStatusId = 3 // 'Pending'
        };
        _context.Bookings.Add(booking);
        await _context.SaveChangesAsync();
        return booking.Id;
    }

    public async Task DeleteBookingAsync(int id)
    {
        var booking = await _context.Bookings.FindAsync(id);
        if (booking != null)
        {
            _context.Bookings.Remove(booking);
            await _context.SaveChangesAsync();
        }
    }

    public async Task<IEnumerable<Booking>> GetAllBookingsAsync()
    {
        return await _context.Bookings.ToListAsync();
    }

    public async Task<Booking> GetBookingByIdAsync(int id)
    {
        return await _context.Bookings.FindAsync(id);
    }

    public async Task<IEnumerable<Booking>> GetBookingsByUserIdAsync(int userId)
    {
        return await _context.Bookings
            .Where(b => b.CustomerId == userId)
            .ToListAsync();
    }

    public Task UpdateBookingAsync(int id, Booking booking)
    {
        throw new NotImplementedException();
    }

    public async Task UpdateBookingStatusAsync(int bookingId, int bookingStatusId)
    {
        var booking = await _context.Bookings.FindAsync(bookingId);
        if (booking != null)
        {
            booking.BookingStatusId = bookingStatusId;
            await _context.SaveChangesAsync();
        }
    }
}