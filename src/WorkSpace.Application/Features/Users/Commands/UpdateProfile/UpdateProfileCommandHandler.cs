using MediatR;
using WorkSpace.Application.Interfaces.Services;
using WorkSpace.Application.Wrappers;

namespace WorkSpace.Application.Features.Users.Commands.UpdateProfile
{
    public class UpdateProfileCommandHandler : IRequestHandler<UpdateProfileCommand, Response<string>>
    {
        private readonly IAccountService _accountService;

        public UpdateProfileCommandHandler(IAccountService accountService)
        {
            _accountService = accountService;
        }

        public async Task<Response<string>> Handle(UpdateProfileCommand request, CancellationToken cancellationToken)
        {
            return await _accountService.UpdateProfileAsync(request.UserId, request.UpdateRequest);
        }
    }
}