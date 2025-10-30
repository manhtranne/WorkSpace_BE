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
    private readonly IPromotionService _promotionService;
    private readonly IMapper _mapper;
    
    
    public CreateBookingCommandHandler(
        IBookingRepository bookingRepo, 
        IAvailabilityService availability, 
        IBookingPricingService pricing, 
        IPromotionService promotionService,
        IMapper mapper)
    {
        _bookingRepo = bookingRepo;
        _availability = availability;
        _pricing = pricing;
        _promotionService = promotionService;
        _mapper = mapper;
    }
    
    
    public async Task<Response<int>> Handle(CreateBookingCommand request, CancellationToken cancellationToken)
    {
        var m = request.Model;
        var available = await _availability.IsAvailableAsync(m.WorkspaceId, m.StartTimeUtc, m.EndTimeUtc, cancellationToken);
        if (!available) return new Response<int>("Time range is not available.");

        var quote = await _pricing.QuoteAsync(m.WorkspaceId, m.StartTimeUtc, m.EndTimeUtc, m.NumberOfParticipants, cancellationToken);

        // Apply promotion if provided
        decimal discountAmount = 0;
        Promotion? appliedPromotion = null;
        var finalAmount = quote.FinalAmount;

        if (!string.IsNullOrWhiteSpace(m.PromotionCode))
        {
            var (isValid, discount, promotion, errorMessage) = await _promotionService
                .ValidateAndCalculateDiscountAsync(m.PromotionCode, m.CustomerId, quote.FinalAmount, cancellationToken);

            if (!isValid)
            {
                return new Response<int>(errorMessage ?? "Invalid promotion code.");
            }

            discountAmount = discount;
            appliedPromotion = promotion;
            finalAmount = quote.FinalAmount - discountAmount;

            // Ensure final amount is not negative
            if (finalAmount < 0) finalAmount = 0;
        }

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
            FinalAmount = finalAmount,
            BookingStatusId = 1 // PendingPayment
        };

        var model = await _bookingRepo.AddAsync(booking);

        // Record promotion usage if applied
        if (appliedPromotion != null)
        {
            await _promotionService.RecordPromotionUsageAsync(
                appliedPromotion.Id, 
                model.Id, 
                m.CustomerId, 
                discountAmount, 
                cancellationToken);
        }

        var message = discountAmount > 0 
            ? $"Booking created with {discountAmount:N0} VND discount applied. Complete payment to confirm."
            : "Booking created with PendingPayment. Complete payment to confirm.";

        return new Response<int>(model.Id, message);
    }
}