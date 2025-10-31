﻿using FluentValidation;

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
        
        // Validate customer info if provided
        When(x => !string.IsNullOrWhiteSpace(x.Model.FirstName), () =>
        {
            RuleFor(x => x.Model.FirstName).MaximumLength(100).WithMessage("FirstName cannot exceed 100 characters.");
        });
        
        When(x => !string.IsNullOrWhiteSpace(x.Model.LastName), () =>
        {
            RuleFor(x => x.Model.LastName).MaximumLength(100).WithMessage("LastName cannot exceed 100 characters.");
        });
        
        When(x => !string.IsNullOrWhiteSpace(x.Model.PhoneNumber), () =>
        {
            RuleFor(x => x.Model.PhoneNumber)
                .Matches(@"^[0-9+\-() ]+$")
                .WithMessage("PhoneNumber must contain only numbers and valid characters (+, -, (), space).");
        });
    }
}