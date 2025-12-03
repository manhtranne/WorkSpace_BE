using MediatR;
using Microsoft.AspNetCore.Identity;
using WorkSpace.Application.DTOs.Chat;
using WorkSpace.Application.Exceptions;
using WorkSpace.Application.Interfaces.Repositories;
using WorkSpace.Application.Interfaces.Services;
using WorkSpace.Application.Wrappers;
using WorkSpace.Domain.Entities;

<<<<<<<< HEAD:src/WorkSpace.Application/Features/GuestChat/Commands/OwnerReplyToGuest/OwnerReplyToGuestCommandHandler.cs
namespace WorkSpace.Application.Features.GuestChat.Commands.OwnerReplyToGuest;

public class OwnerReplyToGuestCommandHandler : IRequestHandler<OwnerReplyToGuestCommand, Response<GuestChatMessageDto>>
========
namespace WorkSpace.Application.Features.CustomerChat.Commands.OwnerReplyToCustomer;

public class OwnerReplyToCustomerCommandHandler : IRequestHandler<OwnerReplyToCustomerCommand, Response<CustomerChatMessageDto>>
>>>>>>>> manh/future-2:src/WorkSpace.Application/Features/CustomerChat/Commands/OwnerReplyToCustomer/OwnerReplyToCustomerCommandHandler.cs
{
    private readonly ICustomerChatSessionRepository _sessionRepository;
    private readonly IGenericRepositoryAsync<CustomerChatMessage> _messageRepository;
    private readonly IDateTimeService _dateTimeService;
    private readonly UserManager<AppUser> _userManager;

<<<<<<<< HEAD:src/WorkSpace.Application/Features/GuestChat/Commands/OwnerReplyToGuest/OwnerReplyToGuestCommandHandler.cs
    public OwnerReplyToGuestCommandHandler(
        IGuestChatSessionRepository sessionRepository,
        IGenericRepositoryAsync<GuestChatMessage> messageRepository,
========
    public OwnerReplyToCustomerCommandHandler(
        ICustomerChatSessionRepository sessionRepository,
        IGenericRepositoryAsync<CustomerChatMessage> messageRepository,
>>>>>>>> manh/future-2:src/WorkSpace.Application/Features/CustomerChat/Commands/OwnerReplyToCustomer/OwnerReplyToCustomerCommandHandler.cs
        IDateTimeService dateTimeService,
        UserManager<AppUser> userManager)
    {
        _sessionRepository = sessionRepository;
        _messageRepository = messageRepository;
        _dateTimeService = dateTimeService;
        _userManager = userManager;
    }
<<<<<<<< HEAD:src/WorkSpace.Application/Features/GuestChat/Commands/OwnerReplyToGuest/OwnerReplyToGuestCommandHandler.cs
    public async Task<Response<GuestChatMessageDto>> Handle(OwnerReplyToGuestCommand request, CancellationToken cancellationToken)
========
    public async Task<Response<CustomerChatMessageDto>> Handle(OwnerReplyToCustomerCommand request, CancellationToken cancellationToken)
>>>>>>>> manh/future-2:src/WorkSpace.Application/Features/CustomerChat/Commands/OwnerReplyToCustomer/OwnerReplyToCustomerCommandHandler.cs
    {
        var session = await _sessionRepository.GetBySessionIdAsync(request.SessionId, cancellationToken);
            
        if (session == null)
        {
            throw new ApiException($"Customer chat session not found: {request.SessionId}");
        }

        var owner = await _userManager.FindByIdAsync(request.OwnerUserId.ToString());
        if (owner == null)
        {
            throw new ApiException("Owner user not found");
        }

        var ownerName = GetOwnerName(owner);
        var now = _dateTimeService.NowUtc;

        if (session.AssignedOwnerId == null)
        {
            session.AssignedOwnerId = request.OwnerUserId;
            await _sessionRepository.UpdateAsync(session, cancellationToken);
        }

        var message = new CustomerChatMessage
        {
            CustomerChatSessionId = session.Id,
            Content = request.Message,
            SenderName = ownerName,
            IsOwner = true,
            OwnerId = request.OwnerUserId,
            CreateUtc = now
        };

        await _messageRepository.AddAsync(message, cancellationToken);

        session.LastMessageAt = now;
        await _sessionRepository.UpdateAsync(session, cancellationToken);

        var dto = new CustomerChatMessageDto
        {
            Id = message.Id,
            SessionId = session.SessionId,
            SenderName = message.SenderName,
            IsOwner = message.IsOwner,
            Content = message.Content,
            SentAt = message.CreateUtc
        };

<<<<<<<< HEAD:src/WorkSpace.Application/Features/GuestChat/Commands/OwnerReplyToGuest/OwnerReplyToGuestCommandHandler.cs
        return new Response<GuestChatMessageDto>(dto, "Owner reply sent successfully");
========
        return new Response<CustomerChatMessageDto>(dto, "Owner reply sent successfully");
>>>>>>>> manh/future-2:src/WorkSpace.Application/Features/CustomerChat/Commands/OwnerReplyToCustomer/OwnerReplyToCustomerCommandHandler.cs
    }
    
    private static string GetOwnerName(AppUser user)
    {
        var fullName = $"{user.FirstName ?? string.Empty} {user.LastName ?? string.Empty}".Trim();
        return string.IsNullOrWhiteSpace(fullName) ? user.UserName ?? "Owner" : fullName;
    }
}

<<<<<<<< HEAD:src/WorkSpace.Application/Features/GuestChat/Commands/OwnerReplyToGuest/OwnerReplyToGuestCommandHandler.cs
========

>>>>>>>> manh/future-2:src/WorkSpace.Application/Features/CustomerChat/Commands/OwnerReplyToCustomer/OwnerReplyToCustomerCommandHandler.cs
