using AutoMapper;
using MediatR;
using WorkSpace.Application.Interfaces.Repositories;
using WorkSpace.Application.Interfaces.Services;
using WorkSpace.Application.Wrappers;
using WorkSpace.Domain.Entities;

namespace WorkSpace.Application.Features.Bookings.Commands;

public class CreateBookingCommandHandler : IRequestHandler<CreateBookingCommand, Response<int>>
{
    private readonly IBookingRepository _bookingRepo;
    private readonly IAvailabilityService _availability;
    private readonly IBookingPricingService _pricing;
    private readonly IMapper _mapper;
    
    
    public CreateBookingCommandHandler(IBookingRepository bookingRepo, IAvailabilityService availability, IBookingPricingService pricing, IMapper mapper)
    {
        _bookingRepo = bookingRepo;
        _availability = availability;
        _pricing = pricing;
        _mapper = mapper;
    }
    
    
    public async Task<Response<int>> Handle(CreateBookingCommand request, CancellationToken cancellationToken)
    {
        var m = request.Model;
        var available = await _availability.IsAvailableAsync(m.WorkspaceId, m.StartTimeUtc, m.EndTimeUtc, cancellationToken);
        if (!available) return new Response<int>("Time range is not available.");

        var quote = await _pricing.QuoteAsync(m.WorkspaceId, m.StartTimeUtc, m.EndTimeUtc, m.NumberOfParticipants, cancellationToken);

        var booking = new Booking
        {
            BookingCode = $"BK-{Guid.NewGuid().ToString("N")[..10].ToUpper()}",
            CustomerId = m.CustomerId,
            WorkSpaceRoomId = m.WorkspaceId,
            StartTimeUtc = m.StartTimeUtc,
            EndTimeUtc = m.EndTimeUtc,
            NumberOfParticipants = m.NumberOfParticipants,
            SpecialRequests = m.SpecialRequests,
            Currency = m.Currency,
            TotalPrice = quote.TotalPrice,
            TaxAmount = quote.TaxAmount,
            ServiceFee = quote.ServiceFee,
            FinalAmount = quote.FinalAmount,
            BookingStatusId = 1 // PendingPayment
        };

        var model = await _bookingRepo.AddAsync(booking);
        return new Response<int>(model.Id, "Booking created with PendingPayment. Complete payment to confirm.");
    }
}