using MediatR;
using WorkSpace.Application.DTOs.Users;
using WorkSpace.Application.Exceptions;
using System.Linq;
using WorkSpace.Domain.Entities;

namespace WorkSpace.Application.Features.Users.Queries.GetUserById
{
    public class GetUserByIdQuery : IRequest<UserDto>
    {
        public int Id { get; set; }
    }

    public class GetUserByIdQueryHandler : IRequestHandler<GetUserByIdQuery, UserDto>
    {
        
        private readonly Microsoft.AspNetCore.Identity.UserManager<AppUser> _userManager;
        private readonly AutoMapper.IMapper _mapper;

        public GetUserByIdQueryHandler(
            Microsoft.AspNetCore.Identity.UserManager<AppUser> userManager,
            AutoMapper.IMapper mapper)
        {
            _userManager = userManager;
            _mapper = mapper;
        }

        public async Task<UserDto> Handle(GetUserByIdQuery request, CancellationToken cancellationToken)
        {
            var user = await _userManager.FindByIdAsync(request.Id.ToString());
            if (user == null)
            {
                throw new ApiException($"User with id {request.Id} not found.");
            }

            var userDto = _mapper.Map<UserDto>(user);
            userDto.Roles = (await _userManager.GetRolesAsync(user)).ToList();

            return userDto;
        }
    }
}