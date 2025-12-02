using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WorkSpace.Application.DTOs.Account;
using WorkSpace.Application.Enums;
using WorkSpace.Application.Interfaces.Services;

namespace WorkSpace.WebApi.Controllers.v1;

[Route("api/v1/admin")]
[Authorize(Roles = nameof(Roles.Admin))]
[ApiController]
public class AdminController : BaseApiController
{
    private readonly IAccountService _accountService;

    public AdminController(IAccountService accountService)
    {
        _accountService = accountService;
    }

  
    [HttpGet("users")]
    public async Task<IActionResult> GetAllUsers([FromQuery] GetAllUsersRequestDto request)
    {
        var result = await _accountService.GetAllUsersAsync(request);
        return Ok(result);
    }


    [HttpGet("users/{userId}")]
    public async Task<IActionResult> GetUserById([FromRoute] int userId)
    {
        var result = await _accountService.GetUserByIdAsync(userId);
        return Ok(result);
    }


    [HttpPost("users")]
    public async Task<IActionResult> CreateUser([FromBody] CreateUserByAdminDto request)
    {
        var result = await _accountService.CreateUserByAdminAsync(request);
        return Ok(result);
    }

    [HttpPut("users/{userId}/status")]
    public async Task<IActionResult> UpdateUserStatus(
        [FromRoute] int userId,
        [FromBody] UpdateUserStatusDto request)
    {
        var result = await _accountService.UpdateUserStatusAsync(userId, request);
        return Ok(result);
    }
}

