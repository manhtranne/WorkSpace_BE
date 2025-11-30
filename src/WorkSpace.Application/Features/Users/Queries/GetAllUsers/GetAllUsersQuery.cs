using MediatR;
using WorkSpace.Application.DTOs.Users;
using System.Collections.Generic;

using Microsoft.EntityFrameworkCore;

namespace WorkSpace.Application.Features.Users.Queries.GetAllUsers
{
    public class GetAllUsersQuery : IRequest<IEnumerable<UserDto>>
    {
        public string? SearchTerm { get; set; }
    }

    public class GetAllUsersQueryHandler : IRequestHandler<GetAllUsersQuery, IEnumerable<UserDto>>
    {
  
        private readonly Microsoft.AspNetCore.Identity.UserManager<global::WorkSpace.Domain.Entities.AppUser> _userManager;
        private readonly AutoMapper.IMapper _mapper;

        public GetAllUsersQueryHandler(
            Microsoft.AspNetCore.Identity.UserManager<global::WorkSpace.Domain.Entities.AppUser> userManager,
            AutoMapper.IMapper mapper)
        {
            _userManager = userManager;
            _mapper = mapper;
        }

        public async Task<IEnumerable<UserDto>> Handle(GetAllUsersQuery request, CancellationToken cancellationToken)
        {
            var query = _userManager.Users.AsQueryable();

            if (!string.IsNullOrWhiteSpace(request.SearchTerm))
            {
                query = query.Where(u => u.UserName.Contains(request.SearchTerm) || u.Email.Contains(request.SearchTerm));
            }

         
            var users = await query.ToListAsync(cancellationToken);

            var userDtos = new List<UserDto>();

            foreach (var user in users)
            {
                var userDto = _mapper.Map<UserDto>(user);
                userDto.Roles = (await _userManager.GetRolesAsync(user)).ToList();
                userDtos.Add(userDto);
            }

            return userDtos;
        }
    }
}