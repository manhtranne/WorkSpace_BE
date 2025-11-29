using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WorkSpace.Application.DTOs.Users;
using WorkSpace.Application.Features.Users.Commands.CreateUser;
using WorkSpace.Application.Features.Users.Commands.DeleteUser;
using WorkSpace.Application.Features.Users.Commands.UpdateUser;
using WorkSpace.Application.Features.Users.Queries.GetAllUsers;
using WorkSpace.Application.Features.Users.Queries.GetUserById;
using WorkSpace.Application.Enums;
using WorkSpace.Application.Extensions;

namespace WorkSpace.WebApi.Controllers.v1
{
    [Route("api/v1/users")]
    [ApiController]
    [Authorize(Roles = $"{nameof(Roles.Admin)},{nameof(Roles.Staff)}")]
    public class UsersController : BaseApiController
    {

        [HttpGet]
        public async Task<IActionResult> GetListUsers([FromQuery] string? searchTerm)
        {
       
            var result = await Mediator.Send(new GetAllUsersQuery { SearchTerm = searchTerm });
            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetUserDetail(int id)
        {
          
            var result = await Mediator.Send(new GetUserByIdQuery { Id = id });
            return Ok(result);
        }


        [HttpPost]
        public async Task<IActionResult> CreateNewUser([FromBody] CreateUserRequest request)
        {
            return Ok(await Mediator.Send(new CreateUserCommand { CreateUserRequest = request }));
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateUserInfo(int id, [FromBody] UpdateUserRequest request)
        {
            return Ok(await Mediator.Send(new UpdateUserCommand { Id = id, UpdateUserRequest = request }));
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUserAccount(int id)
        {
            return Ok(await Mediator.Send(new DeleteUserCommand { Id = id }));
        }
    }
}