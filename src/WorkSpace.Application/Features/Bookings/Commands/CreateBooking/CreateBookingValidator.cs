using FluentValidation;

namespace WorkSpace.Application.Features.Bookings.Commands;

public class CreateBookingValidator : AbstractValidator<CreateBookingCommand>
{
    public CreateBookingValidator()
    {
        RuleFor(x => x.Model.CustomerId).GreaterThan(0);
        RuleFor(x => x.Model.WorkspaceId).GreaterThan(0);
        RuleFor(x => x.Model.NumberOfParticipants).GreaterThan(0);
        RuleFor(x => x.Model.EndTimeUtc)
            .GreaterThan(x => x.Model.StartTimeUtc)
            .WithMessage("EndTime must be after StartTime.");
        RuleFor(x => x.Model.StartTimeUtc.Offset).Equal(TimeSpan.Zero)
            .WithMessage("Use UTC (Offset must be 0).");
        RuleFor(x => x.Model.EndTimeUtc.Offset).Equal(TimeSpan.Zero)
            .WithMessage("Use UTC (Offset must be 0).");
    }
}