using MediatR;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using WorkSpace.Application.DTOs.Chat;
using WorkSpace.Application.Interfaces.Repositories;
// using WorkSpace.Application.Wrappers; // Bỏ wrapper

namespace WorkSpace.Application.Features.GuestChat.Queries.GetActiveGuestSessions;

// Thay đổi interface implement
public class GetActiveGuestSessionsQueryHandler : IRequestHandler<GetActiveGuestSessionsQuery, IEnumerable<GuestChatSessionDto>>
{
    private readonly IGuestChatSessionRepository _sessionRepository;

    public GetActiveGuestSessionsQueryHandler(IGuestChatSessionRepository sessionRepository)
    {
        _sessionRepository = sessionRepository;
    }

    public async Task<IEnumerable<GuestChatSessionDto>> Handle(GetActiveGuestSessionsQuery request, CancellationToken cancellationToken)
    {
        List<Domain.Entities.GuestChatSession> sessions;

        if (request.StaffId.HasValue)
        {
            sessions = await _sessionRepository.GetSessionsByStaffIdAsync(request.StaffId.Value, cancellationToken);
        }
        else
        {
            sessions = await _sessionRepository.GetActiveSessionsAsync(cancellationToken);
        }

        var dtos = sessions.Select(s => new GuestChatSessionDto
        {
            SessionId = s.SessionId,
            GuestName = s.GuestName,
            GuestEmail = s.GuestEmail,
            CreatedAt = s.CreateUtc,
            LastMessageAt = s.LastMessageAt,
            IsActive = s.IsActive,
            AssignedStaffId = s.AssignedStaffId,
            AssignedStaffName = s.AssignedStaff?.GetFullName()
        }).ToList();


        return dtos;
    }
}